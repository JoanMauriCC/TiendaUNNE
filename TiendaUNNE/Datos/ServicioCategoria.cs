using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace TiendaUNNE
{
    /// <summary>
    /// CRUD de Categoria. Mismo patrón que ServicioUsuario: transacción + ServicioAuditoria
    /// (tabla_afectada = 'Categoria') y traducción de los UNIQUE a ReglaNegocioException.
    /// </summary>
    public static class ServicioCategoria
    {
        // ---------------------------------------------------------------------
        // Consultas
        // ---------------------------------------------------------------------

        /// <summary>Listado para la grilla de frmCategorias (categorías activas).</summary>
        public static DataTable ListarParaGrilla()
        {
            const string sql = @"
SELECT  id_categoria AS IdCategoria,
        nombre       AS Nombre,
        descripcion  AS Descripcion,
        activo       AS Activo
FROM   dbo.Categoria
WHERE  activo = 1
ORDER BY nombre;";

            var dt = new DataTable();
            using (var cn = Db.AbrirConexion())
            using (var da = new SqlDataAdapter(sql, cn))
                da.Fill(dt);
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

        /// <summary>Trae una categoría para cargar el editor en modo edición.</summary>
        public static CategoriaEditModel ObtenerParaEdicion(int idCategoria)
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
                        throw new ReglaNegocioException("La categoría ya no existe.");

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

        public static int Crear(CategoriaEditModel m, int idUsuarioSesion)
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
                        cmd.Parameters.Add("@nombre", SqlDbType.NVarChar, 100).Value = m.Nombre.Trim();
                        cmd.Parameters.Add("@descripcion", SqlDbType.NVarChar, 200).Value = Nz(m.Descripcion);
                        idCategoria = (int)cmd.ExecuteScalar();
                    }

                    ServicioAuditoria.Registrar(
                        "ALTA", "Categoria", idCategoria,
                        valorAnterior: null,
                        valorNuevo: Resumen(m),
                        idUsuario: idUsuarioSesion,
                        cn: cn, tx: tx);

                    tx.Commit();
                    return idCategoria;
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

        public static void Actualizar(CategoriaEditModel m, int idUsuarioSesion)
        {
            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    string valorAnterior = LeerResumen(cn, tx, m.IdCategoria);

                    const string sql = @"
UPDATE dbo.Categoria
SET nombre = @nombre, descripcion = @descripcion
WHERE id_categoria = @id;";

                    using (var cmd = new SqlCommand(sql, cn, tx))
                    {
                        cmd.Parameters.Add("@nombre", SqlDbType.NVarChar, 100).Value = m.Nombre.Trim();
                        cmd.Parameters.Add("@descripcion", SqlDbType.NVarChar, 200).Value = Nz(m.Descripcion);
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = m.IdCategoria;
                        cmd.ExecuteNonQuery();
                    }

                    ServicioAuditoria.Registrar(
                        "MODIFICACION", "Categoria", m.IdCategoria,
                        valorAnterior, Resumen(m), idUsuarioSesion,
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

        public static void DarDeBaja(int idCategoria, int idUsuarioSesion)
        {
            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    string valorAnterior = LeerResumen(cn, tx, idCategoria);

                    int filas;
                    using (var cmd = new SqlCommand(
                        "UPDATE dbo.Categoria SET activo = 0 WHERE id_categoria = @id AND activo = 1;", cn, tx))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = idCategoria;
                        filas = cmd.ExecuteNonQuery();
                    }

                    if (filas == 0)
                        throw new ReglaNegocioException("La categoría ya estaba dada de baja o no existe.");

                    ServicioAuditoria.Registrar(
                        "BAJA", "Categoria", idCategoria,
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

        private static object Nz(string s)
            => string.IsNullOrWhiteSpace(s) ? (object)DBNull.Value : s.Trim();

        private static string LeerResumen(SqlConnection cn, SqlTransaction tx, int idCategoria)
        {
            const string sql = "SELECT nombre, descripcion, activo FROM dbo.Categoria WHERE id_categoria = @id;";
            using (var cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idCategoria;
                using (var dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!dr.Read())
                        return "(sin datos)";

                    return string.Format("Nombre={0}; Descripción={1}; Activo={2}",
                        dr["nombre"],
                        dr["descripcion"] == DBNull.Value ? "-" : dr["descripcion"],
                        dr["activo"]);
                }
            }
        }

        private static string Resumen(CategoriaEditModel m)
        {
            return string.Format("Nombre={0}; Descripción={1}",
                m.Nombre.Trim(),
                string.IsNullOrWhiteSpace(m.Descripcion) ? "-" : m.Descripcion.Trim());
        }

        private static Exception TraducirDuplicado(SqlException ex)
        {
            if (ex.Number == 2627 || ex.Number == 2601)   // PK/UNIQUE violation
            {
                if (ex.Message.IndexOf("UQ_Categoria_nombre", StringComparison.OrdinalIgnoreCase) >= 0)
                    return new ReglaNegocioException("Ya existe una categoría con ese nombre.");

                return new ReglaNegocioException("Ya existe un registro con esos datos (valor duplicado).");
            }
            return ex;
        }
    }
}
