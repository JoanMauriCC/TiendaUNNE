using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace TiendaUNNE
{
    /// <summary>
    /// Acceso a datos de las ventas: conexión, SQL y la transacción que las guarda.
    /// Qué comprobante se emite, cuánto vuelto corresponde y cómo quedan los pagos lo
    /// decide NegocioVenta; acá llega todo resuelto.
    /// </summary>
    public static class ServicioVenta
    {
        /// <summary>Id del tipo de comprobante con ese código, o null si no está cargado.</summary>
        public static int? ObtenerIdTipoComprobante(string codigo)
        {
            const string sql = @"
SELECT id_tipo_comprobante
FROM   dbo.Tipo_comprobante
WHERE  codigo = @codigo AND activo = 1;";

            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@codigo", SqlDbType.NVarChar, 5).Value = codigo;

                object id = cmd.ExecuteScalar();
                return id == null ? (int?)null : (int)id;
            }
        }

        // ---------------------------------------------------------------------
        // Ventas del turno y anulación
        // ---------------------------------------------------------------------

        private const string SelectVentas = @"
SELECT  vc.id_venta, vc.fecha_hora, tc.nombre AS comprobante, vc.punto_venta,
        vc.numero_comprobante, vc.total, vc.estado, vc.observaciones,
        ISNULL(p.apellido + N', ' + p.nombre, N'Consumidor final') AS cliente
FROM        dbo.Venta_cabecera    vc
INNER JOIN  dbo.Tipo_comprobante  tc ON tc.id_tipo_comprobante = vc.id_tipo_comprobante
LEFT JOIN   dbo.Cliente           c  ON c.id_cliente = vc.id_cliente
LEFT JOIN   dbo.Persona           p  ON p.id_persona = c.id_persona
";

        /// <summary>Todas las ventas de un turno de caja, las más nuevas primero.</summary>
        public static List<VentaListada> ListarDelTurno(int idCajaSesion)
        {
            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(
                SelectVentas + "WHERE vc.id_caja_sesion = @sesion ORDER BY vc.id_venta DESC;", cn))
            {
                cmd.Parameters.Add("@sesion", SqlDbType.Int).Value = idCajaSesion;
                return LeerVentas(cmd);
            }
        }

        /// <summary>Una venta por su id, o null si no existe.</summary>
        public static VentaListada ObtenerVenta(int idVenta)
        {
            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(SelectVentas + "WHERE vc.id_venta = @id;", cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idVenta;

                List<VentaListada> lista = LeerVentas(cmd);
                return lista.Count == 0 ? null : lista[0];
            }
        }

        private static List<VentaListada> LeerVentas(SqlCommand cmd)
        {
            var lista = new List<VentaListada>();
            using (var dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    lista.Add(new VentaListada
                    {
                        IdVenta = (int)dr["id_venta"],
                        Fecha = (DateTime)dr["fecha_hora"],
                        Comprobante = (string)dr["comprobante"],
                        PuntoVenta = (int)dr["punto_venta"],
                        Numero = (long)dr["numero_comprobante"],
                        Cliente = (string)dr["cliente"],
                        Total = (decimal)dr["total"],
                        Estado = (string)dr["estado"],
                        Motivo = dr["observaciones"] as string
                    });
                }
            }
            return lista;
        }

        /// <summary>
        /// Anula la venta con el procedimiento sp_AnularVenta. Si se anuló, deja la auditoría
        /// (MODIFICACION) en la misma transacción: o queda todo o no queda nada.
        /// </summary>
        public static ResultadoAnulacion Anular(int idVenta, int idCajaSesion, int idUsuario, string motivo,
                                                string resumenAnterior, string resumenNuevo)
        {
            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    ResultadoAnulacion resultado;
                    using (var cmd = new SqlCommand("dbo.sp_AnularVenta", cn, tx))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@id_venta", SqlDbType.Int).Value = idVenta;
                        cmd.Parameters.Add("@id_caja_sesion", SqlDbType.Int).Value = idCajaSesion;
                        cmd.Parameters.Add("@id_usuario", SqlDbType.Int).Value = idUsuario;
                        cmd.Parameters.Add("@motivo", SqlDbType.NVarChar, 300).Value = motivo;

                        SqlParameter salida = cmd.Parameters.Add("@resultado", SqlDbType.Int);
                        salida.Direction = ParameterDirection.Output;

                        cmd.ExecuteNonQuery();
                        resultado = (ResultadoAnulacion)(int)salida.Value;
                    }

                    if (resultado == ResultadoAnulacion.Anulada)
                    {
                        ServicioAuditoria.Registrar(
                            "MODIFICACION", "Venta_cabecera", idVenta,
                            resumenAnterior, resumenNuevo, idUsuario,
                            cn, tx);
                    }

                    tx.Commit();
                    return resultado;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
        /// <summary>
        /// Guarda la venta completa en una transacción: la cabecera, los renglones, los pagos
        /// y un ingreso de caja por cada pago. Si algo falla no queda nada a medias.
        /// Devuelve el número de comprobante asignado (correlativo por tipo de comprobante y punto
        /// de venta) y la fecha y hora en que quedó registrada.
        /// </summary>
        public static VentaRegistrada Registrar(VentaEditModel venta, IList<PagoVenta> pagos,
                                     int idTipoComprobante, int puntoVenta, string conceptoCaja)
        {
            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    long numero = SiguienteNumero(cn, tx, idTipoComprobante, puntoVenta);
                    DateTime fecha;
                    int idVenta = InsertarCabecera(cn, tx, venta, idTipoComprobante, puntoVenta, numero, out fecha);
                    InsertarRenglones(cn, tx, idVenta, venta.Renglones);
                    DescontarStock(cn, tx, venta);
                    InsertarPagos(cn, tx, venta, idVenta, pagos, conceptoCaja);

                    tx.Commit();
                    return new VentaRegistrada { IdVenta = idVenta, Numero = numero, FechaHora = fecha };
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// El bloqueo (UPDLOCK, HOLDLOCK) evita que dos cobros simultáneos lean el mismo
        /// último número; UQ_VentaCab_numero es la red de seguridad.
        /// </summary>
        private static long SiguienteNumero(SqlConnection cn, SqlTransaction tx,
                                            int idTipoComprobante, int puntoVenta)
        {
            const string sql = @"
SELECT ISNULL(MAX(numero_comprobante), 0) + 1
FROM   dbo.Venta_cabecera WITH (UPDLOCK, HOLDLOCK)
WHERE  id_tipo_comprobante = @tipo AND punto_venta = @punto_venta;";

            using (var cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.Parameters.Add("@tipo", SqlDbType.Int).Value = idTipoComprobante;
                cmd.Parameters.Add("@punto_venta", SqlDbType.Int).Value = puntoVenta;
                return (long)cmd.ExecuteScalar();
            }
        }

        private static int InsertarCabecera(SqlConnection cn, SqlTransaction tx, VentaEditModel venta,
                                            int idTipoComprobante, int puntoVenta, long numero,
                                            out DateTime fechaHora)
        {
            const string sql = @"
INSERT INTO dbo.Venta_cabecera
    (id_tipo_comprobante, punto_venta, numero_comprobante, id_cliente, id_usuario, id_caja_sesion,
     subtotal, descuento, importe_neto, importe_iva, total)
VALUES
    (@tipo, @punto_venta, @numero, @cliente, @usuario, @sesion, @total, 0, @neto, @iva, @total);
SELECT id_venta, fecha_hora FROM dbo.Venta_cabecera WHERE id_venta = SCOPE_IDENTITY();";

            using (var cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.Parameters.Add("@tipo", SqlDbType.Int).Value = idTipoComprobante;
                cmd.Parameters.Add("@punto_venta", SqlDbType.Int).Value = puntoVenta;
                cmd.Parameters.Add("@numero", SqlDbType.BigInt).Value = numero;
                cmd.Parameters.Add("@cliente", SqlDbType.Int).Value =
                    venta.Cliente == null ? (object)DBNull.Value : venta.Cliente.IdCliente;
                cmd.Parameters.Add("@usuario", SqlDbType.Int).Value = venta.IdUsuario;
                cmd.Parameters.Add("@sesion", SqlDbType.Int).Value = venta.IdCajaSesion;
                AgregarDecimal(cmd, "@total", venta.Total, 12, 2);
                AgregarDecimal(cmd, "@neto", venta.ImporteNeto, 12, 2);
                AgregarDecimal(cmd, "@iva", venta.ImporteIva, 12, 2);

                using (var dr = cmd.ExecuteReader())
                {
                    dr.Read();
                    fechaHora = dr.GetDateTime(1);
                    return dr.GetInt32(0);
                }
            }
        }

        private static void InsertarRenglones(SqlConnection cn, SqlTransaction tx, int idVenta,
                                              IList<RenglonVenta> renglones)
        {
            const string sql = @"
INSERT INTO dbo.Venta_detalle
    (id_venta, id_producto, nro_linea, descripcion, cantidad, precio_unitario,
     alicuota_iva, importe_iva, subtotal)
VALUES
    (@venta, @producto, @linea, @descripcion, @cantidad, @precio, @alicuota, @iva, @subtotal);";

            for (int i = 0; i < renglones.Count; i++)
            {
                RenglonVenta r = renglones[i];

                using (var cmd = new SqlCommand(sql, cn, tx))
                {
                    cmd.Parameters.Add("@venta", SqlDbType.Int).Value = idVenta;
                    cmd.Parameters.Add("@producto", SqlDbType.Int).Value = r.IdProducto;
                    cmd.Parameters.Add("@linea", SqlDbType.Int).Value = i + 1;
                    cmd.Parameters.Add("@descripcion", SqlDbType.NVarChar, 150).Value = r.Descripcion;
                    AgregarDecimal(cmd, "@cantidad", r.Cantidad, 12, 3);
                    AgregarDecimal(cmd, "@precio", r.PrecioUnitario, 12, 2);
                    AgregarDecimal(cmd, "@subtotal", r.Subtotal, 12, 2);
                    AgregarDecimal(cmd, "@alicuota", r.AlicuotaIva, 5, 2);
                    AgregarDecimal(cmd, "@iva", r.ImporteIva, 12, 2);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Descuenta del stock lo vendido y borra las reservas del ticket, en la misma transacción
        /// que la venta. Se bloquea la fila del producto y se comprueba que, además de alcanzar el
        /// stock, no se pise lo que otros tickets tienen reservado.
        /// </summary>
        private static void DescontarStock(SqlConnection cn, SqlTransaction tx, VentaEditModel venta)
        {
            const string sqlLeer = @"
SELECT p.stock,
       (SELECT ISNULL(SUM(rs.cantidad), 0)
        FROM   dbo.Reserva_stock rs
        WHERE  rs.id_producto = p.id_producto AND rs.token <> @token AND rs.vence > GETDATE())
FROM   dbo.Producto p WITH (UPDLOCK, HOLDLOCK)
WHERE  p.id_producto = @producto;";

            const string sqlDescontar = @"
UPDATE dbo.Producto SET stock = stock - @cantidad WHERE id_producto = @producto;";

            const string sqlLiberar = @"
DELETE FROM dbo.Reserva_stock WHERE token = @token;";

            foreach (RenglonVenta r in venta.Renglones)
            {
                decimal libre;
                using (var cmd = new SqlCommand(sqlLeer, cn, tx))
                {
                    cmd.Parameters.Add("@producto", SqlDbType.Int).Value = r.IdProducto;
                    cmd.Parameters.Add("@token", SqlDbType.UniqueIdentifier).Value = venta.Token;

                    using (var dr = cmd.ExecuteReader())
                    {
                        libre = dr.Read() ? dr.GetDecimal(0) - dr.GetDecimal(1) : 0;
                    }
                }

                if (r.Cantidad > libre)
                    throw new StockInsuficienteException(r.IdProducto, libre > 0 ? libre : 0);

                using (var cmd = new SqlCommand(sqlDescontar, cn, tx))
                {
                    cmd.Parameters.Add("@producto", SqlDbType.Int).Value = r.IdProducto;
                    AgregarDecimal(cmd, "@cantidad", r.Cantidad, 12, 3);
                    cmd.ExecuteNonQuery();
                }
            }

            using (var cmd = new SqlCommand(sqlLiberar, cn, tx))
            {
                cmd.Parameters.Add("@token", SqlDbType.UniqueIdentifier).Value = venta.Token;
                cmd.ExecuteNonQuery();
            }
        }

        private static void InsertarPagos(SqlConnection cn, SqlTransaction tx, VentaEditModel venta,
                                          int idVenta, IList<PagoVenta> pagos, string conceptoCaja)
        {
            const string sqlPago = @"
INSERT INTO dbo.Venta_pago (id_venta, id_medio_pago, importe, referencia)
VALUES (@venta, @medio, @importe, @referencia);";

            const string sqlMovimiento = @"
INSERT INTO dbo.Movimiento_caja
    (id_caja_sesion, id_usuario, id_medio_pago, id_venta, tipo, concepto, monto)
VALUES
    (@sesion, @usuario, @medio, @venta, 'INGRESO', @concepto, @importe);";

            foreach (PagoVenta pago in pagos)
            {
                using (var cmd = new SqlCommand(sqlPago, cn, tx))
                {
                    cmd.Parameters.Add("@venta", SqlDbType.Int).Value = idVenta;
                    cmd.Parameters.Add("@medio", SqlDbType.Int).Value = pago.IdMedioPago;
                    AgregarDecimal(cmd, "@importe", pago.Importe, 12, 2);
                    cmd.Parameters.Add("@referencia", SqlDbType.NVarChar, 100).Value =
                        (object)pago.Referencia ?? DBNull.Value;
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new SqlCommand(sqlMovimiento, cn, tx))
                {
                    cmd.Parameters.Add("@sesion", SqlDbType.Int).Value = venta.IdCajaSesion;
                    cmd.Parameters.Add("@usuario", SqlDbType.Int).Value = venta.IdUsuario;
                    cmd.Parameters.Add("@medio", SqlDbType.Int).Value = pago.IdMedioPago;
                    cmd.Parameters.Add("@venta", SqlDbType.Int).Value = idVenta;
                    cmd.Parameters.Add("@concepto", SqlDbType.NVarChar, 150).Value = conceptoCaja;
                    AgregarDecimal(cmd, "@importe", pago.Importe, 12, 2);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void AgregarDecimal(SqlCommand cmd, string nombre, decimal valor,
                                           byte precision, byte escala)
        {
            SqlParameter p = cmd.Parameters.Add(nombre, SqlDbType.Decimal);
            p.Precision = precision;
            p.Scale = escala;
            p.Value = valor;
        }
    }
}
