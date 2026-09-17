using System;
using System.Data;
using System.Data.SqlClient;

namespace TiendaUNNE
{
    /// <summary>
    /// Consultas y escrituras que hacen falta la primera vez que arranca el sistema.
    /// La decisión de si corresponde crear el administrador inicial la toma NegocioArranque.
    /// </summary>
    public static class ServicioArranque
    {
        public static int ContarUsuarios()
        {
            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Usuario;", cn))
                return (int)cmd.ExecuteScalar();
        }

        /// <summary>Devuelve el id del perfil con ese nombre, creándolo si todavía no existe.</summary>
        public static int ObtenerOCrearPerfil(string nombre, string descripcion)
        {
            using (var cn = Db.AbrirConexion())
            {
                using (var cmd = new SqlCommand("SELECT id_perfil FROM dbo.Perfil WHERE nombre = @n;", cn))
                {
                    cmd.Parameters.Add("@n", SqlDbType.NVarChar, 50).Value = nombre;
                    object encontrado = cmd.ExecuteScalar();
                    if (encontrado != null && encontrado != DBNull.Value)
                        return (int)encontrado;
                }

                using (var cmd = new SqlCommand(
                    "INSERT INTO dbo.Perfil (nombre, descripcion) VALUES (@n, @d); SELECT CAST(SCOPE_IDENTITY() AS INT);", cn))
                {
                    cmd.Parameters.Add("@n", SqlDbType.NVarChar, 50).Value = nombre;
                    cmd.Parameters.Add("@d", SqlDbType.NVarChar, 200).Value = descripcion;
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// Crea la persona y el usuario administrador inicial en una sola transacción,
        /// junto con su registro de auditoría. Queda marcado para que deba cambiar la
        /// contraseña en el primer ingreso.
        /// </summary>
        public static int CrearAdministradorInicial(
            string dniCuit, string nombre, string apellido,
            int idPerfil,
            byte[] hash, byte[] salt, string resumenAuditoria)
        {
            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    const string sqlPersona = @"
INSERT INTO dbo.Persona (dni_cuit, nombre, apellido)
VALUES (@dni, @nombre, @apellido);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    int idPersona;
                    using (var cmd = new SqlCommand(sqlPersona, cn, tx))
                    {
                        cmd.Parameters.Add("@dni", SqlDbType.NVarChar, 20).Value = dniCuit;
                        cmd.Parameters.Add("@nombre", SqlDbType.NVarChar, 100).Value = nombre;
                        cmd.Parameters.Add("@apellido", SqlDbType.NVarChar, 100).Value = apellido;
                        idPersona = (int)cmd.ExecuteScalar();
                    }

                    const string sqlUsuario = @"
INSERT INTO dbo.Usuario (id_persona, id_perfil, hash_password, salt, debe_cambiar_pass)
VALUES (@id_persona, @id_perfil, @hash, @salt, @debe_cambiar);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    int idUsuario;
                    using (var cmd = new SqlCommand(sqlUsuario, cn, tx))
                    {
                        cmd.Parameters.Add("@id_persona", SqlDbType.Int).Value = idPersona;
                        cmd.Parameters.Add("@id_perfil", SqlDbType.Int).Value = idPerfil;
                        cmd.Parameters.Add("@hash", SqlDbType.VarBinary, 256).Value = hash;
                        cmd.Parameters.Add("@salt", SqlDbType.VarBinary, 128).Value = salt;
                        cmd.Parameters.Add("@debe_cambiar", SqlDbType.Bit).Value = true;
                        idUsuario = (int)cmd.ExecuteScalar();
                    }

                    ServicioAuditoria.Registrar(
                        "ALTA", "Usuario", idUsuario,
                        valorAnterior: null,
                        valorNuevo: resumenAuditoria,
                        idUsuario: idUsuario,
                        cn: cn, tx: tx);

                    tx.Commit();
                    return idUsuario;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
    }
}
