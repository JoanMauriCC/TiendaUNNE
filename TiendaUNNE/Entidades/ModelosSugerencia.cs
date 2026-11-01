namespace TiendaUNNE
{
    /// <summary>
    /// Una coincidencia que se le ofrece a quien está escribiendo en un campo clave (DNI, nombre):
    /// qué registro es y cómo se lee en la lista. Solo datos: el texto lo arma Negocio.
    /// </summary>
    public sealed class SugerenciaItem
    {
        public int Id { get; set; }
        public string Texto { get; set; }
        public bool Activo { get; set; }

        public override string ToString() => Texto;
    }
}