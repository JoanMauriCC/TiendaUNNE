using System.Collections.Generic;
using System.Linq;

namespace TiendaUNNE
{
    /// <summary>Producto tal como se lo busca desde la caja para agregarlo al ticket.</summary>
    public sealed class ProductoVentaItem
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal Stock { get; set; }
    }

    /// <summary>Un renglón del ticket. El precio queda congelado al momento de agregarlo.</summary>
    public sealed class RenglonVenta
    {
        public int IdProducto { get; set; }
        public string Descripcion { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal => decimal.Round(Cantidad * PrecioUnitario, 2);
    }

    /// <summary>Un pago aplicado a la venta. Una venta puede tener varios.</summary>
    public sealed class PagoVenta
    {
        public int IdMedioPago { get; set; }
        public string NombreMedioPago { get; set; }
        public bool EsEfectivo { get; set; }
        public decimal Importe { get; set; }
        public string Referencia { get; set; }
    }

    /// <summary>
    /// La venta que se está armando en la caja. Vive en memoria mientras el cajero
    /// carga renglones; recién al confirmar el cobro se escribe en la base.
    /// </summary>
    public sealed class VentaEditModel
    {
        public int IdCajaSesion { get; set; }
        public int IdUsuario { get; set; }

        public List<RenglonVenta> Renglones { get; } = new List<RenglonVenta>();
        public List<PagoVenta> Pagos { get; } = new List<PagoVenta>();

        public decimal Total => decimal.Round(Renglones.Sum(r => r.Subtotal), 2);

        public decimal TotalPagado => decimal.Round(Pagos.Sum(p => p.Importe), 2);

        public decimal EfectivoRecibido =>
            decimal.Round(Pagos.Where(p => p.EsEfectivo).Sum(p => p.Importe), 2);

        public bool EstaVacia => Renglones.Count == 0;
    }
}
