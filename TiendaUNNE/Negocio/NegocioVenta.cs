using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TiendaUNNE
{
    /// <summary>
    /// Reglas de la venta en caja: qué se puede agregar al ticket, cuánto stock hay,
    /// si los pagos alcanzan y cuánto vuelto corresponde.
    /// El cobro se guarda en la base (venta, renglones, pagos y movimientos de caja) en una
    /// sola transacción. Todavía no descuenta stock.
    /// </summary>
    public static class NegocioVenta
    {
        public const decimal CantidadMaxima = 9999m;

        public const string FormatoImporte = "N2";
        public const string FormatoCantidad = "N3";

        /// <summary>El número de ticket se muestra con ceros a la izquierda: 00000012.</summary>
        public const string FormatoNumeroTicket = "D8";

        /// <summary>Código del tipo de comprobante que se emite al cobrar (Tipo_comprobante).</summary>
        private const string CodigoTicket = "TKT";

        private const string ConceptoCobro = "Cobro de venta";

        /// <summary>Como FormatoCantidad pero sin ceros de más: 1 se ve "1", no "1,000".</summary>
        public const string FormatoCantidadTicket = "#,##0.###";

        // ---------------------------------------------------------------------
        // Armado del ticket
        // ---------------------------------------------------------------------

        public static VentaEditModel NuevaVenta(CajaSesion sesion, int idUsuario)
        {
            if (sesion == null)
                throw new ReglaNegocioException("Hay que abrir la caja antes de vender.");

            return new VentaEditModel
            {
                IdCaja = sesion.IdCaja,
                IdCajaSesion = sesion.IdCajaSesion,
                IdUsuario = idUsuario
            };
        }

        /// <summary>Busca en el catálogo de productos (solo lectura, no modifica nada).</summary>
        public static List<ProductoVentaItem> BuscarProductos(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return new List<ProductoVentaItem>();

            return ServicioProducto.BuscarParaVenta(texto.Trim());
        }

        /// <summary>
        /// Agrega un producto al ticket. Si ya estaba cargado suma la cantidad al
        /// renglón existente en vez de repetirlo, y valida el stock sobre el total.
        /// </summary>
        public static void AgregarProducto(VentaEditModel venta, ProductoVentaItem producto, decimal cantidad)
        {
            if (venta == null) throw new ArgumentNullException(nameof(venta));

            if (producto == null)
                throw new ReglaNegocioException("Elegí un producto de la lista.");

            if (cantidad <= 0)
                throw new ReglaNegocioException("La cantidad tiene que ser mayor a cero.");

            if (cantidad > CantidadMaxima)
                throw new ReglaNegocioException("La cantidad supera el máximo permitido.");

            RenglonVenta existente = venta.Renglones
                .FirstOrDefault(r => r.IdProducto == producto.IdProducto);

            decimal cantidadFinal = cantidad + (existente == null ? 0 : existente.Cantidad);

            if (cantidadFinal > producto.Stock)
                throw new ReglaNegocioException(string.Format(CultureInfo.CurrentCulture,
                    "No hay stock suficiente de {0}. Disponible: {1}.",
                    producto.Nombre, producto.Stock.ToString(FormatoCantidad)));

            if (existente != null)
            {
                existente.Cantidad = cantidadFinal;
                return;
            }

            venta.Renglones.Add(new RenglonVenta
            {
                IdProducto = producto.IdProducto,
                Descripcion = producto.Nombre,
                Cantidad = cantidad,
                PrecioUnitario = producto.PrecioVenta
            });
        }

        public static void QuitarRenglon(VentaEditModel venta, int indice)
        {
            if (venta == null) throw new ArgumentNullException(nameof(venta));

            if (indice < 0 || indice >= venta.Renglones.Count)
                throw new ReglaNegocioException("Elegí un renglón del ticket para quitar.");

            venta.Renglones.RemoveAt(indice);
        }

        // ---------------------------------------------------------------------
        // Cobro
        // ---------------------------------------------------------------------

        public static List<MedioPagoItem> ListarMediosDePago() => ServicioMedioPago.ListarActivos();

        /// <summary>Lo que hay que devolverle al cliente si pagó de más.</summary>
        public static decimal CalcularVuelto(VentaEditModel venta)
        {
            if (venta == null) return 0;

            decimal excedente = venta.TotalPagado - venta.Total;
            return excedente > 0 ? excedente : 0;
        }

        /// <summary>Lo que todavía falta cobrar.</summary>
        public static decimal CalcularRestante(VentaEditModel venta)
        {
            if (venta == null) return 0;

            decimal restante = venta.Total - venta.TotalPagado;
            return restante > 0 ? restante : 0;
        }

        /// <summary>
        /// Valida el cobro y lo guarda en la base: la venta, sus renglones y los pagos, con el
        /// vuelto ya descontado del efectivo. Devuelve el número de ticket asignado.
        /// Si no se puede guardar, la venta queda como estaba y se puede reintentar.
        /// </summary>
        public static long ConfirmarVenta(VentaEditModel venta)
        {
            ValidarParaCobrar(venta);
            ValidarCajaAbierta(venta);

            int idTipoTicket = ServicioVenta.ObtenerIdTipoComprobante(CodigoTicket)
                ?? throw new ReglaNegocioException(
                    "No está cargado el tipo de comprobante «Ticket» en la base de datos.");

            List<PagoVenta> pagos = PagosSinVuelto(venta);

            // El punto de venta es la caja: cada caja numera sus tickets desde 1.
            return ServicioVenta.Registrar(venta, pagos, idTipoTicket, venta.IdCaja, ConceptoCobro);
        }

        /// <summary>No se cobra contra un turno que otro puesto ya cerró.</summary>
        private static void ValidarCajaAbierta(VentaEditModel venta)
        {
            CajaSesion abierta = NegocioCaja.ObtenerSesionAbierta();
            if (abierta == null || abierta.IdCajaSesion != venta.IdCajaSesion)
                throw new ReglaNegocioException("La caja de esta venta ya no está abierta.");
        }

        private static void ValidarParaCobrar(VentaEditModel venta)
        {
            if (venta == null) throw new ArgumentNullException(nameof(venta));

            if (venta.EstaVacia)
                throw new ReglaNegocioException("El ticket está vacío.");

            if (venta.Total <= 0)
                throw new ReglaNegocioException("El total de la venta tiene que ser mayor a cero.");

            if (venta.Pagos.Count == 0)
                throw new ReglaNegocioException("Cargá al menos un medio de pago.");

            if (venta.Pagos.Any(p => p.Importe <= 0))
                throw new ReglaNegocioException("Los importes de los pagos tienen que ser mayores a cero.");

            if (venta.TotalPagado < venta.Total)
                throw new ReglaNegocioException(string.Format(CultureInfo.CurrentCulture,
                    "Falta cobrar {0}.", CalcularRestante(venta).ToString(FormatoImporte)));

            // Solo se puede dar vuelto de lo que se pagó en efectivo: no se devuelve
            // plata de una transferencia o de una tarjeta.
            if (CalcularVuelto(venta) > venta.EfectivoRecibido)
                throw new ReglaNegocioException(
                    "El pago supera el total y no alcanza el efectivo para dar el vuelto. " +
                    "Ajustá los importes.");
        }

        /// <summary>
        /// Copia de los pagos con el vuelto bajado del efectivo, para que sumen exactamente el
        /// total: en el cajón queda lo cobrado, no lo que entregó el cliente. La venta no se
        /// modifica, así un cobro que falla se puede reintentar tal cual estaba.
        /// </summary>
        private static List<PagoVenta> PagosSinVuelto(VentaEditModel venta)
        {
            List<PagoVenta> pagos = venta.Pagos.Select(p => new PagoVenta
            {
                IdMedioPago = p.IdMedioPago,
                NombreMedioPago = p.NombreMedioPago,
                EsEfectivo = p.EsEfectivo,
                Importe = p.Importe,
                Referencia = p.Referencia
            }).ToList();

            decimal vuelto = CalcularVuelto(venta);

            foreach (PagoVenta pago in pagos.Where(p => p.EsEfectivo))
            {
                if (vuelto <= 0) break;

                decimal aDescontar = Math.Min(pago.Importe, vuelto);
                pago.Importe -= aDescontar;
                vuelto -= aDescontar;
            }

            pagos.RemoveAll(p => p.Importe <= 0);
            return pagos;
        }
    }
}
