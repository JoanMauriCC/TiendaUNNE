using System;
using System.Collections.Generic;
using System.Linq;

namespace TiendaUNNE
{
    /// <summary>
    /// Lo que se vendió de un producto en una venta, y cuánto de eso ya se devolvió con notas de
    /// crédito anteriores. Lo que todavía se puede devolver es la diferencia.
    /// </summary>
    public sealed class LineaDevolvible
    {
        public int IdProducto { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal AlicuotaIva { get; set; }
        public decimal Vendida { get; set; }
        public decimal Devuelta { get; set; }

        public decimal Disponible => Vendida - Devuelta;

        /// <summary>Lo que se quiere devolver ahora; la completa quien emite la nota. No puede superar Disponible.</summary>
        public decimal ADevolver { get; set; }
    }

    /// <summary>La venta sobre la que se va a emitir una nota de crédito, con lo que se puede devolver de cada renglón.</summary>
    public sealed class VentaADevolver
    {
        public int IdVenta { get; set; }
        public string CodigoTipo { get; set; }
        public string NombreTipo { get; set; }
        public int PuntoVenta { get; set; }
        public long Numero { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }

        /// <summary>1 si es una venta; -1 si es una nota de crédito (no se le puede hacer otra encima).</summary>
        public int Signo { get; set; }

        public int? IdCliente { get; set; }
        public string ClienteNombre { get; set; }

        /// <summary>Medio de pago con el que se cobró la mayor parte de la venta: se sugiere para devolver.</summary>
        public int? IdMedioPagoSugerido { get; set; }

        public List<LineaDevolvible> Renglones { get; } = new List<LineaDevolvible>();

        public bool HayAlgoParaDevolver => Renglones.Any(r => r.Disponible > 0);
    }

    /// <summary>Los datos con los que se emite una nota de crédito: qué se devuelve y cómo se paga la devolución.</summary>
    public sealed class NotaCreditoEdit
    {
        public int IdVentaOriginal { get; set; }
        public string CodigoTipo { get; set; }
        public int IdCaja { get; set; }
        public int IdCajaSesion { get; set; }
        public int IdUsuario { get; set; }
        public int IdMedioPago { get; set; }
        public string Motivo { get; set; }

        /// <summary>Solo los renglones que se devuelven, con la cantidad a devolver y su IVA ya calculado.</summary>
        public List<RenglonVenta> Renglones { get; } = new List<RenglonVenta>();

        public decimal ImporteNeto { get; set; }
        public decimal ImporteIva { get; set; }
        public decimal Total => decimal.Round(Renglones.Sum(r => r.Subtotal), 2);
    }
}