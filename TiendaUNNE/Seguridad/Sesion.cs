using System;

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
