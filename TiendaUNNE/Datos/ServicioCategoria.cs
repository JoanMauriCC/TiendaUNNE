using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace TiendaUNNE
{
    /// <summary>
    /// Acceso a datos de Categoria: conexión, SQL y transacciones. Nada más.
    /// Los datos llegan ya validados y normalizados desde NegocioCategoria, y los textos
    /// de auditoría vienen armados desde ahí; acá solo se persisten.
    /// </summary>
    public static class ServicioCategoria
    {
        // ---------------------------------------------------------------------
        // Consultas
        // ---------------------------------------------------------------------

        /// <summary>
        /// Listado para la grilla, filtrado por estado: <paramref name="activas"/> en true trae
        /// las activas; en false, las dadas de baja. Incluye cuántos productos activos tiene
        /// cada categoría (dato calculado, no se guarda).
        /// </summary>
        public static DataTable Listar(bool activas)
        {
            const string sql = @"
SELECT  c.id_categoria AS IdCategoria,
        c.nombre       AS Nombre,
        c.descripcion  AS Descripcion,
        (SELECT COUNT(*) FROM dbo.Producto p
          WHERE p.id_categoria = c.id_categoria AND p.activo = 1) AS Productos,
        c.activo       AS Activo
FROM   dbo.Categoria c
WHERE  c.activo = @activo
ORDER BY c.nombre;";

            var dt = new DataTable();
            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@activo", SqlDbType.Bit).Value = activas;
                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(dt);
            }
            return dt;
        }

        /// <summary>Categorías activas para poblar el ComboBox del editor de productos.</summary>
        public static List<CategoriaItem> ListarActivas()
        {
            const string sql = @"
SELECT id_categoria, nombre
FROM   dbo.Categoria
WHERE  activo = 1
ORDER BY nombre;";

            var lista = new List<CategoriaItem>();
            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            using (var dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    lista.Add(new CategoriaItem
                    {
                        Id = (int)dr["id_categoria"],
                        Nombre = (string)dr["nombre"]
                    });
                }
            }
            return lista;
        }

        /// <summary>Trae una categoría, o null si no existe. Interpretar el null es cosa de Negocio.</summary>
        public static CategoriaEditModel Obtener(int idCategoria)
        {
            const string sql = @"
SELECT id_categoria, nombre, descripcion
FROM   dbo.Categoria
WHERE  id_categoria = @id;";

            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idCategoria;
                using (var dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!dr.Read())
                        return null;

                    return new CategoriaEditModel
                    {
                        IdCategoria = (int)dr["id_categoria"],
                        Nombre = (string)dr["nombre"],
                        Descripcion = dr["descripcion"] as string
                    };
                }
            }
        }

        // ---------------------------------------------------------------------
        // Alta
        // ---------------------------------------------------------------------

        public static int Crear(CategoriaEditModel m, int idUsuarioSesion, string resumenNuevo)
        {
            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    const string sql = @"
INSERT INTO dbo.Categoria (nombre, descripcion)
VALUES (@nombre, @descripcion);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    int idCategoria;
                    using (var cmd = new SqlCommand(sql, cn, tx))
                    {
                        AgregarParams(cmd, m);
                        idCategoria = (int)cmd.ExecuteScalar();
                    }

                    ServicioAuditoria.Registrar(
                        "ALTA", "Categoria", idCategoria,
                        valorAnterior: null,
                        valorNuevo: resumenNuevo,
                        idUsuario: idUsuarioSesion,
                        cn: cn, tx: tx);

                    tx.Commit();
                    return idCategoria;
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

        public static void Actualizar(CategoriaEditModel m, int idUsuarioSesion,
                                      string resumenAnterior, string resumenNuevo)
        {
            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    const string sql = @"
UPDATE dbo.Categoria
SET nombre = @nombre, descripcion = @descripcion
WHERE id_categoria = @id;";

                    using (var cmd = new SqlCommand(sql, cn, tx))
                    {
                        AgregarParams(cmd, m);
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = m.IdCategoria;
                        cmd.ExecuteNonQuery();
                    }

                    ServicioAuditoria.Registrar(
                        "MODIFICACION", "Categoria", m.IdCategoria,
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

        /// <summary>Devuelve la cantidad de filas afectadas; 0 significa que ya estaba inactiva.</summary>
        public static int DarDeBaja(int idCategoria, int idUsuarioSesion, string resumenAnterior)
        {
            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    int filas;
                    using (var cmd = new SqlCommand(
                        "UPDATE dbo.Categoria SET activo = 0 WHERE id_categoria = @id AND activo = 1;", cn, tx))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = idCategoria;
                        filas = cmd.ExecuteNonQuery();
                    }

                    if (filas > 0)
                    {
                        ServicioAuditoria.Registrar(
                            "BAJA", "Categoria", idCategoria,
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
        /// Reactivación: UPDATE Categoria SET activo = 1 + Auditoria (MODIFICACION).
        /// Devuelve la cantidad de filas afectadas; 0 significa que ya estaba activa.
        /// </summary>
        public static int DarDeAlta(int idCategoria, int idUsuarioSesion, string resumenAnterior)
        {
            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    int filas;
                    using (var cmd = new SqlCommand(
                        "UPDATE dbo.Categoria SET activo = 1 WHERE id_categoria = @id AND activo = 0;", cn, tx))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = idCategoria;
                        filas = cmd.ExecuteNonQuery();
                    }

                    if (filas > 0)
                    {
                        ServicioAuditoria.Registrar(
                            "MODIFICACION", "Categoria", idCategoria,
                            valorAnterior: resumenAnterior,
                            valorNuevo: "activo = 1 (reactivada)",
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

        private static void AgregarParams(SqlCommand cmd, CategoriaEditModel m)
        {
            cmd.Parameters.Add("@nombre", SqlDbType.NVarChar, 100).Value = m.Nombre;
            cmd.Parameters.Add("@descripcion", SqlDbType.NVarChar, 200).Value = Nz(m.Descripcion);
        }

        /// <summary>Mapea null de C# a NULL de SQL. El recorte de espacios ya lo hizo Negocio.</summary>
        private static object Nz(string s) => (object)s ?? DBNull.Value;
    }
}
