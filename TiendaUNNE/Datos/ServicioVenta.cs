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

        /// <summary>
        /// Guarda la venta completa en una transacción: la cabecera, los renglones, los pagos
        /// y un ingreso de caja por cada pago. Si algo falla no queda nada a medias.
        /// Devuelve el número de comprobante asignado, correlativo por tipo de comprobante
        /// y punto de venta.
        /// </summary>
        public static long Registrar(VentaEditModel venta, IList<PagoVenta> pagos,
                                     int idTipoComprobante, int puntoVenta, string conceptoCaja)
        {
            using (var cn = Db.AbrirConexion())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    long numero = SiguienteNumero(cn, tx, idTipoComprobante, puntoVenta);
                    int idVenta = InsertarCabecera(cn, tx, venta, idTipoComprobante, puntoVenta, numero);
                    InsertarRenglones(cn, tx, idVenta, venta.Renglones);
                    InsertarPagos(cn, tx, venta, idVenta, pagos, conceptoCaja);

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
                                            int idTipoComprobante, int puntoVenta, long numero)
        {
            const string sql = @"
INSERT INTO dbo.Venta_cabecera
    (id_tipo_comprobante, punto_venta, numero_comprobante, id_usuario, id_caja_sesion,
     subtotal, descuento, importe_neto, importe_iva, total)
VALUES
    (@tipo, @punto_venta, @numero, @usuario, @sesion, @total, 0, @total, 0, @total);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.Parameters.Add("@tipo", SqlDbType.Int).Value = idTipoComprobante;
                cmd.Parameters.Add("@punto_venta", SqlDbType.Int).Value = puntoVenta;
                cmd.Parameters.Add("@numero", SqlDbType.BigInt).Value = numero;
                cmd.Parameters.Add("@usuario", SqlDbType.Int).Value = venta.IdUsuario;
                cmd.Parameters.Add("@sesion", SqlDbType.Int).Value = venta.IdCajaSesion;
                AgregarDecimal(cmd, "@total", venta.Total, 12, 2);

                return (int)cmd.ExecuteScalar();
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
    (@venta, @producto, @linea, @descripcion, @cantidad, @precio, 0, 0, @subtotal);";

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
                    cmd.ExecuteNonQuery();
                }
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
