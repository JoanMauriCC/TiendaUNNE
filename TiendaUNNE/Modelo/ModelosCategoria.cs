namespace TiendaUNNE
{
    /// <summary>Ítem de la tabla Categoria para enlazar a un ComboBox (ValueMember = Id).</summary>
    public sealed class CategoriaItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public override string ToString() => Nombre;
    }

    /// <summary>
    /// Datos de una categoría que viajan entre frmCategoriaEditor y ServicioCategoria.
    /// <see cref="IdCategoria"/> == 0 indica ALTA; distinto de 0 indica EDICIÓN.
    /// </summary>
    public sealed class CategoriaEditModel
    {
        public int IdCategoria { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }

        public bool EsAlta => IdCategoria == 0;
    }
}
