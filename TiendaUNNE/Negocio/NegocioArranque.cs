namespace TiendaUNNE
{
    /// <summary>
    /// Regla de arranque del sistema: si todavía no hay ningún usuario cargado, se crea
    /// un administrador inicial para poder entrar por primera vez.
    /// Usuario: admin   /   Contraseña: Admin.1234   (hay que cambiarla tras el primer ingreso).
    /// </summary>
    public static class NegocioArranque
    {
        public const string UsuarioInicial = "admin";
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
                dniCuit: "00000000",
                nombre: "Administrador",
                apellido: "del Sistema",
                nombreUsuario: UsuarioInicial,
                idPerfil: idPerfilAdmin,
                hash: hash,
                salt: salt,
                resumenAuditoria: "Alta automática del administrador inicial");
        }
    }
}
