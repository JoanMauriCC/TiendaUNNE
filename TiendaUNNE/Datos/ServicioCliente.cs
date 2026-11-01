using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace TiendaUNNE
{
    /// <summary>
    /// Acceso a datos de Cliente: conexión, SQL y transacciones. Nada más.
    /// Los datos llegan ya validados y normalizados desde NegocioCliente, y el texto de
    /// auditoría viene armado desde ahí.
    /// </summary>
    public static class ServicioCliente
    {
        private const string SelectCliente = @"
SELECT  c.id_cliente, p.dni_cuit, p.nombre, p.apellido, c.razon_social, c.cuit,
        c.condicion_iva, p.email, p.telefono
FROM        dbo.Cliente c
INNER JOIN  dbo.Persona p ON p.id_persona = c.id_persona
";

        /// <summary>
        /// Clientes activos que coinciden con lo escrito: el documento (DNI o CUIT) empieza igual,
        /// o el apellido, nombre o razón social lo contienen. Hasta 20, por apellido.
        /// </summary>
        public static List<ClienteItem> Buscar(string texto)
        {
            const string where = @"
WHERE c.activo = 1
  AND (p.dni_cuit LIKE @inicio ESCAPE '\' OR c.cuit LIKE @inicio ESCAPE '\'
       OR p.apellido LIKE @contiene ESCAPE '\' OR p.nombre LIKE @contiene ESCAPE '\'
       OR c.razon_social LIKE @contiene ESCAPE '\')
ORDER BY p.apellido, p.nombre;";

            string escapado = EscaparLike(texto);

            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(SelectCliente.Replace("SELECT ", "SELECT TOP 20 ") + where, cn))
            {
                cmd.Parameters.Add("@inicio", SqlDbType.NVarChar, 120).Value = escapado + "%";
                cmd.Parameters.Add("@contiene", SqlDbType.NVarChar, 120).Value = "%" + escapado + "%";
                return Leer(cmd);
            }
        }

        /// <summary>El cliente activo con ese DNI o CUIT exacto, o null si no existe.</summary>
        public static ClienteItem ObtenerPorDocumento(string documento)
        {
            const string where = @"
WHERE c.activo = 1 AND (p.dni_cuit = @doc OR c.cuit = @doc);";

            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(SelectCliente + where, cn))
            {
                cmd.Parameters.Add("@doc", SqlDbType.NVarChar, 20).Value = documento;

                List<ClienteItem> lista = Leer(cmd);
                return lista.Count == 0 ? null : lista[0];
            }
        }

        /// <summary>
        /// Devuelve el cliente con ese DNI o, si no existe, lo crea (procedimiento
        /// sp_ObtenerOCrearCliente). Si se creó, deja la auditoría (ALTA) en la misma transacción.
        /// <paramref name="creado"/> indica si se creó ahora o ya existía.
        /// </summary>
        public static ClienteItem ObtenerOCrear(ClienteEditModel m, int idUsuario,
                                                string resumenNuevo, out bool creado)
        {
            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    int idCliente;
                    using (var cmd = new SqlCommand("dbo.sp_ObtenerOCrearCliente", cn, tx))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@dni", SqlDbType.NVarChar, 20).Value = m.Dni;
                        cmd.Parameters.Add("@nombre", SqlDbType.NVarChar, 100).Value = m.Nombre;
                        cmd.Parameters.Add("@apellido", SqlDbType.NVarChar, 100).Value = m.Apellido;
                        cmd.Parameters.Add("@email", SqlDbType.NVarChar, 150).Value = Nz(m.Email);
                        cmd.Parameters.Add("@telefono", SqlDbType.NVarChar, 30).Value = Nz(m.Telefono);
                        cmd.Parameters.Add("@razon_social", SqlDbType.NVarChar, 150).Value = Nz(m.RazonSocial);
                        cmd.Parameters.Add("@cuit", SqlDbType.NVarChar, 11).Value = Nz(m.Cuit);
                        cmd.Parameters.Add("@condicion_iva", SqlDbType.NVarChar, 30).Value = m.CondicionIva;

                        SqlParameter pId = cmd.Parameters.Add("@id_cliente", SqlDbType.Int);
                        pId.Direction = ParameterDirection.Output;
                        SqlParameter pCreado = cmd.Parameters.Add("@creado", SqlDbType.Bit);
                        pCreado.Direction = ParameterDirection.Output;

                        cmd.ExecuteNonQuery();

                        idCliente = (int)pId.Value;
                        creado = (bool)pCreado.Value;
                    }

                    if (creado)
                    {
                        ServicioAuditoria.Registrar(
                            "ALTA", "Cliente", idCliente,
                            valorAnterior: null,
                            valorNuevo: resumenNuevo,
                            idUsuario: idUsuario,
                            cn: cn, tx: tx);
                    }

                    ClienteItem cliente;
                    using (var cmd = new SqlCommand(SelectCliente + "WHERE c.id_cliente = @id;", cn, tx))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = idCliente;
                        cliente = Leer(cmd)[0];
                    }

                    tx.Commit();
                    return cliente;
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
        // Auxiliares
        // ---------------------------------------------------------------------

        private static List<ClienteItem> Leer(SqlCommand cmd)
        {
            var lista = new List<ClienteItem>();
            using (var dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    lista.Add(new ClienteItem
                    {
                        IdCliente = (int)dr["id_cliente"],
                        Dni = (string)dr["dni_cuit"],
                        Nombre = (string)dr["nombre"],
                        Apellido = (string)dr["apellido"],
                        RazonSocial = dr["razon_social"] as string,
                        Cuit = dr["cuit"] as string,
                        CondicionIva = (string)dr["condicion_iva"],
                        Email = dr["email"] as string,
                        Telefono = dr["telefono"] as string
                    });
                }
            }
            return lista;
        }

        /// <summary>Para que un % o un _ escrito por la persona se busque tal cual, sin hacer de comodín.</summary>
        private static string EscaparLike(string texto)
        {
            return texto.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_").Replace("[", "\\[");
        }

        private static object Nz(string s) => (object)s ?? DBNull.Value;
    }
}
