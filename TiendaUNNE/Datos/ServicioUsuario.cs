using System;
using System.Data;
using System.Data.SqlClient;

namespace TiendaUNNE
{
    /// <summary>
    /// CRUD de Usuario combinado con Persona. Reusa PasswordHasher, ServicioAuditoria
    /// y el patrón de transacción Persona -> SCOPE_IDENTITY() -> Usuario de Bootstrap.
    /// </summary>
    public static class ServicioUsuario
    {
        // ---------------------------------------------------------------------
        // Consultas
        // ---------------------------------------------------------------------

        /// <summary>Listado para la grilla: usuarios activos con datos de Persona y Perfil.</summary>
        public static DataTable ListarActivos()
        {
            const string sql = @"
SELECT  u.id_usuario                            AS IdUsuario,
        p.dni_cuit                              AS DniCuit,
        p.apellido                              AS Apellido,
        p.nombre                                AS Nombre,
        p.telefono                              AS Telefono,
        p.email                                 AS Email,
        u.nombre_usuario                        AS Usuario,
        pf.nombre                               AS Perfil,
        u.activo                                AS Activo
FROM        dbo.Usuario u
INNER JOIN  dbo.Persona p  ON p.id_persona = u.id_persona
INNER JOIN  dbo.Perfil  pf ON pf.id_perfil = u.id_perfil
WHERE u.activo = 1
ORDER BY p.apellido, p.nombre;";

            var dt = new DataTable();
            using (var cn = Db.AbrirConexion())
            using (var da = new SqlDataAdapter(sql, cn))
                da.Fill(dt);
            return dt;
        }

