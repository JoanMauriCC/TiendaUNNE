using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace TiendaUNNE
{
    /// <summary>
    /// Consultas y escrituras que hacen falta la primera vez que arranca el sistema.
    /// La decisión de si corresponde crear el administrador inicial la toma NegocioArranque.
    /// </summary>
    public static class ServicioArranque
    {
        /// <summary>
        /// Crea (o vuelve a crear) todos los procedimientos almacenados. El script
        /// TiendaUNNE_StoredProcedures.sql viaja dentro del ejecutable y se corre lote por lote
        /// (separados por GO), así el cliente no tiene que ejecutar nada en SQL Server.
        /// Es seguro repetirlo: cada procedimiento se borra y se crea de nuevo.
        /// </summary>
        public static void AsegurarProcedimientos()
        {
            Assembly ensamblado = typeof(ServicioArranque).Assembly;
            string recurso = ensamblado.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("TiendaUNNE_StoredProcedures.sql", StringComparison.OrdinalIgnoreCase));

            if (recurso == null)
                throw new InvalidOperationException("No se encontró el script de procedimientos almacenados.");

            string script;
            using (var lector = new StreamReader(ensamblado.GetManifestResourceStream(recurso), Encoding.UTF8))
                script = lector.ReadToEnd();

            // La base la define la cadena de conexión: se ignora el "USE" del script.
            script = Regex.Replace(script, @"^\s*USE\s+\w+\s*;?\s*$", string.Empty,
                RegexOptions.Multiline | RegexOptions.IgnoreCase);

            string[] lotes = Regex.Split(script, @"^[ \t]*GO[ \t]*\r?$",
                RegexOptions.Multiline | RegexOptions.IgnoreCase);

            using (var cn = Db.AbrirConexion())
            {
                foreach (string lote in lotes)
                {
                    if (string.IsNullOrWhiteSpace(lote)) continue;

                    using (var cmd = new SqlCommand(lote, cn))
                        cmd.ExecuteNonQuery();
                }
            }
        }

        public static int ContarUsuarios()
        {
            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Usuario;", cn))
                return (int)cmd.ExecuteScalar();
        }

        /// <summary>
        /// Carga lo mínimo para poder vender si falta: la primera caja, los medios de pago y el
        /// tipo de comprobante «Ticket». Solo agrega lo que no existe, así que se puede llamar
        /// en cada arranque sin pisar nada de lo que se haya configurado.
        /// </summary>
        public static void AsegurarDatosDeCaja()
        {
            const string sql = @"
IF NOT EXISTS (SELECT 1 FROM dbo.Caja)
    INSERT INTO dbo.Caja (nombre, descripcion)
    VALUES (N'Caja 1', N'Caja principal del local');

IF NOT EXISTS (SELECT 1 FROM dbo.Medio_pago)
    INSERT INTO dbo.Medio_pago (nombre, requiere_referencia, es_efectivo) VALUES
        (N'Efectivo',           0, 1),
        (N'Tarjeta de débito',  1, 0),
        (N'Tarjeta de crédito', 1, 0),
        (N'Transferencia',      1, 0);

IF NOT EXISTS (SELECT 1 FROM dbo.Tipo_comprobante WHERE codigo = N'TKT')
    INSERT INTO dbo.Tipo_comprobante (codigo, nombre, letra, signo)
    VALUES (N'TKT', N'Ticket', NULL, 1);";

            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
                cmd.ExecuteNonQuery();
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
