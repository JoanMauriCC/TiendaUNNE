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
    /// sola transacción, que también descuenta el stock vendido.
    /// Cada producto cargado en el ticket queda reservado (30 minutos, renovables) para que otra
    /// caja o la tienda online no lo venda mientras tanto.
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

        /// <summary>
        /// Catálogo completo de productos activos (solo lectura). El stock de cada uno es el libre
        /// para este ticket: descuenta lo que tienen reservado los demás tickets.
        /// </summary>
        public static List<ProductoVentaItem> ListarCatalogo(VentaEditModel venta)
        {
            if (venta == null) throw new ArgumentNullException(nameof(venta));

            return ServicioProducto.ListarParaVenta(venta.Token);
        }

        /// <summary>Categorías que tienen algún producto en el catálogo, en orden alfabético.</summary>
        public static List<string> CategoriasDelCatalogo(IEnumerable<ProductoVentaItem> catalogo)
        {
            return catalogo.Select(p => p.Categoria)
                .Distinct(StringComparer.CurrentCultureIgnoreCase)
                .OrderBy(c => c, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Productos que se muestran con el texto y la categoría elegidos. <paramref name="categoria"/>
        /// en null muestra todas. El texto no distingue mayúsculas ni tildes.
        /// </summary>
        public static List<ProductoVentaItem> FiltrarCatalogo(
            IEnumerable<ProductoVentaItem> catalogo, VentaEditModel venta,
            string texto, string categoria, bool ocultarSinStock)
        {
            string buscado = Normalizar(texto);

            return catalogo
                .Where(p => categoria == null ||
                            string.Equals(p.Categoria, categoria, StringComparison.CurrentCultureIgnoreCase))
                .Where(p => !ocultarSinStock || Disponible(venta, p) > 0)
                .Where(p => buscado.Length == 0 || Normalizar(p.Nombre).Contains(buscado))
                .ToList();
        }

        /// <summary>Lo que todavía se puede agregar de ese producto: su stock libre menos lo ya cargado en el ticket.</summary>
        public static decimal Disponible(VentaEditModel venta, ProductoVentaItem producto)
        {
            RenglonVenta renglon = venta == null
                ? null
                : venta.Renglones.FirstOrDefault(r => r.IdProducto == producto.IdProducto);

            decimal libre = producto.Stock - (renglon == null ? 0 : renglon.Cantidad);
            return libre > 0 ? libre : 0;
        }

        /// <summary>Quita una unidad del renglón; si era la última, lo saca del ticket.</summary>
        public static void RestarUnaUnidad(VentaEditModel venta, int indice)
        {
            if (venta == null) throw new ArgumentNullException(nameof(venta));

            if (indice < 0 || indice >= venta.Renglones.Count)
                throw new ReglaNegocioException("Elegí un renglón del ticket.");

            RenglonVenta renglon = venta.Renglones[indice];

            if (renglon.Cantidad <= 1)
            {
                QuitarRenglon(venta, indice);
                return;
            }

            ServicioReserva.Reservar(venta.Token, renglon.IdProducto, renglon.Cantidad - 1, venta.IdUsuario);
            renglon.Cantidad -= 1;
        }

        private static string Normalizar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

            string descompuesto = texto.Trim().ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder(descompuesto.Length);
            foreach (char c in descompuesto)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            return sb.ToString();
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

            // La reserva es la que decide: si otra caja se llevó el stock después de la búsqueda, falla acá.
            try
            {
                ServicioReserva.Reservar(venta.Token, producto.IdProducto, cantidadFinal, venta.IdUsuario);
            }
            catch (StockInsuficienteException ex)
            {
                throw new ReglaNegocioException(string.Format(CultureInfo.CurrentCulture,
                    "No hay stock suficiente de {0}. Disponible: {1}.",
                    producto.Nombre, ex.Disponible.ToString(FormatoCantidad)));
            }

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

            int idProducto = venta.Renglones[indice].IdProducto;
            venta.Renglones.RemoveAt(indice);

            ServicioReserva.Liberar(venta.Token, idProducto);
        }

        /// <summary>
        /// Suelta las reservas del ticket (al cancelarlo o cerrar la pantalla). Si no se puede,
        /// no pasa nada grave: las reservas vencen solas a los 30 minutos.
        /// </summary>
        public static void LiberarReservas(VentaEditModel venta)
        {
            if (venta == null || venta.EstaVacia) return;

            try { ServicioReserva.LiberarTodas(venta.Token); }
            catch (Exception) { }
        }

        /// <summary>Mantiene vigentes las reservas mientras el ticket se sigue usando.</summary>
        public static void RenovarReservas(VentaEditModel venta)
        {
            if (venta == null || venta.EstaVacia) return;

            try { ServicioReserva.Renovar(venta.Token); }
            catch (Exception) { }
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
            try
            {
                return ServicioVenta.Registrar(venta, pagos, idTipoTicket, venta.IdCaja, ConceptoCobro);
            }
            catch (StockInsuficienteException ex)
            {
                string nombre = venta.Renglones
                    .Where(r => r.IdProducto == ex.IdProducto)
                    .Select(r => r.Descripcion)
                    .FirstOrDefault() ?? "un producto del ticket";

                throw new ReglaNegocioException(string.Format(CultureInfo.CurrentCulture,
                    "No hay stock suficiente de {0}. Disponible: {1}. Ajustá el ticket y volvé a cobrar.",
                    nombre, ex.Disponible.ToString(FormatoCantidad)));
            }
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