        /// <summary>Trae un usuario (con su persona y perfil) para cargar el editor en modo edición.</summary>
        public static UsuarioEditModel ObtenerParaEdicion(int idUsuario)
        {
            const string sql = @"
SELECT  u.id_usuario, u.id_persona, u.nombre_usuario, u.id_perfil,
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
                        throw new ReglaNegocioException("El usuario ya no existe.");

                    return new UsuarioEditModel
                    {
                        IdUsuario = (int)dr["id_usuario"],
                        IdPersona = (int)dr["id_persona"],
                        NombreUsuario = (string)dr["nombre_usuario"],
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

        // ---------------------------------------------------------------------
        // Alta
        // ---------------------------------------------------------------------

        /// <summary>
        /// Alta en una transacción: INSERT Persona -> SCOPE_IDENTITY() -> INSERT Usuario
        /// (con hash/salt de PasswordHasher) -> INSERT Auditoria (ALTA). Devuelve el id_usuario nuevo.
        /// </summary>
        public static int Crear(UsuarioEditModel m, int idUsuarioSesion)
        {
            byte[] hash, salt;
            PasswordHasher.Generar(m.PasswordPlano, out hash, out salt);

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
INSERT INTO dbo.Usuario (id_persona, id_perfil, nombre_usuario, hash_password, salt)
VALUES (@id_persona, @id_perfil, @usuario, @hash, @salt);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    int idUsuario;
                    using (var cmd = new SqlCommand(sqlUsuario, cn, tx))
                    {
                        cmd.Parameters.Add("@id_persona", SqlDbType.Int).Value = idPersona;
                        cmd.Parameters.Add("@id_perfil", SqlDbType.Int).Value = m.IdPerfil;
                        cmd.Parameters.Add("@usuario", SqlDbType.NVarChar, 50).Value = m.NombreUsuario.Trim();
                        cmd.Parameters.Add("@hash", SqlDbType.VarBinary, 256).Value = hash;
                        cmd.Parameters.Add("@salt", SqlDbType.VarBinary, 128).Value = salt;
                        idUsuario = (int)cmd.ExecuteScalar();
                    }

                    ServicioAuditoria.Registrar(
                        "ALTA", "Usuario", idUsuario,
                        valorAnterior: null,
                        valorNuevo: Resumen(m),
                        idUsuario: idUsuarioSesion,
                        cn: cn, tx: tx);

                    tx.Commit();
                    return idUsuario;
                }
                catch (SqlException ex)
                {
                    tx.Rollback();
                    throw TraducirDuplicado(ex);
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
        /// Edición en una transacción: SELECT del estado anterior -> UPDATE Persona ->
        /// UPDATE Usuario (la contraseña solo si vino cargada) -> INSERT Auditoria (MODIFICACION).
        /// </summary>
        public static void Actualizar(UsuarioEditModel m, int idUsuarioSesion)
        {
            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    string valorAnterior = LeerResumen(cn, tx, m.IdUsuario);

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

                    bool cambiaPassword = !string.IsNullOrWhiteSpace(m.PasswordPlano);
                    string sqlUsuario = "UPDATE dbo.Usuario SET nombre_usuario = @usuario, id_perfil = @id_perfil";
                    byte[] hash = null, salt = null;
                    if (cambiaPassword)
                    {
                        PasswordHasher.Generar(m.PasswordPlano, out hash, out salt);
                        sqlUsuario += ", hash_password = @hash, salt = @salt, debe_cambiar_pass = 0";
                    }
                    sqlUsuario += " WHERE id_usuario = @id_usuario;";

                    using (var cmd = new SqlCommand(sqlUsuario, cn, tx))
                    {
                        cmd.Parameters.Add("@usuario", SqlDbType.NVarChar, 50).Value = m.NombreUsuario.Trim();
                        cmd.Parameters.Add("@id_perfil", SqlDbType.Int).Value = m.IdPerfil;
                        if (cambiaPassword)
                        {
                            cmd.Parameters.Add("@hash", SqlDbType.VarBinary, 256).Value = hash;
                            cmd.Parameters.Add("@salt", SqlDbType.VarBinary, 128).Value = salt;
                        }
                        cmd.Parameters.Add("@id_usuario", SqlDbType.Int).Value = m.IdUsuario;
                        cmd.ExecuteNonQuery();
                    }

                    string valorNuevo = Resumen(m) + (cambiaPassword ? "  [contraseña actualizada]" : string.Empty);

                    ServicioAuditoria.Registrar(
                        "MODIFICACION", "Usuario", m.IdUsuario,
                        valorAnterior, valorNuevo, idUsuarioSesion,
                        cn, tx);

                    tx.Commit();
                }
                catch (SqlException ex)
                {
                    tx.Rollback();
                    throw TraducirDuplicado(ex);
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
        /// Baja lógica: UPDATE Usuario SET activo = 0 + Auditoria (BAJA).
        /// Persona.activo NO se toca. Un usuario no puede darse de baja a sí mismo.
        /// </summary>
        public static void DarDeBaja(int idUsuario, int idUsuarioSesion)
        {
            if (idUsuario == idUsuarioSesion)
                throw new ReglaNegocioException("No podés dar de baja tu propio usuario.");

            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    string valorAnterior = LeerResumen(cn, tx, idUsuario);

                    int filas;
                    using (var cmd = new SqlCommand(
                        "UPDATE dbo.Usuario SET activo = 0 WHERE id_usuario = @id AND activo = 1;", cn, tx))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = idUsuario;
                        filas = cmd.ExecuteNonQuery();
                    }

                    if (filas == 0)
                        throw new ReglaNegocioException("El usuario ya estaba dado de baja o no existe.");

                    ServicioAuditoria.Registrar(
                        "BAJA", "Usuario", idUsuario,
                        valorAnterior: valorAnterior,
                        valorNuevo: "activo = 0 (baja lógica)",
                        idUsuario: idUsuarioSesion,
                        cn: cn, tx: tx);

                    tx.Commit();
                }
                catch (ReglaNegocioException)
                {
                    tx.Rollback();
                    throw;
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
            cmd.Parameters.Add("@dni", SqlDbType.NVarChar, 20).Value = m.DniCuit.Trim();
            cmd.Parameters.Add("@nombre", SqlDbType.NVarChar, 100).Value = m.Nombre.Trim();
            cmd.Parameters.Add("@apellido", SqlDbType.NVarChar, 100).Value = m.Apellido.Trim();
            cmd.Parameters.Add("@direccion", SqlDbType.NVarChar, 200).Value = Nz(m.Direccion);
            cmd.Parameters.Add("@telefono", SqlDbType.NVarChar, 30).Value = Nz(m.Telefono);
            cmd.Parameters.Add("@email", SqlDbType.NVarChar, 150).Value = Nz(m.Email);
            cmd.Parameters.Add("@fnac", SqlDbType.Date).Value = (object)m.FechaNacimiento ?? DBNull.Value;
        }

        private static object Nz(string s)
            => string.IsNullOrWhiteSpace(s) ? (object)DBNull.Value : s.Trim();

        /// <summary>Resumen legible del estado actual del usuario (para valor_anterior de Auditoria).</summary>
        private static string LeerResumen(SqlConnection cn, SqlTransaction tx, int idUsuario)
        {
            const string sql = @"
SELECT  p.dni_cuit, p.nombre, p.apellido, p.direccion, p.telefono, p.email, p.fecha_nacimiento,
        u.nombre_usuario, pf.nombre AS perfil_nombre, u.activo
FROM        dbo.Usuario u
INNER JOIN  dbo.Persona p  ON p.id_persona = u.id_persona
INNER JOIN  dbo.Perfil  pf ON pf.id_perfil = u.id_perfil
WHERE u.id_usuario = @id;";

            using (var cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idUsuario;
                using (var dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!dr.Read())
                        return "(sin datos)";

                    var fnac = dr["fecha_nacimiento"] == DBNull.Value
                        ? "-"
                        : ((DateTime)dr["fecha_nacimiento"]).ToString("yyyy-MM-dd");

                    return string.Format(
                        "DNI/CUIT={0}; Nombre={1}, {2}; Dir={3}; Tel={4}; Email={5}; FNac={6}; Usuario={7}; Perfil={8}; Activo={9}",
                        dr["dni_cuit"], dr["apellido"], dr["nombre"],
                        dr["direccion"] == DBNull.Value ? "-" : dr["direccion"],
                        dr["telefono"] == DBNull.Value ? "-" : dr["telefono"],
                        dr["email"] == DBNull.Value ? "-" : dr["email"],
                        fnac, dr["nombre_usuario"], dr["perfil_nombre"], dr["activo"]);
                }
            }
        }

        /// <summary>Resumen legible del modelo cargado en el editor (para valor_nuevo de Auditoria).</summary>
        private static string Resumen(UsuarioEditModel m)
        {
            return string.Format(
                "DNI/CUIT={0}; Nombre={1}, {2}; Dir={3}; Tel={4}; Email={5}; FNac={6}; Usuario={7}; Perfil={8}",
                m.DniCuit, m.Apellido, m.Nombre,
                string.IsNullOrWhiteSpace(m.Direccion) ? "-" : m.Direccion.Trim(),
                string.IsNullOrWhiteSpace(m.Telefono) ? "-" : m.Telefono.Trim(),
                string.IsNullOrWhiteSpace(m.Email) ? "-" : m.Email.Trim(),
                m.FechaNacimiento.HasValue ? m.FechaNacimiento.Value.ToString("yyyy-MM-dd") : "-",
                m.NombreUsuario.Trim(), m.NombrePerfil);
        }

        /// <summary>Traduce las violaciones de UNIQUE de la base a mensajes claros.</summary>
        private static Exception TraducirDuplicado(SqlException ex)
        {
            if (ex.Number == 2627 || ex.Number == 2601)   // PK/UNIQUE violation
            {
                if (ex.Message.IndexOf("UQ_Persona_dni_cuit", StringComparison.OrdinalIgnoreCase) >= 0)
                    return new ReglaNegocioException("Ya existe una persona registrada con ese DNI/CUIT.");

                if (ex.Message.IndexOf("UQ_Usuario_nombre", StringComparison.OrdinalIgnoreCase) >= 0)
                    return new ReglaNegocioException("Ya existe un usuario con ese nombre de usuario.");

                if (ex.Message.IndexOf("UQ_Usuario_persona", StringComparison.OrdinalIgnoreCase) >= 0)
                    return new ReglaNegocioException("La persona seleccionada ya tiene un usuario asociado.");

                return new ReglaNegocioException("Ya existe un registro con esos datos (valor duplicado).");
            }
            return ex;
        }
    }
}
