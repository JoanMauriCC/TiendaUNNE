using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace TiendaUNNE
{
    /// <summary>
    /// Acceso a datos de Producto: conexión, SQL y transacciones. Nada más.
    /// Los datos llegan validados y normalizados desde NegocioProducto, y los textos
    /// de auditoría vienen armados desde ahí.
    /// </summary>
    public static class ServicioProducto
    {
        // ---------------------------------------------------------------------
        // Consultas
        // ---------------------------------------------------------------------

        /// <summary>
        /// Listado para la grilla: productos con nombre de categoría, filtrados por estado.
        /// <paramref name="activos"/> en true trae los activos; en false, los dados de baja.
        /// </summary>
        public static DataTable Listar(bool activos)
        {
            const string sql = @"
SELECT  p.id_producto    AS IdProducto,
        p.nombre         AS Nombre,
        c.nombre         AS Categoria,
        p.precio_venta   AS PrecioVenta,
        p.stock          AS Stock,
        p.stock_minimo   AS StockMinimo,
        p.activo         AS Activo
FROM        dbo.Producto  p
INNER JOIN  dbo.Categoria c ON c.id_categoria = p.id_categoria
WHERE p.activo = @activo
ORDER BY p.nombre;";

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

        /// <summary>
        /// Productos activos cuyo nombre contiene el texto buscado, con su precio y
        /// stock actual. Lo usa la caja para armar el ticket.
        /// </summary>
        public static List<ProductoVentaItem> BuscarParaVenta(string texto)
        {
            const string sql = @"
SELECT TOP 50 id_producto, nombre, precio_venta, stock
FROM   dbo.Producto
WHERE  activo = 1 AND nombre LIKE @texto
ORDER BY nombre;";

            var lista = new List<ProductoVentaItem>();
            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@texto", SqlDbType.NVarChar, 150).Value = "%" + texto + "%";

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new ProductoVentaItem
                        {
                            IdProducto = (int)dr["id_producto"],
                            Nombre = (string)dr["nombre"],
                            PrecioVenta = (decimal)dr["precio_venta"],
                            Stock = (decimal)dr["stock"]
                        });
                    }
                }
            }
            return lista;
        }

        /// <summary>Trae un producto, o null si no existe. Interpretar el null es cosa de Negocio.</summary>
        public static ProductoEditModel Obtener(int idProducto)
        {
            const string sql = @"
SELECT  p.id_producto, p.id_categoria, p.nombre, p.descripcion,
        p.precio_venta, p.stock, p.stock_minimo,
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
                        return null;

                    return new ProductoEditModel
                    {
                        IdProducto = (int)dr["id_producto"],
                        IdCategoria = (int)dr["id_categoria"],
                        NombreCategoria = (string)dr["categoria_nombre"],
                        Nombre = (string)dr["nombre"],
                        Descripcion = dr["descripcion"] as string,
                        PrecioVenta = (decimal)dr["precio_venta"],
                        Stock = (decimal)dr["stock"],
                        StockMinimo = (decimal)dr["stock_minimo"]
                    };
                }
            }
        }

        // ---------------------------------------------------------------------
        // Alta
        // ---------------------------------------------------------------------

        public static int Crear(ProductoEditModel m, int idUsuarioSesion, string resumenNuevo)
        {
            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    const string sql = @"
INSERT INTO dbo.Producto
    (id_categoria, nombre, descripcion, precio_venta, stock, stock_minimo)
VALUES
    (@id_categoria, @nombre, @descripcion, @precio_venta, @stock, @stock_minimo);
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
                        valorNuevo: resumenNuevo,
                        idUsuario: idUsuarioSesion,
                        cn: cn, tx: tx);

                    tx.Commit();
                    return idProducto;
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

        public static void Actualizar(ProductoEditModel m, int idUsuarioSesion,
                                      string resumenAnterior, string resumenNuevo)
        {
            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    const string sql = @"
UPDATE dbo.Producto
SET id_categoria = @id_categoria,
    nombre       = @nombre,
    descripcion  = @descripcion,
    precio_venta = @precio_venta,
    stock        = @stock,
    stock_minimo = @stock_minimo
WHERE id_producto = @id_producto;";

                    using (var cmd = new SqlCommand(sql, cn, tx))
                    {
                        AgregarParams(cmd, m);
                        cmd.Parameters.Add("@id_producto", SqlDbType.Int).Value = m.IdProducto;
                        cmd.ExecuteNonQuery();
                    }

                    ServicioAuditoria.Registrar(
                        "MODIFICACION", "Producto", m.IdProducto,
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

        /// <summary>Devuelve la cantidad de filas afectadas; 0 significa que ya estaba inactivo.</summary>
        public static int DarDeBaja(int idProducto, int idUsuarioSesion, string resumenAnterior)
        {
            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    int filas;
                    using (var cmd = new SqlCommand(
                        "UPDATE dbo.Producto SET activo = 0 WHERE id_producto = @id AND activo = 1;", cn, tx))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProducto;
                        filas = cmd.ExecuteNonQuery();
                    }

                    if (filas > 0)
                    {
                        ServicioAuditoria.Registrar(
                            "BAJA", "Producto", idProducto,
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
        /// Reactivación: UPDATE Producto SET activo = 1 + Auditoria (MODIFICACION).
        /// Devuelve la cantidad de filas afectadas; 0 significa que ya estaba activo.
        /// </summary>
        public static int DarDeAlta(int idProducto, int idUsuarioSesion, string resumenAnterior)
        {
            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    int filas;
                    using (var cmd = new SqlCommand(
                        "UPDATE dbo.Producto SET activo = 1 WHERE id_producto = @id AND activo = 0;", cn, tx))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProducto;
                        filas = cmd.ExecuteNonQuery();
                    }

                    if (filas > 0)
                    {
                        ServicioAuditoria.Registrar(
                            "MODIFICACION", "Producto", idProducto,
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

        private static void AgregarParams(SqlCommand cmd, ProductoEditModel m)
        {
            cmd.Parameters.Add("@id_categoria", SqlDbType.Int).Value = m.IdCategoria;
            cmd.Parameters.Add("@nombre", SqlDbType.NVarChar, 150).Value = m.Nombre;
            cmd.Parameters.Add("@descripcion", SqlDbType.NVarChar, 500).Value = Nz(m.Descripcion);
            cmd.Parameters.Add("@precio_venta", SqlDbType.Decimal).Value = m.PrecioVenta;
            cmd.Parameters.Add("@stock", SqlDbType.Decimal).Value = m.Stock;
            cmd.Parameters.Add("@stock_minimo", SqlDbType.Decimal).Value = m.StockMinimo;
        }

        /// <summary>Mapea null de C# a NULL de SQL. El recorte de espacios ya lo hizo Negocio.</summary>
        private static object Nz(string s) => (object)s ?? DBNull.Value;
    }
}
