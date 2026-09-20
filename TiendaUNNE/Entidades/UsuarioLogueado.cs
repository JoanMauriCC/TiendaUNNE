namespace TiendaUNNE
{
    /// <summary>Datos del usuario autenticado que se conservan durante la sesión.</summary>
    public sealed class UsuarioLogueado
    {
        public int IdUsuario { get; set; }
        public int IdPersona { get; set; }
        public string NombreCompleto { get; set; }

        /// <summary>Solo el nombre de pila, para saludar ("Bienvenido, Joan").</summary>
        public string Nombre { get; set; }
        public int IdPerfil { get; set; }
        public string Rol { get; set; }        // "Administrador", "Supervisor", "Cajero"
    }
}
