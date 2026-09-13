namespace TiendaUNNE
{
    /// <summary>Opciones del menú principal del sistema.</summary>
    public enum OpcionMenu
    {
        Usuarios,
        Productos,
        Auditoria,
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
                case OpcionMenu.Usuarios:
                case OpcionMenu.Productos:
                case OpcionMenu.Auditoria:
                case OpcionMenu.Caja:
                    return SesionActual.EsAdministrador;

                default:
                    return false;
            }
        }

        /// <summary>
        /// ¿El módulo ya está implementado? Es distinto de tener permiso: Caja todavía
        /// no existe, así que se muestra deshabilitada para cualquier rol.
        /// </summary>
        public static bool ModuloDisponible(OpcionMenu opcion)
        {
            return opcion != OpcionMenu.Caja;
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
