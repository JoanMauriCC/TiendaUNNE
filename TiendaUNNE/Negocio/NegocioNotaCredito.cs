using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TiendaUNNE
{
    /// <summary>
    /// Reglas de la nota de crédito: el comprobante con el que se corrige o se devuelve una venta ya
    /// emitida. No se borra ni se modifica la venta: se emite otro comprobante que la compensa.
    ///   - Solo el Administrador y el Supervisor las emiten (devuelven plata y stock).
    ///   - Se puede devolver todo o solo algunos productos y unidades, sin pasarse de lo vendido ni
    ///     de lo que ya se devolvió con otras notas de la misma venta.
    ///   - Corresponde Nota de crédito A a una Factura A, y B a una Factura B o a un Ticket.
    ///   - A una nota de crédito no se le hace otra nota de crédito.
    ///   - La plata sale de la caja que está abierta, por el medio de pago que se elija, y el stock vuelve al depósito.
    ///   - Hay que escribir el motivo; queda guardado y en la Auditoría.
    /// Los reportes y el arqueo restan las notas de crédito porque su tipo de comprobante tiene signo -1.
    /// </summary>
    public static class NegocioNotaCredito
    {
        public const int LargoMinimoMotivo = 5;
        public const int LargoMaximoMotivo = 300;

        public static bool PuedeEmitir => SesionActual.EsAdministrador || SesionActual.EsSupervisor;

        private static void ExigirPermiso()
        {
            if (!PuedeEmitir)
                throw new ReglaNegocioException("Solo un Administrador o un Supervisor puede emitir notas de crédito.");
        }

        // ---------------------------------------------------------------------
        // Preparar: qué se puede devolver de una venta
        // ---------------------------------------------------------------------

        /// <summary>
        /// La venta con lo que todavía se puede devolver de cada renglón. Si no se puede emitir una
        /// nota de crédito sobre ella (no existe, es otra nota, está anulada o ya se devolvió todo),
        /// explica por qué.
        /// </summary>
        public static VentaADevolver Preparar(int idVenta)
        {
            ExigirPermiso();

            VentaADevolver venta = ServicioVenta.ObtenerADevolver(idVenta)
                ?? throw new ReglaNegocioException("Esa venta ya no existe.");

            if (venta.Signo != 1)
                throw new ReglaNegocioException("Eso ya es una nota de crédito: no se le puede hacer otra.");

            if (!string.Equals(venta.Estado, "EMITIDA", StringComparison.OrdinalIgnoreCase))
                throw new ReglaNegocioException("Esa venta figura como anulada: no admite nota de crédito.");

            if (!venta.HayAlgoParaDevolver)
                throw new ReglaNegocioException(
                    "De esa venta ya se devolvió todo con notas de crédito anteriores.");

            return venta;
        }

        /// <summary>Nombre del comprobante que se va a emitir para esa venta (ej. «Nota de crédito A»).</summary>
        public static string NombreDeLaNota(VentaADevolver venta)
        {
            return NegocioComprobante.Buscar(NegocioComprobante.NotaCreditoPara(venta.CodigoTipo)).Nombre;
        }

        /// <summary>Lo que se devolvería con las cantidades cargadas hasta ahora (para mostrar el total en pantalla).</summary>
        public static decimal TotalADevolver(VentaADevolver venta)
        {
            return decimal.Round(venta.Renglones.Sum(r => r.ADevolver * r.PrecioUnitario), 2);
        }

        // ---------------------------------------------------------------------
        // Emitir
        // ---------------------------------------------------------------------

        /// <summary>
        /// Emite la nota de crédito con las cantidades cargadas en <see cref="LineaDevolvible.ADevolver"/>.
        /// Devuelve el comprobante emitido, listo para mostrarlo y guardarlo en PDF.
        /// </summary>
        public static ComprobanteVista Emitir(VentaADevolver venta, int idMedioPago, string motivo)
        {
            ExigirPermiso();
            if (venta == null) throw new ArgumentNullException(nameof(venta));

            // ---- Qué se devuelve
            foreach (LineaDevolvible r in venta.Renglones)
            {
                if (r.ADevolver < 0)
                    throw new ReglaNegocioException("Las cantidades a devolver no pueden ser negativas.");

                if (r.ADevolver > r.Disponible)
                    throw new ReglaNegocioException(string.Format(CultureInfo.CurrentCulture,
                        "De «{0}» solo se puede devolver {1}.",
                        r.Descripcion, r.Disponible.ToString(NegocioVenta.FormatoCantidadTicket)));
            }

            List<LineaDevolvible> aDevolver = venta.Renglones.Where(r => r.ADevolver > 0).ToList();
            if (aDevolver.Count == 0)
                throw new ReglaNegocioException("Elegí al menos un producto para devolver.");

            // ---- Motivo
            string razon = Validaciones.NormalizarEspacios(motivo);
            if (string.IsNullOrEmpty(razon))
                throw new ReglaNegocioException("Escribí el motivo de la nota de crédito.");
            if (razon.Length < LargoMinimoMotivo)
                throw new ReglaNegocioException("El motivo es muy corto: explicá brevemente por qué se devuelve.");
            if (razon.Length > LargoMaximoMotivo)
                throw new ReglaNegocioException("El motivo no puede superar los " + LargoMaximoMotivo + " caracteres.");

            // ---- Caja y medio de pago
            CajaSesion sesion = NegocioCaja.ObtenerSesionAbierta();
            if (sesion == null)
                throw new ReglaNegocioException("Abrí la caja para emitir una nota de crédito: la devolución sale del cajón.");

            MedioPagoItem medio = NegocioVenta.ListarMediosDePago().FirstOrDefault(m => m.Id == idMedioPago)
                ?? throw new ReglaNegocioException("Elegí con qué medio de pago se devuelve el dinero.");

            // ---- Armado de la nota
            string codigo = NegocioComprobante.NotaCreditoPara(venta.CodigoTipo);
            TipoComprobanteItem tipo = NegocioComprobante.Buscar(codigo);

            int idTipo = ServicioVenta.ObtenerIdTipoComprobante(codigo)
                ?? throw new ReglaNegocioException(
                    "No está cargado el tipo de comprobante «" + tipo.Nombre + "» en la base de datos.");

            UsuarioLogueado quien = SesionActual.Usuario;

            var nota = new NotaCreditoEdit
            {
                IdVentaOriginal = venta.IdVenta,
                CodigoTipo = codigo,
                IdCaja = sesion.IdCaja,
                IdCajaSesion = sesion.IdCajaSesion,
                IdUsuario = quien.IdUsuario,
                IdMedioPago = medio.Id,
                Motivo = razon
            };

            decimal neto = 0, iva = 0;
            foreach (LineaDevolvible r in aDevolver)
            {
                var renglon = new RenglonVenta
                {
                    IdProducto = r.IdProducto,
                    Descripcion = r.Descripcion,
                    Cantidad = r.ADevolver,
                    PrecioUnitario = r.PrecioUnitario,
                    AlicuotaIva = r.AlicuotaIva
                };

                // El IVA se discrimina igual que en la venta original: un renglón sin IVA (Ticket) lo devuelve sin IVA.
                if (r.AlicuotaIva > 0)
                {
                    decimal netoRenglon = decimal.Round(
                        renglon.Subtotal / (1 + r.AlicuotaIva / 100m), 2, MidpointRounding.AwayFromZero);
                    renglon.ImporteIva = renglon.Subtotal - netoRenglon;
                    neto += netoRenglon;
                    iva += renglon.ImporteIva;
                }
                else
                {
                    neto += renglon.Subtotal;
                }

                nota.Renglones.Add(renglon);
            }
            nota.ImporteNeto = neto;
            nota.ImporteIva = iva;

            string original = venta.NombreTipo + " " + NegocioComprobante.FormatearNumero(venta.PuntoVenta, venta.Numero);

            string resumen = string.Format(CultureInfo.CurrentCulture,
                "{0} sobre {1}; cliente = {2}; total devuelto = $ {3} por {4}; stock devuelto al depósito; emitida por {5} ({6}); motivo: {7}",
                tipo.Nombre, original, venta.ClienteNombre ?? NegocioCliente.ConsumidorFinal,
                nota.Total.ToString(NegocioVenta.FormatoImporte), medio.Nombre,
                quien.NombreCompleto, quien.Rol, razon);

            // ---- Guardado
            VentaRegistrada registrada;
            try
            {
                // Como en las ventas, el punto de venta es la caja: cada caja numera desde 1.
                registrada = ServicioVenta.RegistrarNotaCredito(nota, idTipo, sesion.IdCaja, resumen);
            }
            catch (NotaCreditoInvalidaException ex)
            {
                throw Traducir(ex, venta);
            }

            return NegocioHistorial.ObtenerComprobante(registrada.IdVenta, esCopia: false);
        }

        private static ReglaNegocioException Traducir(NotaCreditoInvalidaException ex, VentaADevolver venta)
        {
            switch (ex.Causa)
            {
                case CausaNotaCredito.CantidadExcedida:
                    string producto = venta.Renglones
                        .Where(r => r.IdProducto == ex.IdProducto)
                        .Select(r => r.Descripcion)
                        .FirstOrDefault() ?? "un producto";

                    return new ReglaNegocioException(string.Format(CultureInfo.CurrentCulture,
                        "De «{0}» ya no se puede devolver esa cantidad: quedan {1}. Alguien emitió otra nota de crédito antes. Volvé a abrir la venta.",
                        producto, ex.Disponible.ToString(NegocioVenta.FormatoCantidadTicket)));

                case CausaNotaCredito.NoSeAdmiteNotaDeCredito:
                    return new ReglaNegocioException("Esa venta no admite nota de crédito (es otra nota o está anulada).");

                default:
                    return new ReglaNegocioException("Esa venta ya no existe.");
            }
        }
    }
}
