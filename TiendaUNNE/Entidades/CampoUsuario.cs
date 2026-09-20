namespace TiendaUNNE
{
    /// <summary>
    /// Campos del formulario de usuario que Negocio puede señalar como faltantes.
    /// Es un identificador neutro: la capa de Presentación decide a qué control
    /// corresponde cada uno.
    /// </summary>
    public enum CampoUsuario
    {
        Dni,
        Nombre,
        Apellido,
        Perfil,
        Password
    }
}
