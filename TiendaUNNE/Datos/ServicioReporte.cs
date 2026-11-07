using System;
using System.Data;
using System.Data.SqlClient;

namespace TiendaUNNE
{
    /// <summary>
    /// Consultas de los reportes: solo SQL, sin decidir nada. El rango llega ya resuelto
    /// desde NegocioReporte como [desde, hasta): incluye "desde" y no incluye "hasta".
    ///
    /// Cuentan los comprobantes emitidos. Cada tipo de comprobante tiene un "signo": 1 en una venta
    /// (Ticket, Factura A y B) y -1 en una nota de crédito. Los importes y las unidades se multiplican
    /// por ese signo, así una devolución RESTA y los totales son netos; y las notas de crédito no se
    /// cuentan como ventas (la cantidad de ventas solo cuenta las de signo 1).
    /// </summary>
    public static class ServicioReporte
    {
        // Ventas con su tipo de comprobante, en el rango pedido: base de casi todas las consultas.
        private const string VentasEnRango = @"
FROM        dbo.Venta_cabecera   vc
INNER JOIN  dbo.Tipo_comprobante tc ON tc.id_tipo_comprobante = vc.id_tipo_comprobante
WHERE vc.estado = 'EMITIDA' AND vc.fecha_hora >= @desde AND vc.fecha_hora < @hasta";

        // Pagos con el tipo de comprobante de su venta, en el rango pedido.
        private const string PagosEnRango = @"
FROM        dbo.Venta_pago       vp
INNER JOIN  dbo.Venta_cabecera   vc ON vc.id_venta      = vp.id_venta
INNER JOIN  dbo.Tipo_comprobante tc ON tc.id_tipo_comprobante = vc.id_tipo_comprobante
INNER JOIN  dbo.Medio_pago       mp ON mp.id_medio_pago = vp.id_medio_pago
WHERE vc.estado = 'EMITIDA' AND vc.fecha_hora >= @desde AND vc.fecha_hora < @hasta";

        /// <summary>Una fila: Cantidad de ventas y Total vendido (neto de notas de crédito).</summary>
        public static DataTable ResumenVentas(DateTime desde, DateTime hasta)
        {
            return Consultar(@"
SELECT COUNT(CASE WHEN tc.signo = 1 THEN 1 END) AS Cantidad,
       ISNULL(SUM(vc.total * tc.signo), 0)      AS Total"
                + VentasEnRango + ";",
                desde, hasta);
        }

        /// <summary>Hora (0-23), Ventas y Total de cada hora en que hubo movimientos.</summary>
        public static DataTable VentasPorHora(DateTime desde, DateTime hasta)
        {
            return Consultar(@"
SELECT DATEPART(HOUR, vc.fecha_hora) AS Hora,
       COUNT(CASE WHEN tc.signo = 1 THEN 1 END) AS Ventas,
       SUM(vc.total * tc.signo) AS Total"
                + VentasEnRango + @"
GROUP BY DATEPART(HOUR, vc.fecha_hora)
ORDER BY Hora;",
                desde, hasta);
        }

        /// <summary>Fecha, Ventas y Total de cada día en que hubo movimientos.</summary>
        public static DataTable VentasPorDia(DateTime desde, DateTime hasta)
        {
            return Consultar(@"
SELECT CAST(vc.fecha_hora AS DATE) AS Fecha,
       COUNT(CASE WHEN tc.signo = 1 THEN 1 END) AS Ventas,
       SUM(vc.total * tc.signo) AS Total"
                + VentasEnRango + @"
GROUP BY CAST(vc.fecha_hora AS DATE)
ORDER BY Fecha;",
                desde, hasta);
        }

        /// <summary>Anio, Mes, Ventas y Total de cada mes en que hubo movimientos.</summary>
        public static DataTable VentasPorMes(DateTime desde, DateTime hasta)
        {
            return Consultar(@"
SELECT YEAR(vc.fecha_hora) AS Anio, MONTH(vc.fecha_hora) AS Mes,
       COUNT(CASE WHEN tc.signo = 1 THEN 1 END) AS Ventas,
       SUM(vc.total * tc.signo) AS Total"
                + VentasEnRango + @"
GROUP BY YEAR(vc.fecha_hora), MONTH(vc.fecha_hora)
ORDER BY Anio, Mes;",
                desde, hasta);
        }

        /// <summary>
        /// Numero, Fecha, Total (negativo en una nota de crédito) y EsNotaCredito de cada comprobante,
        /// del más nuevo al más viejo.
        /// </summary>
        public static DataTable DetalleVentas(DateTime desde, DateTime hasta)
        {
            return Consultar(@"
SELECT vc.numero_comprobante AS Numero, vc.fecha_hora AS Fecha,
       vc.total * tc.signo AS Total,
       CAST(CASE WHEN tc.signo < 0 THEN 1 ELSE 0 END AS BIT) AS EsNotaCredito"
                + VentasEnRango + @"
ORDER BY vc.fecha_hora DESC;",
                desde, hasta);
        }

