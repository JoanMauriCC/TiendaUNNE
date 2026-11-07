using System;
using System.Data;

namespace TiendaUNNE
{
    /// <summary>Qué período del historial de ventas se quiere ver.</summary>
    public enum RangoHistorial
    {
        TurnoActual,
        Hoy,
        UltimosSieteDias,
        UltimosTreintaDias,
        Todas
    }

    /// <summary>
    /// Historial de ventas: lista de lo vendido y reimpresión de cualquier comprobante por si se
    /// extravió. Puede verlo cualquier rol que use la Caja. Reimprimir no modifica nada: arma de nuevo
    /// el comprobante con lo que quedó guardado y lo marca como «REIMPRESIÓN», sin vuelto (el vuelto
    /// no se guarda: en la base quedan los pagos ya descontados).
    /// </summary>
    public static class NegocioHistorial
    {
        public const string EstadoAnulada = "ANULADA";

        /// <summary>Tope de filas que trae la lista: alcanza para buscar y no carga la base entera.</summary>
        public const int MaximoDeVentas = 300;

        /// <summary>Texto de cada opción del selector de período.</summary>
        public static string NombreRango(RangoHistorial rango)
        {
            switch (rango)
            {
                case RangoHistorial.Hoy: return "Hoy";
                case RangoHistorial.UltimosSieteDias: return "Últimos 7 días";
                case RangoHistorial.UltimosTreintaDias: return "Últimos 30 días";
                case RangoHistorial.Todas: return "Todas (las " + MaximoDeVentas + " más nuevas)";
                default: return "Turno actual";
            }
        }

        /// <summary>Las ventas del período para la grilla, con el número ya con su formato.</summary>
        public static DataTable Listar(RangoHistorial rango, CajaSesion sesion)
        {
            int? idSesion = null;
            DateTime? desde = null;

            switch (rango)
            {
                case RangoHistorial.TurnoActual:
                    if (sesion == null)
                        throw new ReglaNegocioException("Hay que abrir la caja para ver las ventas del turno.");
                    idSesion = sesion.IdCajaSesion;
                    break;
                case RangoHistorial.Hoy:
                    desde = DateTime.Today;
                    break;
                case RangoHistorial.UltimosSieteDias:
                    desde = DateTime.Today.AddDays(-6);
                    break;
                case RangoHistorial.UltimosTreintaDias:
                    desde = DateTime.Today.AddDays(-29);
                    break;
            }

            var dt = new DataTable();
            dt.Columns.Add("IdVenta", typeof(int));
            dt.Columns.Add("Fecha", typeof(DateTime));
            dt.Columns.Add("Comprobante", typeof(string));
            dt.Columns.Add("Numero", typeof(string));
            dt.Columns.Add("Cliente", typeof(string));
            dt.Columns.Add("Total", typeof(decimal));
            dt.Columns.Add("Estado", typeof(string));

            foreach (VentaListada v in ServicioVenta.Listar(idSesion, desde, MaximoDeVentas))
            {
                dt.Rows.Add(v.IdVenta, v.Fecha, v.Comprobante,
                    NegocioComprobante.FormatearNumero(v.PuntoVenta, v.Numero),
                    v.Cliente, v.Total, v.Estado);
            }

            return dt;
        }

        /// <summary>El comprobante de una venta ya emitida, listo para mostrarlo y guardarlo de nuevo en PDF.</summary>
        public static ComprobanteVista ObtenerComprobante(int idVenta)
        {
            VentaCompleta v = ServicioVenta.ObtenerCompleta(idVenta)
                ?? throw new ReglaNegocioException("Esa venta ya no existe.");

            bool discrimina;
            try { discrimina = NegocioComprobante.Buscar(v.CodigoTipo).DiscriminaIva; }
            catch (ReglaNegocioException) { discrimina = false; }

            bool consumidorFinal = v.ClienteNombre == null;

            var vista = new ComprobanteVista
            {
                Codigo = v.CodigoTipo,
                Nombre = v.NombreTipo,
                DiscriminaIva = discrimina,
                PuntoVenta = v.PuntoVenta,
                Numero = v.Numero,
                NumeroFormateado = NegocioComprobante.FormatearNumero(v.PuntoVenta, v.Numero),
                FechaHora = v.Fecha,
                Cajero = v.Cajero,
                ClienteNombre = consumidorFinal ? "Consumidor final" : v.ClienteNombre,
                ClienteDocumento = consumidorFinal
                    ? "—"
                    : (string.IsNullOrEmpty(v.ClienteCuit) ? "DNI " + v.ClienteDni : "CUIT " + v.ClienteCuit),
                ClienteCondicionIva = consumidorFinal ? NegocioCliente.ConsumidorFinal : v.ClienteCondicionIva,
                ImporteNeto = v.ImporteNeto,
                ImporteIva = v.ImporteIva,
                Total = v.Total,
                Vuelto = 0,
                EsCopia = true,
                Anulada = string.Equals(v.Estado, EstadoAnulada, StringComparison.OrdinalIgnoreCase)
            };

            vista.Renglones.AddRange(v.Renglones);
            vista.Pagos.AddRange(v.Pagos);
            return vista;
        }
    }
}
