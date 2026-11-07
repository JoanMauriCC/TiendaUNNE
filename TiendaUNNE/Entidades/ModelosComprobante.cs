using System;
using System.Collections.Generic;

namespace TiendaUNNE
{
    /// <summary>Un tipo de comprobante que se puede emitir al cobrar (Ticket, Factura B, Factura A).</summary>
    public sealed class TipoComprobanteItem
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }

        /// <summary>True si el comprobante muestra el IVA separado del neto (Facturas A y B).</summary>
        public bool DiscriminaIva { get; set; }

        public override string ToString() => Nombre;
    }

    /// <summary>Lo que devuelve la base al guardar una venta: su id, el número asignado y la fecha.</summary>
    public sealed class VentaRegistrada
    {
        public int IdVenta { get; set; }
        public long Numero { get; set; }
        public DateTime FechaHora { get; set; }
    }

    public sealed class ComprobanteRenglon
    {
        public string Descripcion { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public sealed class ComprobantePago
    {
        public string Medio { get; set; }
        public decimal Importe { get; set; }
    }

    /// <summary>
    /// El comprobante ya emitido, listo para mostrar o guardar en PDF. Solo datos: todo viene
    /// armado desde NegocioComprobante, quien lo muestra no calcula nada.
    /// </summary>
    public sealed class ComprobanteVista
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public bool DiscriminaIva { get; set; }

        public int PuntoVenta { get; set; }
        public long Numero { get; set; }
        public string NumeroFormateado { get; set; }
        public DateTime FechaHora { get; set; }
        public string Cajero { get; set; }

        public string ClienteNombre { get; set; }
        public string ClienteDocumento { get; set; }
        public string ClienteCondicionIva { get; set; }

        public List<ComprobanteRenglon> Renglones { get; } = new List<ComprobanteRenglon>();
        public List<ComprobantePago> Pagos { get; } = new List<ComprobantePago>();

        public decimal ImporteNeto { get; set; }
        public decimal ImporteIva { get; set; }
        public decimal Total { get; set; }
        public decimal Vuelto { get; set; }

        /// <summary>True si se vuelve a imprimir un comprobante ya emitido (se aclara «REIMPRESIÓN»).</summary>
        public bool EsCopia { get; set; }

        /// <summary>True en una nota de crédito: los importes son lo que se devuelve.</summary>
        public bool EsNotaCredito { get; set; }

        /// <summary>La factura o ticket que corrige una nota de crédito (ej. «Factura A 0001-00000023»).</summary>
        public string ComprobanteAsociado { get; set; }

        public string Motivo { get; set; }

        /// <summary>True si la venta figura como anulada (ventas de antes; se aclara en el comprobante).</summary>
        public bool Anulada { get; set; }
    }
}
