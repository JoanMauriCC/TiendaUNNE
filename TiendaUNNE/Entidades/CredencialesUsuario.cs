namespace TiendaUNNE
{
    /// <summary>
    /// Lo que la base sabe de un usuario al momento de intentar entrar: sus datos,
    /// el hash y el salt guardados, y su estado. Decidir si con esto puede o no
    /// iniciar sesión es responsabilidad de NegocioAutenticacion.
    /// </summary>
    public sealed class CredencialesUsuario
    {
        public UsuarioLogueado Usuario { get; set; }
        public byte[] HashPassword { get; set; }
        public byte[] Salt { get; set; }
        public bool Activo { get; set; }
        public bool Bloqueado { get; set; }
    }
}
