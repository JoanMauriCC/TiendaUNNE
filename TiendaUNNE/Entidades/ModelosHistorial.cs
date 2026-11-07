using System;
using System.Collections.Generic;

namespace TiendaUNNE
{
    /// <summary>Una venta tal como aparece en la lista del historial de ventas.</summary>
    public sealed class VentaListada
    {
        public int IdVenta { get; set; }
        public DateTime Fecha { get; set; }
        public string Comprobante { get; set; }
        public int PuntoVenta { get; set; }
        public long Numero { get; set; }
        public string Cliente { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; }

        /// <summary>Observación guardada en la venta (en ventas anuladas de antes, el motivo); null si no tiene.</summary>
        public string Motivo { get; set; }
    }

    /// <summary>
    /// Una venta ya guardada, con todo lo necesario para volver a armar su comprobante: tipo, número,
    /// cliente, renglones y pagos. La arma la capa de datos; Negocio la convierte en el comprobante.
    /// </summary>
    public sealed class VentaCompleta
    {
        public int IdVenta { get; set; }
        public string CodigoTipo { get; set; }
        public string NombreTipo { get; set; }
        public int PuntoVenta { get; set; }
        public long Numero { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }
        public string Cajero { get; set; }

        /// <summary>Null en una venta a consumidor final.</summary>
        public string ClienteNombre { get; set; }
        public string ClienteDni { get; set; }
        public string ClienteCuit { get; set; }
        public string ClienteCondicionIva { get; set; }

        public decimal ImporteNeto { get; set; }
        public decimal ImporteIva { get; set; }
        public decimal Total { get; set; }

        public List<ComprobanteRenglon> Renglones { get; } = new List<ComprobanteRenglon>();
        public List<ComprobantePago> Pagos { get; } = new List<ComprobantePago>();
    }
}