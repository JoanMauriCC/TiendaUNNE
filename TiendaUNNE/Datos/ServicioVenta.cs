using System;
using System.Data;
using System.Data.SqlClient;

namespace TiendaUNNE
{
    /// <summary>
    /// Acceso a datos de la venta. Todo el registro ocurre dentro de UNA transacción:
    /// cabecera, renglones, pagos, movimientos de caja, descuento de stock y auditoría.
    /// Si falla cualquier paso no queda nada a medias (por ejemplo, stock descontado
    /// sin venta registrada).
    /// </summary>
    public static class ServicioVenta
    {
        /// <summary>Id del tipo de comprobante activo con ese código, o null si no existe.</summary>
        public static int? ObtenerIdTipoComprobante(string codigo)
        {
            const string sql = @"
SELECT TOP 1 id_tipo_comprobante
FROM   dbo.Tipo_comprobante
WHERE  codigo = @codigo AND activo = 1;";

            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@codigo", SqlDbType.NVarChar, 5).Value = codigo;
                object resultado = cmd.ExecuteScalar();

                if (resultado == null || resultado == DBNull.Value)
                    return null;

                return (int)resultado;
            }
        }

        /// <summary>
        /// Registra la venta completa y devuelve el número de comprobante asignado.
        /// Los datos llegan validados desde NegocioVenta.
        /// </summary>
        public static long RegistrarVenta(VentaEditModel venta, int idTipoComprobante,
                                          int puntoVenta, string resumenAuditoria)
        {
            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    bool afectaStock, afectaCaja;
                    LeerFlagsComprobante(cn, tx, idTipoComprobante, out afectaStock, out afectaCaja);

                    long numero = SiguienteNumero(cn, tx, idTipoComprobante, puntoVenta);
                    int idVenta = InsertarCabecera(cn, tx, venta, idTipoComprobante, puntoVenta, numero);

                    InsertarRenglones(cn, tx, venta, idVenta);
                    InsertarPagos(cn, tx, venta, idVenta);

                    if (afectaCaja)
                        InsertarMovimientosDeCaja(cn, tx, venta, idVenta, numero);

                    if (afectaStock)
                        DescontarStock(cn, tx, venta);

                    ServicioAuditoria.Registrar(
                        "ALTA", "Venta", idVenta,
                        valorAnterior: null,
                        valorNuevo: resumenAuditoria,
                        idUsuario: venta.IdUsuario,
                        cn: cn, tx: tx);

                    tx.Commit();
                    return numero;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        // ---------------------------------------------------------------------
        // Pasos de la transacción
        // ---------------------------------------------------------------------

        private static void LeerFlagsComprobante(SqlConnection cn, SqlTransaction tx,
                                                 int idTipoComprobante,
                                                 out bool afectaStock, out bool afectaCaja)
        {
            const string sql = @"
SELECT afecta_stock, afecta_caja
FROM   dbo.Tipo_comprobante
WHERE  id_tipo_comprobante = @id;";

            using (var cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idTipoComprobante;
                using (var dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!dr.Read())
                        throw new InvalidOperationException(
                            "El tipo de comprobante configurado ya no existe.");

                    afectaStock = (bool)dr["afecta_stock"];
                    afectaCaja = (bool)dr["afecta_caja"];
                }
            }
        }

        /// <summary>
        /// Siguiente número para ese tipo y punto de venta. Los lock hints mantienen
        /// el rango bloqueado hasta el commit, así dos ventas simultáneas no pueden
        /// sacar el mismo número (hay un UNIQUE que lo rechazaría).
        /// </summary>
        private static long SiguienteNumero(SqlConnection cn, SqlTransaction tx,
                                            int idTipoComprobante, int puntoVenta)
        {
            const string sql = @"
SELECT ISNULL(MAX(numero_comprobante), 0) + 1
FROM   dbo.Venta_cabecera WITH (UPDLOCK, HOLDLOCK)
WHERE  id_tipo_comprobante = @tipo AND punto_venta = @punto;";

            using (var cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.Parameters.Add("@tipo", SqlDbType.Int).Value = idTipoComprobante;
                cmd.Parameters.Add("@punto", SqlDbType.Int).Value = puntoVenta;
                return Convert.ToInt64(cmd.ExecuteScalar());
            }
        }

