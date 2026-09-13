namespace TiendaUNNE
{
    /// <summary>
    /// Reglas de inicio de sesión: qué datos hacen falta, si la contraseña coincide
    /// y si el usuario está en condiciones de entrar.
    /// </summary>
    public static class NegocioAutenticacion
    {
        /// <summary>
        /// Mensaje único para todos los motivos de rechazo. Es a propósito: si dijéramos
        /// "ese usuario no existe" le estaríamos confirmando a un atacante qué nombres son válidos.
        /// </summary>
        private const string CredencialesInvalidas = "Usuario o contraseña incorrectos.";

        /// <summary>
        /// Valida las credenciales y, si son correctas, deja la sesión abierta.
        /// Lanza ReglaNegocioException con un mensaje presentable si no se puede entrar.
        /// </summary>
        public static UsuarioLogueado IniciarSesion(string usuario, string password)
        {
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrEmpty(password))
                throw new ReglaNegocioException("Ingrese usuario y contraseña.");

            var credenciales = ServicioAutenticacion.ObtenerCredenciales(usuario.Trim());
            if (credenciales == null)
                throw new ReglaNegocioException(CredencialesInvalidas);

            bool passwordCorrecta = PasswordHasher.Verificar(
                password, credenciales.HashPassword, credenciales.Salt);

            if (!passwordCorrecta || !credenciales.Activo || credenciales.Bloqueado)
                throw new ReglaNegocioException(CredencialesInvalidas);

            ServicioAutenticacion.RegistrarAccesoExitoso(credenciales.Usuario.IdUsuario);
            SesionActual.Iniciar(credenciales.Usuario);

            return credenciales.Usuario;
        }

        public static void CerrarSesion() => SesionActual.Cerrar();
    }
}
