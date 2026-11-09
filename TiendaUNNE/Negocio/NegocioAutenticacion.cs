namespace TiendaUNNE
{
    /// <summary>
    /// Reglas de inicio de sesión: qué datos hacen falta, si la contraseña coincide
    /// y si el usuario está en condiciones de entrar. También cuenta los intentos fallidos:
    /// al llegar a IntentosParaBloquear el usuario queda bloqueado hasta que un administrador
    /// lo desbloquee.
    /// </summary>
    public static class NegocioAutenticacion
    {
        /// <summary>Intentos fallidos seguidos de contraseña que bloquean al usuario.</summary>
        public const int IntentosParaBloquear = 5;

        /// <summary>
        /// Mensaje único para todos los motivos de rechazo. Es a propósito: si dijéramos
        /// "ese usuario no existe" le estaríamos confirmando a un atacante qué nombres son válidos.
        /// </summary>
        private const string CredencialesInvalidas = "Usuario o contraseña incorrectos.";

        private const string UsuarioBloqueado =
            "Tu usuario está bloqueado por demasiados intentos fallidos. Consultá al administrador.";

        /// <summary>
        /// Valida las credenciales (DNI + contraseña) y, si son correctas, deja la sesión
        /// abierta. Lanza ReglaNegocioException con un mensaje presentable si no se puede entrar.
        /// </summary>
        public static UsuarioLogueado IniciarSesion(string dni, string password)
        {
            if (string.IsNullOrWhiteSpace(dni) || string.IsNullOrEmpty(password))
                throw new ReglaNegocioException("Ingresá el DNI y la contraseña.");

            dni = dni.Trim();
            var credenciales = ServicioAutenticacion.ObtenerCredenciales(dni);
            if (credenciales == null || !credenciales.Activo)
                throw new ReglaNegocioException(CredencialesInvalidas);

            bool passwordCorrecta = PasswordHasher.Verificar(
                password, credenciales.HashPassword, credenciales.Salt);

            // Solo quien sabe la contraseña se entera de que el usuario está bloqueado: a alguien
            // que está probando contraseñas no le confirmamos que ese DNI existe.
            if (credenciales.Bloqueado)
                throw new ReglaNegocioException(passwordCorrecta ? UsuarioBloqueado : CredencialesInvalidas);

            if (!passwordCorrecta)
            {
                bool seBloqueo = RegistrarIntentoFallido(credenciales.Usuario, dni);
                throw new ReglaNegocioException(seBloqueo ? UsuarioBloqueado : CredencialesInvalidas);
            }

            // Se mira antes de registrar este acceso: después ya tendría fecha.
            credenciales.Usuario.PrimerIngreso = !credenciales.UltimoAcceso.HasValue;

            ServicioAutenticacion.RegistrarAccesoExitoso(credenciales.Usuario.IdUsuario);
            SesionActual.Iniciar(credenciales.Usuario);

            return credenciales.Usuario;
        }

        /// <summary>
        /// Suma un intento fallido de contraseña. En el intento número IntentosParaBloquear el usuario
        /// queda bloqueado y el bloqueo queda en la auditoría. Devuelve true si se bloqueó con este intento.
        /// Entrar bien vuelve el contador a cero.
        /// </summary>
        public static bool RegistrarIntentoFallido(UsuarioLogueado usuario, string dni)
        {
            string datos = string.Format("DNI={0}; Nombre={1}", dni, usuario.NombreCompleto);

            return ServicioAutenticacion.RegistrarIntentoFallido(
                usuario.IdUsuario, IntentosParaBloquear,
                datos + "; Bloqueado=No",
                datos + "; Bloqueado=Sí; Motivo=" + IntentosParaBloquear + " intentos fallidos de contraseña");
        }

        public static void CerrarSesion() => SesionActual.Cerrar();
    }
}
