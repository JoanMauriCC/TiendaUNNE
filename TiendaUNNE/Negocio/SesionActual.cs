using System;

namespace TiendaUNNE
{
    /// <summary>
    /// Sesión actual de la aplicación (usuario logueado + rol). Clase estática:
    /// una sola sesión activa por instancia de la app.
    /// </summary>
    public static class SesionActual
    {
        public static UsuarioLogueado Usuario { get; private set; }

        public static bool HaySesion => Usuario != null;

        public static bool EsAdministrador => TieneRol("Administrador");
        public static bool EsSupervisor => TieneRol("Supervisor");
        public static bool EsCajero => TieneRol("Cajero");

        public static void Iniciar(UsuarioLogueado usuario)
            => Usuario = usuario ?? throw new ArgumentNullException(nameof(usuario));

        public static void Cerrar() => Usuario = null;

        private static bool TieneRol(string rol)
            => HaySesion && string.Equals(Usuario.Rol, rol, StringComparison.OrdinalIgnoreCase);
    }
}
