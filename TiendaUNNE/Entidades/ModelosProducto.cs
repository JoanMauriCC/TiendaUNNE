namespace TiendaUNNE
{
    /// <summary>
    /// Datos de un producto que viajan entre frmProductoEditor y ServicioProducto.
    /// <see cref="IdProducto"/> == 0 indica ALTA; distinto de 0 indica EDICIÓN.
    /// </summary>
    public sealed class ProductoEditModel
    {
        public int IdProducto { get; set; }

        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal Stock { get; set; }

        public int IdCategoria { get; set; }
        public string NombreCategoria { get; set; }

        public bool EsAlta => IdProducto == 0;
    }
}
