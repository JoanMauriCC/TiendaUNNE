namespace TiendaUNNE
{
    /// <summary>Datos del usuario autenticado que se conservan durante la sesión.</summary>
    public sealed class UsuarioLogueado
    {
        public int IdUsuario { get; set; }
        public int IdPersona { get; set; }
        public string NombreUsuario { get; set; }
        public string NombreCompleto { get; set; }
        public int IdPerfil { get; set; }
        public string Rol { get; set; }        // "Administrador", "Supervisor", "Cajero"
    }
}
