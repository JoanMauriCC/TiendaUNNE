using System.Data;
using System.Data.SqlClient;

namespace TiendaUNNE
{
    /// <summary>
    /// Autenticación de usuarios contra la tabla Usuario (join con Persona y Perfil).
    /// </summary>
    public static class ServicioAutenticacion
    {
        /// <summary>
        /// Valida usuario + contraseña. Devuelve los datos del usuario si son correctos,
        /// o <c>null</c> en cualquier caso de fallo (usuario inexistente, contraseña
        /// incorrecta, usuario inactivo o bloqueado). No se distingue el motivo por seguridad.
        /// </summary>
        public static UsuarioLogueado Autenticar(string usuario, string password)
        {
            const string sql = @"
SELECT  u.id_usuario,
        u.id_persona,
        u.id_perfil,
        u.nombre_usuario,
        u.hash_password,
        u.salt,
        u.activo,
        u.bloqueado,
        (p.apellido + N', ' + p.nombre) AS nombre_completo,
        pf.nombre                       AS rol
FROM        dbo.Usuario u
INNER JOIN  dbo.Persona p  ON p.id_persona = u.id_persona
INNER JOIN  dbo.Perfil  pf ON pf.id_perfil = u.id_perfil
WHERE u.nombre_usuario = @usuario;";

            using (var cn = Db.AbrirConexion())
            {
                int idUsuario;
                bool activo, bloqueado;
                byte[] hash, salt;
                UsuarioLogueado login = new UsuarioLogueado();

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@usuario", SqlDbType.NVarChar, 50).Value = usuario;

                    using (var dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (!dr.Read())
                            return null;   // usuario inexistente

                        idUsuario = (int)dr["id_usuario"];
                        activo = (bool)dr["activo"];
                        bloqueado = (bool)dr["bloqueado"];
                        hash = (byte[])dr["hash_password"];
                        salt = dr["salt"] == System.DBNull.Value ? null : (byte[])dr["salt"];

                        login.IdUsuario = idUsuario;
                        login.IdPersona = (int)dr["id_persona"];
                        login.IdPerfil = (int)dr["id_perfil"];
                        login.NombreUsuario = (string)dr["nombre_usuario"];
                        login.NombreCompleto = (string)dr["nombre_completo"];
                        login.Rol = (string)dr["rol"];
                    }
                }

                bool passwordOk = PasswordHasher.Verificar(password, hash, salt);

                if (!passwordOk || !activo || bloqueado)
                    return null;

                using (var cmd = new SqlCommand(
                    "UPDATE dbo.Usuario SET ultimo_acceso = SYSDATETIME(), intentos_fallidos = 0 WHERE id_usuario = @id;", cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idUsuario;
                    cmd.ExecuteNonQuery();
                }

                return login;
            }
        }
    }
}
