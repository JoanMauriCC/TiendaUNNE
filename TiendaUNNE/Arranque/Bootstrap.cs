using System.Data;
using System.Data.SqlClient;

namespace TiendaUNNE
{
    /// <summary>
    /// Utilidad de arranque: si la tabla Usuario está vacía, crea un administrador inicial.
    /// Usuario: admin   /   Contraseña: Admin.1234   (cambiar tras el primer ingreso).
    /// También demuestra el patrón Persona -> SCOPE_IDENTITY() -> Usuario dentro de una transacción.
    /// </summary>
    public static class Bootstrap
    {
        public const string UsuarioInicial = "admin";
        public const string PasswordInicial = "Admin.1234";

        public static void AsegurarAdministradorInicial()
        {
            using (var cn = Db.AbrirConexion())
            {
                if (ContarUsuarios(cn) > 0)
                    return;

                int idPerfilAdmin = ObtenerOCrearPerfil(cn, "Administrador", "Acceso total al sistema");

                byte[] hash, salt;
                PasswordHasher.Generar(PasswordInicial, out hash, out salt);

                using (var tx = cn.BeginTransaction())
                {
                    int idPersona = InsertarPersona(cn, tx, "00000000", "Administrador", "del Sistema");

                    const string sqlUsuario = @"
INSERT INTO dbo.Usuario (id_persona, id_perfil, nombre_usuario, hash_password, salt, debe_cambiar_pass)
VALUES (@id_persona, @id_perfil, @usuario, @hash, @salt, 1);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    int idUsuario;
                    using (var cmd = new SqlCommand(sqlUsuario, cn, tx))
                    {
                        cmd.Parameters.Add("@id_persona", SqlDbType.Int).Value = idPersona;
                        cmd.Parameters.Add("@id_perfil", SqlDbType.Int).Value = idPerfilAdmin;
                        cmd.Parameters.Add("@usuario", SqlDbType.NVarChar, 50).Value = UsuarioInicial;
                        cmd.Parameters.Add("@hash", SqlDbType.VarBinary, 256).Value = hash;
                        cmd.Parameters.Add("@salt", SqlDbType.VarBinary, 128).Value = salt;
                        idUsuario = (int)cmd.ExecuteScalar();
                    }

                    ServicioAuditoria.Registrar(
                        "ALTA", "Usuario", idUsuario,
                        valorAnterior: null,
                        valorNuevo: "Alta automática del administrador inicial",
                        idUsuario: idUsuario,
                        cn: cn, tx: tx);

                    tx.Commit();
                }
            }
        }

        private static int ContarUsuarios(SqlConnection cn)
        {
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Usuario;", cn))
                return (int)cmd.ExecuteScalar();
        }

        private static int ObtenerOCrearPerfil(SqlConnection cn, string nombre, string descripcion)
        {
            using (var cmd = new SqlCommand("SELECT id_perfil FROM dbo.Perfil WHERE nombre = @n;", cn))
            {
                cmd.Parameters.Add("@n", SqlDbType.NVarChar, 50).Value = nombre;
                object r = cmd.ExecuteScalar();
                if (r != null && r != System.DBNull.Value)
                    return (int)r;
            }

            using (var cmd = new SqlCommand(
                "INSERT INTO dbo.Perfil (nombre, descripcion) VALUES (@n, @d); SELECT CAST(SCOPE_IDENTITY() AS INT);", cn))
            {
                cmd.Parameters.Add("@n", SqlDbType.NVarChar, 50).Value = nombre;
                cmd.Parameters.Add("@d", SqlDbType.NVarChar, 200).Value = descripcion;
                return (int)cmd.ExecuteScalar();
            }
        }

        private static int InsertarPersona(SqlConnection cn, SqlTransaction tx,
            string dniCuit, string nombre, string apellido)
        {
            const string sql = @"
INSERT INTO dbo.Persona (dni_cuit, nombre, apellido)
VALUES (@dni, @nombre, @apellido);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.Parameters.Add("@dni", SqlDbType.NVarChar, 20).Value = dniCuit;
                cmd.Parameters.Add("@nombre", SqlDbType.NVarChar, 100).Value = nombre;
                cmd.Parameters.Add("@apellido", SqlDbType.NVarChar, 100).Value = apellido;
                return (int)cmd.ExecuteScalar();
            }
        }
    }
}
