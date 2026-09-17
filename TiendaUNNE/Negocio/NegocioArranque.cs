namespace TiendaUNNE
{
    /// <summary>
    /// Regla de arranque del sistema: si todavía no hay ningún usuario cargado, se crea
    /// un administrador inicial para poder entrar por primera vez.
    /// DNI: 00000000   /   Contraseña: Admin.1234   (hay que cambiarla tras el primer ingreso).
    /// </summary>
    public static class NegocioArranque
    {
        public const string DniInicial = "00000000";
        public const string PasswordInicial = "Admin.1234";

        private const string PerfilAdministrador = "Administrador";
        private const string DescripcionPerfilAdministrador = "Acceso total al sistema";

        public static void AsegurarAdministradorInicial()
        {
            // Si ya hay usuarios, el sistema está inicializado y no se toca nada.
            if (ServicioArranque.ContarUsuarios() > 0)
                return;

            int idPerfilAdmin = ServicioArranque.ObtenerOCrearPerfil(
                PerfilAdministrador, DescripcionPerfilAdministrador);

            byte[] hash, salt;
            PasswordHasher.Generar(PasswordInicial, out hash, out salt);

            ServicioArranque.CrearAdministradorInicial(
                dniCuit: DniInicial,
                nombre: "Administrador",
                apellido: "del Sistema",
                idPerfil: idPerfilAdmin,
                hash: hash,
                salt: salt,
                resumenAuditoria: "Alta automática del administrador inicial");
        }
    }
}
