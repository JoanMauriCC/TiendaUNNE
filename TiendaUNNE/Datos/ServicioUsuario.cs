using System;
using System.Data;
using System.Data.SqlClient;

namespace TiendaUNNE
{
    /// <summary>
    /// Acceso a datos de Usuario + Persona: conexión, SQL y transacciones. Nada más.
    /// El hasheo de la contraseña, las validaciones y los textos de auditoría los
    /// resuelve NegocioUsuario y llegan acá ya listos para persistir.
    /// </summary>
    public static class ServicioUsuario
    {
        // ---------------------------------------------------------------------
        // Consultas
        // ---------------------------------------------------------------------

        /// <summary>
        /// Listado para la grilla: usuarios con datos de Persona y Perfil, filtrados por
        /// estado. <paramref name="activos"/> en true trae los activos; en false, los
        /// dados de baja (para el toggle "Activos / Inactivos" de la pantalla).
        /// </summary>
        public static DataTable Listar(bool activos)
        {
            const string sql = @"
SELECT  u.id_usuario                            AS IdUsuario,
        p.dni_cuit                              AS DniCuit,
        p.apellido                              AS Apellido,
        p.nombre                                AS Nombre,
        p.telefono                              AS Telefono,
        p.email                                 AS Email,
        pf.nombre                               AS Perfil,
        u.activo                                AS Activo
FROM        dbo.Usuario u
INNER JOIN  dbo.Persona p  ON p.id_persona = u.id_persona
INNER JOIN  dbo.Perfil  pf ON pf.id_perfil = u.id_perfil
WHERE u.activo = @activo
ORDER BY p.apellido, p.nombre;";

            var dt = new DataTable();
            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@activo", SqlDbType.Bit).Value = activos;
                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(dt);
            }
            return dt;
        }