        /// <summary>Una fila: Unidades vendidas (netas de devoluciones) y Distintos (productos distintos vendidos).</summary>
        public static DataTable ResumenProductosVendidos(DateTime desde, DateTime hasta)
        {
            return Consultar(@"
SELECT ISNULL(SUM(d.cantidad * tc.signo), 0) AS Unidades,
       COUNT(DISTINCT CASE WHEN tc.signo = 1 THEN d.id_producto END) AS Distintos
FROM        dbo.Venta_detalle    d
INNER JOIN  dbo.Venta_cabecera   vc ON vc.id_venta = d.id_venta
INNER JOIN  dbo.Tipo_comprobante tc ON tc.id_tipo_comprobante = vc.id_tipo_comprobante
WHERE vc.estado = 'EMITIDA' AND vc.fecha_hora >= @desde AND vc.fecha_hora < @hasta;",
                desde, hasta);
        }

        /// <summary>Producto y Unidades (netas de devoluciones) de los más vendidos; ante un empate, por orden alfabético.</summary>
        public static DataTable ProductosMasVendidos(DateTime desde, DateTime hasta, int cantidad)
        {
            return Consultar(@"
SELECT TOP (@cantidad) p.nombre AS Producto, SUM(d.cantidad * tc.signo) AS Unidades
FROM        dbo.Venta_detalle    d
INNER JOIN  dbo.Venta_cabecera   vc ON vc.id_venta    = d.id_venta
INNER JOIN  dbo.Tipo_comprobante tc ON tc.id_tipo_comprobante = vc.id_tipo_comprobante
INNER JOIN  dbo.Producto         p  ON p.id_producto  = d.id_producto
WHERE vc.estado = 'EMITIDA' AND vc.fecha_hora >= @desde AND vc.fecha_hora < @hasta
GROUP BY p.id_producto, p.nombre
HAVING SUM(d.cantidad * tc.signo) > 0
ORDER BY SUM(d.cantidad * tc.signo) DESC, p.nombre;",
                desde, hasta,
                new SqlParameter("@cantidad", SqlDbType.Int) { Value = cantidad });
        }

        /// <summary>Una fila: Total cobrado (neto de devoluciones) y cuánto de eso fue Efectivo.</summary>
        public static DataTable ResumenRecaudacion(DateTime desde, DateTime hasta)
        {
            return Consultar(@"
SELECT ISNULL(SUM(vp.importe * tc.signo), 0) AS Total,
       ISNULL(SUM(CASE WHEN mp.es_efectivo = 1 THEN vp.importe * tc.signo ELSE 0 END), 0) AS Efectivo"
                + PagosEnRango + ";",
                desde, hasta);
        }

        /// <summary>Medio, Pagos (cuántos cobros) e Importe neto con cada medio, del que más al que menos.</summary>
        public static DataTable RecaudacionPorMedio(DateTime desde, DateTime hasta)
        {
            return Consultar(@"
SELECT mp.nombre AS Medio,
       COUNT(CASE WHEN tc.signo = 1 THEN 1 END) AS Pagos,
       SUM(vp.importe * tc.signo) AS Importe"
                + PagosEnRango + @"
GROUP BY mp.id_medio_pago, mp.nombre
ORDER BY SUM(vp.importe * tc.signo) DESC, mp.nombre;",
                desde, hasta);
        }

        /// <summary>Fecha e Importe neto cobrado de cada día en que hubo movimientos.</summary>
        public static DataTable RecaudacionPorDia(DateTime desde, DateTime hasta)
        {
            return Consultar(@"
SELECT CAST(vc.fecha_hora AS DATE) AS Fecha, SUM(vp.importe * tc.signo) AS Importe"
                + PagosEnRango + @"
GROUP BY CAST(vc.fecha_hora AS DATE)
ORDER BY Fecha;",
                desde, hasta);
        }

        /// <summary>Anio, Mes e Importe neto cobrado de cada mes en que hubo movimientos.</summary>
        public static DataTable RecaudacionPorMes(DateTime desde, DateTime hasta)
        {
            return Consultar(@"
SELECT YEAR(vc.fecha_hora) AS Anio, MONTH(vc.fecha_hora) AS Mes, SUM(vp.importe * tc.signo) AS Importe"
                + PagosEnRango + @"
GROUP BY YEAR(vc.fecha_hora), MONTH(vc.fecha_hora)
ORDER BY Anio, Mes;",
                desde, hasta);
        }

        /// <summary>
        /// Reporte de clientes (procedimiento sp_ReporteClientes). La primera tabla trae un cliente
        /// por fila (IdCliente, Cliente, Compras, Total, Promedio, Ultima); la segunda, una sola fila
        /// con ClientesNuevos, VentasSinCliente y VentasTotales.
        /// </summary>
        public static DataSet Clientes(DateTime desde, DateTime hasta)
        {
            var ds = new DataSet();
            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand("dbo.sp_ReporteClientes", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@desde", SqlDbType.DateTime2).Value = desde;
                cmd.Parameters.Add("@hasta", SqlDbType.DateTime2).Value = hasta;

                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(ds);
            }
            return ds;
        }

        private static DataTable Consultar(string sql, DateTime desde, DateTime hasta,
                                           params SqlParameter[] otrosParametros)
        {
            var dt = new DataTable();
            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@desde", SqlDbType.DateTime2).Value = desde;
                cmd.Parameters.Add("@hasta", SqlDbType.DateTime2).Value = hasta;
                cmd.Parameters.AddRange(otrosParametros);

                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(dt);
            }
            return dt;
        }
    }
}
