using System;
using System.Data;
using System.Data.SqlClient;

namespace TiendaUNNE
{
    /// <summary>
    /// Reservas de stock de los tickets en curso. Cada ticket tiene un token propio y reserva
    /// la cantidad de cada producto que tiene cargada; la reserva vence si el ticket se abandona.
    /// Stock libre = Producto.stock − reservas vigentes de OTROS tickets. La hora de vencimiento
    /// la calcula siempre el servidor de base de datos, para que todos los puestos coincidan.
    /// </summary>
    public static class ServicioReserva
    {
        /// <summary>Minutos que dura una reserva sin renovarse.</summary>
        public const int MinutosDeReserva = 30;

        /// <summary>
        /// Deja reservada para el ticket la cantidad indicada (total, no incremento) del producto.
        /// El bloqueo sobre la fila del producto evita que dos cajas reserven lo mismo a la vez.
        /// Lanza StockInsuficienteException si lo que piden supera el stock libre.
        /// </summary>
        public static void Reservar(Guid token, int idProducto, decimal cantidad, int idUsuario)
        {
            const string sqlStock = @"
SELECT stock
FROM   dbo.Producto WITH (UPDLOCK, HOLDLOCK)
WHERE  id_producto = @producto AND activo = 1;";

            const string sqlReservadoPorOtros = @"
SELECT ISNULL(SUM(cantidad), 0)
FROM   dbo.Reserva_stock
WHERE  id_producto = @producto AND token <> @token AND vence > GETDATE();";

            const string sqlGuardar = @"
UPDATE dbo.Reserva_stock
SET    cantidad = @cantidad, vence = DATEADD(MINUTE, @minutos, GETDATE())
WHERE  token = @token AND id_producto = @producto;

IF @@ROWCOUNT = 0
    INSERT INTO dbo.Reserva_stock (token, id_producto, id_usuario, cantidad, vence)
    VALUES (@token, @producto, @usuario, @cantidad, DATEADD(MINUTE, @minutos, GETDATE()));";

            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    decimal stock;
                    using (var cmd = new SqlCommand(sqlStock, cn, tx))
                    {
                        cmd.Parameters.Add("@producto", SqlDbType.Int).Value = idProducto;
                        object r = cmd.ExecuteScalar();
                        if (r == null) throw new StockInsuficienteException(idProducto, 0);
                        stock = (decimal)r;
                    }

                    decimal deOtros;
                    using (var cmd = new SqlCommand(sqlReservadoPorOtros, cn, tx))
                    {
                        cmd.Parameters.Add("@producto", SqlDbType.Int).Value = idProducto;
                        cmd.Parameters.Add("@token", SqlDbType.UniqueIdentifier).Value = token;
                        deOtros = (decimal)cmd.ExecuteScalar();
                    }

                    decimal libre = stock - deOtros;
                    if (cantidad > libre)
                        throw new StockInsuficienteException(idProducto, libre > 0 ? libre : 0);

                    using (var cmd = new SqlCommand(sqlGuardar, cn, tx))
                    {
                        cmd.Parameters.Add("@token", SqlDbType.UniqueIdentifier).Value = token;
                        cmd.Parameters.Add("@producto", SqlDbType.Int).Value = idProducto;
                        cmd.Parameters.Add("@usuario", SqlDbType.Int).Value = idUsuario;
                        cmd.Parameters.Add("@minutos", SqlDbType.Int).Value = MinutosDeReserva;
                        SqlParameter p = cmd.Parameters.Add("@cantidad", SqlDbType.Decimal);
                        p.Precision = 12;
                        p.Scale = 3;
                        p.Value = cantidad;
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        /// <summary>Libera la reserva de un producto del ticket (al quitar el renglón).</summary>
        public static void Liberar(Guid token, int idProducto)
        {
            const string sql = @"
DELETE FROM dbo.Reserva_stock WHERE token = @token AND id_producto = @producto;";

            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@token", SqlDbType.UniqueIdentifier).Value = token;
                cmd.Parameters.Add("@producto", SqlDbType.Int).Value = idProducto;
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Libera todas las reservas del ticket (al cancelarlo o cerrar la pantalla).
        /// De paso borra las reservas vencidas de cualquier ticket.</summary>
        public static void LiberarTodas(Guid token)
        {
            const string sql = @"
DELETE FROM dbo.Reserva_stock WHERE token = @token OR vence <= GETDATE();";

            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@token", SqlDbType.UniqueIdentifier).Value = token;
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Extiende el vencimiento de todas las reservas del ticket mientras se sigue usando.</summary>
        public static void Renovar(Guid token)
        {
            const string sql = @"
UPDATE dbo.Reserva_stock
SET    vence = DATEADD(MINUTE, @minutos, GETDATE())
WHERE  token = @token;";

            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@token", SqlDbType.UniqueIdentifier).Value = token;
                cmd.Parameters.Add("@minutos", SqlDbType.Int).Value = MinutosDeReserva;
                cmd.ExecuteNonQuery();
            }
        }
    }
}