        private static int InsertarCabecera(SqlConnection cn, SqlTransaction tx,
                                            VentaEditModel venta, int idTipoComprobante,
                                            int puntoVenta, long numero)
        {
            // El ticket trabaja con precios finales: no se discrimina IVA ni se aplican
            // descuentos, así que subtotal, neto y total son el mismo importe.
            const string sql = @"
INSERT INTO dbo.Venta_cabecera
    (id_tipo_comprobante, punto_venta, numero_comprobante, id_cliente, id_usuario,
     id_caja_sesion, subtotal, descuento, importe_neto, importe_iva, total)
VALUES
    (@tipo, @punto, @numero, NULL, @id_usuario,
     @id_sesion, @total, 0, @total, 0, @total);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.Parameters.Add("@tipo", SqlDbType.Int).Value = idTipoComprobante;
                cmd.Parameters.Add("@punto", SqlDbType.Int).Value = puntoVenta;
                cmd.Parameters.Add("@numero", SqlDbType.BigInt).Value = numero;
                cmd.Parameters.Add("@id_usuario", SqlDbType.Int).Value = venta.IdUsuario;
                cmd.Parameters.Add("@id_sesion", SqlDbType.Int).Value = venta.IdCajaSesion;
                cmd.Parameters.Add("@total", SqlDbType.Decimal).Value = venta.Total;
                return (int)cmd.ExecuteScalar();
            }
        }

        private static void InsertarRenglones(SqlConnection cn, SqlTransaction tx,
                                              VentaEditModel venta, int idVenta)
        {
            const string sql = @"
INSERT INTO dbo.Venta_detalle
    (id_venta, id_producto, nro_linea, descripcion, cantidad, precio_unitario,
     descuento_porcentaje, alicuota_iva, importe_iva, subtotal)
VALUES
    (@id_venta, @id_producto, @linea, @descripcion, @cantidad, @precio, 0, 0, 0, @subtotal);";

            int linea = 1;
            foreach (RenglonVenta renglon in venta.Renglones)
            {
                using (var cmd = new SqlCommand(sql, cn, tx))
                {
                    cmd.Parameters.Add("@id_venta", SqlDbType.Int).Value = idVenta;
                    cmd.Parameters.Add("@id_producto", SqlDbType.Int).Value = renglon.IdProducto;
                    cmd.Parameters.Add("@linea", SqlDbType.Int).Value = linea;
                    cmd.Parameters.Add("@descripcion", SqlDbType.NVarChar, 150).Value = renglon.Descripcion;
                    cmd.Parameters.Add("@cantidad", SqlDbType.Decimal).Value = renglon.Cantidad;
                    cmd.Parameters.Add("@precio", SqlDbType.Decimal).Value = renglon.PrecioUnitario;
                    cmd.Parameters.Add("@subtotal", SqlDbType.Decimal).Value = renglon.Subtotal;
                    cmd.ExecuteNonQuery();
                }
                linea++;
            }
        }

        private static void InsertarPagos(SqlConnection cn, SqlTransaction tx,
                                          VentaEditModel venta, int idVenta)
        {
            const string sql = @"
INSERT INTO dbo.Venta_pago (id_venta, id_medio_pago, importe, recargo, referencia)
VALUES (@id_venta, @id_medio, @importe, 0, @referencia);";

            foreach (PagoVenta pago in venta.Pagos)
            {
                using (var cmd = new SqlCommand(sql, cn, tx))
                {
                    cmd.Parameters.Add("@id_venta", SqlDbType.Int).Value = idVenta;
                    cmd.Parameters.Add("@id_medio", SqlDbType.Int).Value = pago.IdMedioPago;
                    cmd.Parameters.Add("@importe", SqlDbType.Decimal).Value = pago.Importe;
                    cmd.Parameters.Add("@referencia", SqlDbType.NVarChar, 100).Value =
                        (object)pago.Referencia ?? DBNull.Value;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void InsertarMovimientosDeCaja(SqlConnection cn, SqlTransaction tx,
                                                      VentaEditModel venta, int idVenta, long numero)
        {
            const string sql = @"
INSERT INTO dbo.Movimiento_caja
    (id_caja_sesion, id_usuario, id_medio_pago, id_venta, tipo, concepto, monto)
VALUES
    (@id_sesion, @id_usuario, @id_medio, @id_venta, 'INGRESO', @concepto, @monto);";

            string concepto = "Venta Nº " + numero;

            foreach (PagoVenta pago in venta.Pagos)
            {
                using (var cmd = new SqlCommand(sql, cn, tx))
                {
                    cmd.Parameters.Add("@id_sesion", SqlDbType.Int).Value = venta.IdCajaSesion;
                    cmd.Parameters.Add("@id_usuario", SqlDbType.Int).Value = venta.IdUsuario;
                    cmd.Parameters.Add("@id_medio", SqlDbType.Int).Value = pago.IdMedioPago;
                    cmd.Parameters.Add("@id_venta", SqlDbType.Int).Value = idVenta;
                    cmd.Parameters.Add("@concepto", SqlDbType.NVarChar, 150).Value = concepto;
                    cmd.Parameters.Add("@monto", SqlDbType.Decimal).Value = pago.Importe;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Descuenta el stock vendido. La condición stock >= cantidad evita dejarlo en
        /// negativo si otra venta se adelantó entre la validación y el commit.
        /// </summary>
        private static void DescontarStock(SqlConnection cn, SqlTransaction tx, VentaEditModel venta)
        {
            const string sql = @"
UPDATE dbo.Producto
SET stock = stock - @cantidad
WHERE id_producto = @id_producto AND stock >= @cantidad;";

            foreach (RenglonVenta renglon in venta.Renglones)
            {
                using (var cmd = new SqlCommand(sql, cn, tx))
                {
                    cmd.Parameters.Add("@cantidad", SqlDbType.Decimal).Value = renglon.Cantidad;
                    cmd.Parameters.Add("@id_producto", SqlDbType.Int).Value = renglon.IdProducto;

                    if (cmd.ExecuteNonQuery() == 0)
                        throw new StockInsuficienteException(renglon.IdProducto, renglon.Descripcion);
                }
            }
        }
    }
}
