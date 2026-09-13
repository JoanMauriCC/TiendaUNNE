using System;

namespace TiendaUNNE
{
    /// <summary>
    /// El descuento de stock de una venta no se pudo aplicar porque ya no había
    /// existencia suficiente. La capa Datos la lanza con el producto que falló;
    /// NegocioVenta la traduce al mensaje que ve el cajero.
    ///
    /// Existe porque entre que Negocio valida el stock y la venta se confirma puede
    /// haber pasado otra venta: el UPDATE lleva la condición stock >= cantidad, así
    /// que si no afecta ninguna fila es que la existencia se agotó en el medio.
    /// </summary>
    public class StockInsuficienteException : Exception
    {
        public StockInsuficienteException(int idProducto, string descripcion)
            : base("Stock insuficiente para el producto " + descripcion + ".")
        {
            IdProducto = idProducto;
            Descripcion = descripcion;
        }

        public int IdProducto { get; }
        public string Descripcion { get; }
    }
}
