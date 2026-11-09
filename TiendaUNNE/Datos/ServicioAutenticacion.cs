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
        private const string SelectCredenciales = @"
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
";

        /// <summary>
        /// Trae las credenciales guardadas del usuario cuya Persona tiene ese DNI,
        /// o null si no existe ninguno.
        /// </summary>
        public static CredencialesUsuario ObtenerCredenciales(string dni)
        {
            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(SelectCredenciales + "WHERE p.dni_cuit = @dni;", cn))
            {
                cmd.Parameters.Add("@dni", SqlDbType.NVarChar, 20).Value = dni;
                return LeerCredenciales(cmd);
            }
        }

        /// <summary>
        /// Lo mismo, buscando por id de usuario: sirve para volver a pedirle la contraseña
        /// a quien ya tiene la sesión abierta. Null si no existe.
        /// </summary>
        public static CredencialesUsuario ObtenerCredencialesPorId(int idUsuario)
        {
            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(SelectCredenciales + "WHERE u.id_usuario = @id;", cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idUsuario;
                return LeerCredenciales(cmd);
            }
        }

        private static CredencialesUsuario LeerCredenciales(SqlCommand cmd)
        {
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

        /// <summary>
        /// Suma un intento fallido de contraseña y, si con este llega al límite, bloquea al usuario.
        /// Las dos cosas van en un solo UPDATE, así dos intentos simultáneos no se pisan la cuenta.
        /// Devuelve true solo si quedó bloqueado AHORA (no si ya lo estaba); en ese caso el bloqueo
        /// se graba en la auditoría en la misma transacción, sin usuario (no se sabe quién lo intentó).
        /// </summary>
        public static bool RegistrarIntentoFallido(int idUsuario, int limite,
                                                   string resumenAnterior, string resumenBloqueo)
        {
            const string sql = @"
UPDATE dbo.Usuario
SET    intentos_fallidos = CASE WHEN intentos_fallidos < 255 THEN intentos_fallidos + 1 ELSE 255 END,
       bloqueado         = CASE WHEN intentos_fallidos + 1 >= @limite THEN 1 ELSE bloqueado END
OUTPUT deleted.bloqueado AS antes, inserted.bloqueado AS ahora
WHERE  id_usuario = @id;";

            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    bool seBloqueo = false;
                    using (var cmd = new SqlCommand(sql, cn, tx))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = idUsuario;
                        cmd.Parameters.Add("@limite", SqlDbType.Int).Value = limite;

                        using (var dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (dr.Read())
                                seBloqueo = !(bool)dr["antes"] && (bool)dr["ahora"];
                        }
                    }

                    if (seBloqueo)
                    {
                        ServicioAuditoria.Registrar(
                            "MODIFICACION", "Usuario", idUsuario,
                            resumenAnterior, resumenBloqueo,
                            idUsuario: null, cn: cn, tx: tx);
                    }

                    tx.Commit();
                    return seBloqueo;
                }
                catch
                {
                    tx.Rollback();
                    throw;
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
