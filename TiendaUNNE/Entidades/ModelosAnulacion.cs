using System;

namespace TiendaUNNE
{
    /// <summary>Una venta tal como aparece en la lista de ventas del turno.</summary>
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

        /// <summary>Por qué se anuló; null si la venta sigue emitida.</summary>
        public string Motivo { get; set; }
    }

    /// <summary>Qué decidió la base al intentar anular una venta (lo devuelve sp_AnularVenta).</summary>
    public enum ResultadoAnulacion
    {
        Anulada = 0,
        NoExiste = 1,
        YaAnulada = 2,
        OtroTurno = 3
    }
}
