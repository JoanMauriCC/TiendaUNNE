using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace TiendaUNNE
{
    /// <summary>
    /// CRUD de Producto. Mismo patrón que ServicioUsuario / ServicioCategoria:
    /// transacción + ServicioAuditoria (tabla_afectada = 'Producto').
    /// </summary>
    public static class ServicioProducto
    {
        // ---------------------------------------------------------------------
        // Consultas
        // ---------------------------------------------------------------------

        /// <summary>Listado para la grilla de frmProductos (productos activos, con nombre de categoría).</summary>
        public static DataTable ListarParaGrilla()
        {
            const string sql = @"
SELECT  p.id_producto   AS IdProducto,
        p.nombre         AS Nombre,
        c.nombre         AS Categoria,
        p.precio_venta   AS PrecioVenta,
        p.stock          AS Stock,
        p.activo         AS Activo
FROM        dbo.Producto  p
INNER JOIN  dbo.Categoria c ON c.id_categoria = p.id_categoria
WHERE p.activo = 1
ORDER BY p.nombre;";

            var dt = new DataTable();
            using (var cn = Db.AbrirConexion())
            using (var da = new SqlDataAdapter(sql, cn))
                da.Fill(dt);
            return dt;
        }

        /// <summary>Trae un producto para cargar el editor en modo edición.</summary>
        public static ProductoEditModel ObtenerParaEdicion(int idProducto)
        {
            const string sql = @"
SELECT  p.id_producto, p.id_categoria, p.nombre, p.descripcion,
        p.precio_venta, p.stock,
        c.nombre AS categoria_nombre
FROM        dbo.Producto  p
INNER JOIN  dbo.Categoria c ON c.id_categoria = p.id_categoria
WHERE p.id_producto = @id;";

            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProducto;
                using (var dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!dr.Read())
                        throw new ReglaNegocioException("El producto ya no existe.");

                    return new ProductoEditModel
                    {
                        IdProducto = (int)dr["id_producto"],
                        IdCategoria = (int)dr["id_categoria"],
                        NombreCategoria = (string)dr["categoria_nombre"],
                        Nombre = (string)dr["nombre"],
                        Descripcion = dr["descripcion"] as string,
                        PrecioVenta = (decimal)dr["precio_venta"],
                        Stock = (decimal)dr["stock"]
                    };
                }
            }
        }

        // ---------------------------------------------------------------------
        // Alta
        // ---------------------------------------------------------------------

        public static int Crear(ProductoEditModel m, int idUsuarioSesion)
        {
            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    const string sql = @"
INSERT INTO dbo.Producto
    (id_categoria, nombre, descripcion, precio_venta, stock)
VALUES
    (@id_categoria, @nombre, @descripcion, @precio_venta, @stock);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    int idProducto;
                    using (var cmd = new SqlCommand(sql, cn, tx))
                    {
                        AgregarParams(cmd, m);
                        idProducto = (int)cmd.ExecuteScalar();
                    }

                    ServicioAuditoria.Registrar(
                        "ALTA", "Producto", idProducto,
                        valorAnterior: null,
                        valorNuevo: Resumen(m),
                        idUsuario: idUsuarioSesion,
                        cn: cn, tx: tx);

                    tx.Commit();
                    return idProducto;
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

        public static void Actualizar(ProductoEditModel m, int idUsuarioSesion)
        {
            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    string valorAnterior = LeerResumen(cn, tx, m.IdProducto);

                    const string sql = @"
UPDATE dbo.Producto
SET id_categoria = @id_categoria,
    nombre       = @nombre,
    descripcion  = @descripcion,
    precio_venta = @precio_venta,
    stock        = @stock
WHERE id_producto = @id_producto;";

                    using (var cmd = new SqlCommand(sql, cn, tx))
                    {
                        AgregarParams(cmd, m);
                        cmd.Parameters.Add("@id_producto", SqlDbType.Int).Value = m.IdProducto;
                        cmd.ExecuteNonQuery();
                    }

                    ServicioAuditoria.Registrar(
                        "MODIFICACION", "Producto", m.IdProducto,
                        valorAnterior, Resumen(m), idUsuarioSesion,
                        cn, tx);

                    tx.Commit();
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

        public static void DarDeBaja(int idProducto, int idUsuarioSesion)
        {
            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    string valorAnterior = LeerResumen(cn, tx, idProducto);

                    int filas;
                    using (var cmd = new SqlCommand(
                        "UPDATE dbo.Producto SET activo = 0 WHERE id_producto = @id AND activo = 1;", cn, tx))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProducto;
                        filas = cmd.ExecuteNonQuery();
                    }

                    if (filas == 0)
                        throw new ReglaNegocioException("El producto ya estaba dado de baja o no existe.");

                    ServicioAuditoria.Registrar(
                        "BAJA", "Producto", idProducto,
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

        private static void AgregarParams(SqlCommand cmd, ProductoEditModel m)
        {
            cmd.Parameters.Add("@id_categoria", SqlDbType.Int).Value = m.IdCategoria;
            cmd.Parameters.Add("@nombre", SqlDbType.NVarChar, 150).Value = m.Nombre.Trim();
            cmd.Parameters.Add("@descripcion", SqlDbType.NVarChar, 500).Value = Nz(m.Descripcion);
            cmd.Parameters.Add("@precio_venta", SqlDbType.Decimal).Value = m.PrecioVenta;
            cmd.Parameters.Add("@stock", SqlDbType.Decimal).Value = m.Stock;
        }

        private static object Nz(string s)
            => string.IsNullOrWhiteSpace(s) ? (object)DBNull.Value : s.Trim();

        private static string LeerResumen(SqlConnection cn, SqlTransaction tx, int idProducto)
        {
            const string sql = @"
SELECT  p.nombre, p.descripcion, p.precio_venta, p.stock, p.activo,
        c.nombre AS categoria_nombre
FROM        dbo.Producto  p
INNER JOIN  dbo.Categoria c ON c.id_categoria = p.id_categoria
WHERE p.id_producto = @id;";

            using (var cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProducto;
                using (var dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!dr.Read())
                        return "(sin datos)";

                    return string.Format(CultureInfo.InvariantCulture,
                        "Nombre={0}; Categoría={1}; Desc={2}; PVenta={3}; Stock={4}; Activo={5}",
                        dr["nombre"], dr["categoria_nombre"],
                        dr["descripcion"] == DBNull.Value ? "-" : dr["descripcion"],
                        dr["precio_venta"], dr["stock"], dr["activo"]);
                }
            }
        }

        private static string Resumen(ProductoEditModel m)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Nombre={0}; Categoría={1}; Desc={2}; PVenta={3}; Stock={4}",
                m.Nombre.Trim(),
                string.IsNullOrWhiteSpace(m.NombreCategoria) ? ("id " + m.IdCategoria) : m.NombreCategoria,
                string.IsNullOrWhiteSpace(m.Descripcion) ? "-" : m.Descripcion.Trim(),
                m.PrecioVenta, m.Stock);
        }
    }
}
