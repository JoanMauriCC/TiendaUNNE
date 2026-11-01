using System;
using System.Data;
using System.Globalization;

namespace TiendaUNNE
{
    /// <summary>
    /// Reglas para anular una venta ya cobrada:
    ///   - solo el Administrador y el Supervisor pueden anular (el Cajero puede ver la lista);
    ///   - solo se anulan ventas del turno de caja que está abierto: una venta de un turno ya
    ///     cerrado desarmaría el cierre que se hizo, y para eso existe la nota de crédito;
    ///   - hay que escribir el motivo, y queda guardado en la venta y en la Auditoría;
    ///   - una venta anulada no se puede anular otra vez.
    /// Al anular, el stock vuelve al depósito y la devolución queda como egreso de caja
    /// (lo hace sp_AnularVenta). Los reportes y el arqueo ya ignoran las ventas anuladas.
    /// </summary>
    public static class NegocioAnulacion
    {
        public const string EstadoEmitida = "EMITIDA";
        public const string EstadoAnulada = "ANULADA";

        public const int LargoMinimoMotivo = 5;
        public const int LargoMaximoMotivo = 300;

        public static bool PuedeAnular => SesionActual.EsAdministrador || SesionActual.EsSupervisor;

        /// <summary>Las ventas del turno para la grilla, con el número ya con su formato.</summary>
        public static DataTable ListarDelTurno(CajaSesion sesion)
        {
            if (sesion == null)
                throw new ReglaNegocioException("Hay que abrir la caja para ver las ventas del turno.");

            var dt = new DataTable();
            dt.Columns.Add("IdVenta", typeof(int));
            dt.Columns.Add("Fecha", typeof(DateTime));
            dt.Columns.Add("Comprobante", typeof(string));
            dt.Columns.Add("Numero", typeof(string));
            dt.Columns.Add("Cliente", typeof(string));
            dt.Columns.Add("Total", typeof(decimal));
            dt.Columns.Add("Estado", typeof(string));
            dt.Columns.Add("Motivo", typeof(string));

            foreach (VentaListada v in ServicioVenta.ListarDelTurno(sesion.IdCajaSesion))
            {
                dt.Rows.Add(v.IdVenta, v.Fecha, v.Comprobante,
                    NegocioComprobante.FormatearNumero(v.PuntoVenta, v.Numero),
                    v.Cliente, v.Total, v.Estado, (object)v.Motivo ?? DBNull.Value);
            }

            return dt;
        }

        /// <summary>Anula la venta con el motivo indicado. Si no se puede, explica por qué.</summary>
        public static void Anular(int idVenta, string motivo)
        {
            if (!PuedeAnular)
                throw new ReglaNegocioException("Solo un Administrador o un Supervisor puede anular ventas.");

            string razon = Validaciones.NormalizarEspacios(motivo);
            if (string.IsNullOrEmpty(razon))
                throw new ReglaNegocioException("Escribí el motivo de la anulación.");
            if (razon.Length < LargoMinimoMotivo)
                throw new ReglaNegocioException("El motivo es muy corto: explicá brevemente por qué se anula.");
            if (razon.Length > LargoMaximoMotivo)
                throw new ReglaNegocioException("El motivo no puede superar los " + LargoMaximoMotivo + " caracteres.");

            CajaSesion sesion = NegocioCaja.ObtenerSesionAbierta();
            if (sesion == null)
                throw new ReglaNegocioException("Abrí la caja para anular una venta: la devolución sale del cajón.");

            VentaListada venta = ServicioVenta.ObtenerVenta(idVenta)
                ?? throw new ReglaNegocioException("La venta ya no existe.");

            UsuarioLogueado quien = SesionActual.Usuario;

            string anterior = string.Format(CultureInfo.CurrentCulture,
                "{0} {1} del {2:dd/MM/yyyy HH:mm}; cliente = {3}; total = $ {4}; estado = {5}",
                venta.Comprobante, NegocioComprobante.FormatearNumero(venta.PuntoVenta, venta.Numero),
                venta.Fecha, venta.Cliente, venta.Total.ToString(NegocioVenta.FormatoImporte), venta.Estado);

            string nuevo = string.Format(CultureInfo.CurrentCulture,
                "ANULADA por {0} ({1}); el stock volvió al depósito y la devolución quedó como egreso de caja; motivo: {2}",
                quien.NombreCompleto, quien.Rol, razon);

            ResultadoAnulacion resultado =
                ServicioVenta.Anular(idVenta, sesion.IdCajaSesion, quien.IdUsuario, razon, anterior, nuevo);

            switch (resultado)
            {
                case ResultadoAnulacion.Anulada:
                    return;
                case ResultadoAnulacion.YaAnulada:
                    throw new ReglaNegocioException("Esa venta ya estaba anulada.");
                case ResultadoAnulacion.OtroTurno:
                    throw new ReglaNegocioException(
                        "Esa venta es de otro turno de caja. Solo se anulan ventas del turno abierto.");
                default:
                    throw new ReglaNegocioException("La venta ya no existe.");
            }
        }
    }
}
