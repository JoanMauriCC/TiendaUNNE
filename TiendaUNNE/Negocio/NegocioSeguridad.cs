namespace TiendaUNNE
{
    /// <summary>Secciones del menú lateral del sistema.</summary>
    public enum OpcionMenu
    {
        Inicio,
        Usuarios,
        Productos,
        Categorias,
        Auditoria,
        Reportes,
        Caja
    }

    /// <summary>
    /// Reglas de acceso: qué puede hacer cada rol y qué módulos están disponibles.
    /// Antes estaba repartido en ifs dentro de frmPrincipal; concentrarlo acá evita
    /// que una pantalla nueva se olvide de poner la barrera.
    /// </summary>
    public static class NegocioSeguridad
    {
        /// <summary>¿El usuario de la sesión actual tiene permiso para esta opción?</summary>
        public static bool PuedeAcceder(OpcionMenu opcion)
        {
            switch (opcion)
            {
                case OpcionMenu.Inicio:
                    return SesionActual.HaySesion;

                // La caja la opera el cajero, el supervisor la controla y el
                // administrador tiene acceso total: los tres perfiles pueden entrar.
                case OpcionMenu.Caja:
                    return SesionActual.EsCajero
                        || SesionActual.EsSupervisor
                        || SesionActual.EsAdministrador;

                // "Supervisión de caja, ventas y reportes" según la definición del perfil.
                case OpcionMenu.Reportes:
                    return SesionActual.EsSupervisor || SesionActual.EsAdministrador;

                case OpcionMenu.Usuarios:
                case OpcionMenu.Productos:
                case OpcionMenu.Categorias:
                case OpcionMenu.Auditoria:
                    return SesionActual.EsAdministrador;

                default:
                    return false;
            }
        }

        /// <summary>Secciones del menú, en el orden en que se muestran.</summary>
        public static OpcionMenu[] OpcionesDelMenu()
        {
            return new[]
            {
                OpcionMenu.Inicio,
                OpcionMenu.Usuarios,
                OpcionMenu.Productos,
                OpcionMenu.Categorias,
                OpcionMenu.Auditoria,
                OpcionMenu.Reportes,
                OpcionMenu.Caja
            };
        }

        /// <summary>
        /// ¿El módulo ya está implementado? Es distinto de tener permiso: sirve para
        /// mostrar deshabilitada una sección que todavía no existe. Hoy están todas.
        /// </summary>
        public static bool ModuloDisponible(OpcionMenu opcion)
        {
            return true;
        }

        /// <summary>Lanza ReglaNegocioException si la sesión actual no puede entrar a la opción.</summary>
        public static void ValidarAcceso(OpcionMenu opcion)
        {
            if (!ModuloDisponible(opcion))
                throw new ReglaNegocioException("Ese módulo todavía no está disponible.");

            if (!PuedeAcceder(opcion))
                throw new ReglaNegocioException("No tiene permisos para acceder a esta opción.");
        }
    }
}