        /// <summary>Trae un usuario con su persona y perfil, o null si no existe.</summary>
        public static UsuarioEditModel Obtener(int idUsuario)
        {
            const string sql = @"
SELECT  u.id_usuario, u.id_persona, u.id_perfil,
        p.dni_cuit, p.nombre, p.apellido, p.direccion, p.telefono, p.email, p.fecha_nacimiento,
        pf.nombre AS perfil_nombre
FROM        dbo.Usuario u
INNER JOIN  dbo.Persona p  ON p.id_persona = u.id_persona
INNER JOIN  dbo.Perfil  pf ON pf.id_perfil = u.id_perfil
WHERE u.id_usuario = @id;";

            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idUsuario;
                using (var dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!dr.Read())
                        return null;

                    return new UsuarioEditModel
                    {
                        IdUsuario = (int)dr["id_usuario"],
                        IdPersona = (int)dr["id_persona"],
                        IdPerfil = (int)dr["id_perfil"],
                        NombrePerfil = (string)dr["perfil_nombre"],
                        DniCuit = (string)dr["dni_cuit"],
                        Nombre = (string)dr["nombre"],
                        Apellido = (string)dr["apellido"],
                        Direccion = dr["direccion"] as string,
                        Telefono = dr["telefono"] as string,
                        Email = dr["email"] as string,
                        FechaNacimiento = dr["fecha_nacimiento"] == DBNull.Value
                                            ? (DateTime?)null
                                            : (DateTime)dr["fecha_nacimiento"]
                    };
                }
            }
        }

        /// <summary>
        /// Consulta al stored procedure sp_ExisteEmailPersona si el email ya lo tiene
        /// otra persona. <paramref name="idPersonaExcluir"/> es la persona que se está
        /// editando (null en un alta), para que su propio email no cuente como repetido.
        /// </summary>
        public static bool ExisteEmail(string email, int? idPersonaExcluir)
        {
            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand("dbo.sp_ExisteEmailPersona", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@email", SqlDbType.NVarChar, 150).Value = email;
                cmd.Parameters.Add("@id_persona_excluir", SqlDbType.Int).Value =
                    (object)idPersonaExcluir ?? DBNull.Value;

                return (bool)cmd.ExecuteScalar();
            }
        }

        // ---------------------------------------------------------------------
        // Alta
        // ---------------------------------------------------------------------

        /// <summary>
        /// Alta en una transacción: INSERT Persona -> SCOPE_IDENTITY() -> INSERT Usuario
        /// -> INSERT Auditoria (ALTA). El hash y el salt ya vienen calculados desde Negocio.
        /// </summary>
        public static int Crear(UsuarioEditModel m, int idUsuarioSesion,
                                byte[] hash, byte[] salt, string resumenNuevo)
        {
            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    const string sqlPersona = @"
INSERT INTO dbo.Persona (dni_cuit, nombre, apellido, direccion, telefono, email, fecha_nacimiento)
VALUES (@dni, @nombre, @apellido, @direccion, @telefono, @email, @fnac);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    int idPersona;
                    using (var cmd = new SqlCommand(sqlPersona, cn, tx))
                    {
                        AgregarParamsPersona(cmd, m);
                        idPersona = (int)cmd.ExecuteScalar();
                    }

                    const string sqlUsuario = @"
INSERT INTO dbo.Usuario (id_persona, id_perfil, hash_password, salt)
VALUES (@id_persona, @id_perfil, @hash, @salt);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    int idUsuario;
                    using (var cmd = new SqlCommand(sqlUsuario, cn, tx))
                    {
                        cmd.Parameters.Add("@id_persona", SqlDbType.Int).Value = idPersona;
                        cmd.Parameters.Add("@id_perfil", SqlDbType.Int).Value = m.IdPerfil;
                        cmd.Parameters.Add("@hash", SqlDbType.VarBinary, 256).Value = hash;
                        cmd.Parameters.Add("@salt", SqlDbType.VarBinary, 128).Value = salt;
                        idUsuario = (int)cmd.ExecuteScalar();
                    }

                    ServicioAuditoria.Registrar(
                        "ALTA", "Usuario", idUsuario,
                        valorAnterior: null,
                        valorNuevo: resumenNuevo,
                        idUsuario: idUsuarioSesion,
                        cn: cn, tx: tx);

                    tx.Commit();
                    return idUsuario;
                }
                catch (SqlException ex)
                {
                    tx.Rollback();
                    throw DuplicadoException.Traducir(ex);
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        // ---------------------------------------------------------------------
        // Edición
        // ---------------------------------------------------------------------

        /// <summary>
        /// Edición en una transacción: UPDATE Persona -> UPDATE Usuario -> INSERT Auditoria.
        /// Si <paramref name="hash"/> viene en null, la contraseña no se toca: esa decisión
        /// la tomó NegocioUsuario, acá solo se arma el UPDATE que corresponde.
        /// </summary>
        public static void Actualizar(UsuarioEditModel m, int idUsuarioSesion,
                                      byte[] hash, byte[] salt,
                                      string resumenAnterior, string resumenNuevo)
        {
            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    const string sqlPersona = @"
UPDATE dbo.Persona
SET dni_cuit = @dni, nombre = @nombre, apellido = @apellido,
    direccion = @direccion, telefono = @telefono, email = @email, fecha_nacimiento = @fnac
WHERE id_persona = @id_persona;";

                    using (var cmd = new SqlCommand(sqlPersona, cn, tx))
                    {
                        AgregarParamsPersona(cmd, m);
                        cmd.Parameters.Add("@id_persona", SqlDbType.Int).Value = m.IdPersona;
                        cmd.ExecuteNonQuery();
                    }

                    bool cambiaPassword = hash != null && salt != null;

                    string sqlUsuario = "UPDATE dbo.Usuario SET id_perfil = @id_perfil";
                    if (cambiaPassword)
                        sqlUsuario += ", hash_password = @hash, salt = @salt, debe_cambiar_pass = 0";
                    sqlUsuario += " WHERE id_usuario = @id_usuario;";

                    using (var cmd = new SqlCommand(sqlUsuario, cn, tx))
                    {
                        cmd.Parameters.Add("@id_perfil", SqlDbType.Int).Value = m.IdPerfil;
                        if (cambiaPassword)
                        {
                            cmd.Parameters.Add("@hash", SqlDbType.VarBinary, 256).Value = hash;
                            cmd.Parameters.Add("@salt", SqlDbType.VarBinary, 128).Value = salt;
                        }
                        cmd.Parameters.Add("@id_usuario", SqlDbType.Int).Value = m.IdUsuario;
                        cmd.ExecuteNonQuery();
                    }

                    ServicioAuditoria.Registrar(
                        "MODIFICACION", "Usuario", m.IdUsuario,
                        resumenAnterior, resumenNuevo, idUsuarioSesion,
                        cn, tx);

                    tx.Commit();
                }
                catch (SqlException ex)
                {
                    tx.Rollback();
                    throw DuplicadoException.Traducir(ex);
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        // ---------------------------------------------------------------------
        // Baja lógica
        // ---------------------------------------------------------------------

        /// <summary>
        /// Baja lógica: UPDATE Usuario SET activo = 0 + Auditoria (BAJA). Persona.activo NO se toca.
        /// Devuelve la cantidad de filas afectadas; 0 significa que ya estaba inactivo.
        /// </summary>
        public static int DarDeBaja(int idUsuario, int idUsuarioSesion, string resumenAnterior)
        {
            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    int filas;
                    using (var cmd = new SqlCommand(
                        "UPDATE dbo.Usuario SET activo = 0 WHERE id_usuario = @id AND activo = 1;", cn, tx))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = idUsuario;
                        filas = cmd.ExecuteNonQuery();
                    }

                    if (filas > 0)
                    {
                        ServicioAuditoria.Registrar(
                            "BAJA", "Usuario", idUsuario,
                            valorAnterior: resumenAnterior,
                            valorNuevo: "activo = 0 (baja lógica)",
                            idUsuario: idUsuarioSesion,
                            cn: cn, tx: tx);
                    }

                    tx.Commit();
                    return filas;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Reactivación: UPDATE Usuario SET activo = 1 + Auditoria (MODIFICACION). Persona.activo
        /// NO se toca. Devuelve la cantidad de filas afectadas; 0 significa que ya estaba activo.
        /// </summary>
        public static int DarDeAlta(int idUsuario, int idUsuarioSesion, string resumenAnterior)
        {
            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    int filas;
                    using (var cmd = new SqlCommand(
                        "UPDATE dbo.Usuario SET activo = 1 WHERE id_usuario = @id AND activo = 0;", cn, tx))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = idUsuario;
                        filas = cmd.ExecuteNonQuery();
                    }

                    if (filas > 0)
                    {
                        ServicioAuditoria.Registrar(
                            "MODIFICACION", "Usuario", idUsuario,
                            valorAnterior: resumenAnterior,
                            valorNuevo: "activo = 1 (reactivado)",
                            idUsuario: idUsuarioSesion,
                            cn: cn, tx: tx);
                    }

                    tx.Commit();
                    return filas;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        // ---------------------------------------------------------------------
        // Auxiliares
        // ---------------------------------------------------------------------

        private static void AgregarParamsPersona(SqlCommand cmd, UsuarioEditModel m)
        {
            cmd.Parameters.Add("@dni", SqlDbType.NVarChar, 20).Value = m.DniCuit;
            cmd.Parameters.Add("@nombre", SqlDbType.NVarChar, 100).Value = m.Nombre;
            cmd.Parameters.Add("@apellido", SqlDbType.NVarChar, 100).Value = m.Apellido;
            cmd.Parameters.Add("@direccion", SqlDbType.NVarChar, 200).Value = Nz(m.Direccion);
            cmd.Parameters.Add("@telefono", SqlDbType.NVarChar, 30).Value = Nz(m.Telefono);
            cmd.Parameters.Add("@email", SqlDbType.NVarChar, 150).Value = Nz(m.Email);
            cmd.Parameters.Add("@fnac", SqlDbType.Date).Value = (object)m.FechaNacimiento ?? DBNull.Value;
        }

        /// <summary>Mapea null de C# a NULL de SQL. El recorte de espacios ya lo hizo Negocio.</summary>
        private static object Nz(string s) => (object)s ?? DBNull.Value;
    }
}
