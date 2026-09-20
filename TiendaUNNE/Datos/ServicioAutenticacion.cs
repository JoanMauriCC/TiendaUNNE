using System;
using System.Data;
using System.Data.SqlClient;

namespace TiendaUNNE
{
    /// <summary>
    /// Acceso a los datos de autenticación. Solo lee lo que la base guarda y registra
    /// el acceso: verificar la contraseña y decidir si el usuario puede entrar
    /// es responsabilidad de NegocioAutenticacion.
    /// </summary>
    public static class ServicioAutenticacion
    {
        /// <summary>
        /// Trae las credenciales guardadas del usuario cuya Persona tiene ese DNI,
        /// o null si no existe ninguno.
        /// </summary>
        public static CredencialesUsuario ObtenerCredenciales(string dni)
        {
            const string sql = @"
SELECT  u.id_usuario,
        u.id_persona,
        u.id_perfil,
        u.hash_password,
        u.salt,
        u.activo,
        u.bloqueado,
        (p.apellido + N', ' + p.nombre) AS nombre_completo,
        p.nombre                        AS nombre,
        pf.nombre                       AS rol
FROM        dbo.Usuario u
INNER JOIN  dbo.Persona p  ON p.id_persona = u.id_persona
INNER JOIN  dbo.Perfil  pf ON pf.id_perfil = u.id_perfil
WHERE p.dni_cuit = @dni;";

            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@dni", SqlDbType.NVarChar, 20).Value = dni;

                using (var dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!dr.Read())
                        return null;

                    return new CredencialesUsuario
                    {
                        HashPassword = (byte[])dr["hash_password"],
                        Salt = dr["salt"] == DBNull.Value ? null : (byte[])dr["salt"],
                        Activo = (bool)dr["activo"],
                        Bloqueado = (bool)dr["bloqueado"],
                        Usuario = new UsuarioLogueado
                        {
                            IdUsuario = (int)dr["id_usuario"],
                            IdPersona = (int)dr["id_persona"],
                            IdPerfil = (int)dr["id_perfil"],
                            NombreCompleto = (string)dr["nombre_completo"],
                            Nombre = (string)dr["nombre"],
                            Rol = (string)dr["rol"]
                        }
                    };
                }
            }
        }

        /// <summary>Marca la fecha del último acceso y limpia el contador de intentos fallidos.</summary>
        public static void RegistrarAccesoExitoso(int idUsuario)
        {
            const string sql = @"
UPDATE dbo.Usuario
SET ultimo_acceso = SYSDATETIME(), intentos_fallidos = 0
WHERE id_usuario = @id;";

            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idUsuario;
                cmd.ExecuteNonQuery();
            }
        }
    }
}
