using System;
using System.Data;
using System.Data.SqlClient;

namespace TiendaUNNE
{
    /// <summary>
    /// Consultas de los reportes: solo SQL, sin decidir nada. El rango llega ya resuelto
    /// desde NegocioReporte como [desde, hasta): incluye "desde" y no incluye "hasta".
    /// Solo cuentan las ventas emitidas; las anuladas quedan afuera.
    /// </summary>
    public static class ServicioReporte
    {
        /// <summary>Una fila: Cantidad de ventas y Total vendido.</summary>
        public static DataTable ResumenVentas(DateTime desde, DateTime hasta)
        {
            return Consultar(@"
SELECT COUNT(*) AS Cantidad, ISNULL(SUM(total), 0) AS Total
FROM   dbo.Venta_cabecera
WHERE  estado = 'EMITIDA' AND fecha_hora >= @desde AND fecha_hora < @hasta;",
                desde, hasta);
        }

        /// <summary>Hora (0-23), Ventas y Total de cada hora en que hubo ventas.</summary>
        public static DataTable VentasPorHora(DateTime desde, DateTime hasta)
        {
            return Consultar(@"
SELECT DATEPART(HOUR, fecha_hora) AS Hora, COUNT(*) AS Ventas, SUM(total) AS Total
FROM   dbo.Venta_cabecera
WHERE  estado = 'EMITIDA' AND fecha_hora >= @desde AND fecha_hora < @hasta
GROUP BY DATEPART(HOUR, fecha_hora)
ORDER BY Hora;",
                desde, hasta);
        }

        /// <summary>Fecha, Ventas y Total de cada día en que hubo ventas.</summary>
        public static DataTable VentasPorDia(DateTime desde, DateTime hasta)
        {
            return Consultar(@"
SELECT CAST(fecha_hora AS DATE) AS Fecha, COUNT(*) AS Ventas, SUM(total) AS Total
FROM   dbo.Venta_cabecera
WHERE  estado = 'EMITIDA' AND fecha_hora >= @desde AND fecha_hora < @hasta
GROUP BY CAST(fecha_hora AS DATE)
ORDER BY Fecha;",
                desde, hasta);
        }

        /// <summary>Anio, Mes, Ventas y Total de cada mes en que hubo ventas.</summary>
        public static DataTable VentasPorMes(DateTime desde, DateTime hasta)
        {
            return Consultar(@"
SELECT YEAR(fecha_hora) AS Anio, MONTH(fecha_hora) AS Mes, COUNT(*) AS Ventas, SUM(total) AS Total
FROM   dbo.Venta_cabecera
WHERE  estado = 'EMITIDA' AND fecha_hora >= @desde AND fecha_hora < @hasta
GROUP BY YEAR(fecha_hora), MONTH(fecha_hora)
ORDER BY Anio, Mes;",
                desde, hasta);
        }

        /// <summary>Numero, Fecha y Total de cada venta, de la más nueva a la más vieja.</summary>
        public static DataTable DetalleVentas(DateTime desde, DateTime hasta)
        {
            return Consultar(@"
SELECT numero_comprobante AS Numero, fecha_hora AS Fecha, total AS Total
FROM   dbo.Venta_cabecera
WHERE  estado = 'EMITIDA' AND fecha_hora >= @desde AND fecha_hora < @hasta
ORDER BY fecha_hora DESC;",
                desde, hasta);
        }

        /// <summary>Una fila: Unidades vendidas y Distintos (cuántos productos distintos se vendieron).</summary>
        public static DataTable ResumenProductosVendidos(DateTime desde, DateTime hasta)
        {
            return Consultar(@"
SELECT ISNULL(SUM(d.cantidad), 0) AS Unidades, COUNT(DISTINCT d.id_producto) AS Distintos
FROM        dbo.Venta_detalle  d
INNER JOIN  dbo.Venta_cabecera c ON c.id_venta = d.id_venta
WHERE c.estado = 'EMITIDA' AND c.fecha_hora >= @desde AND c.fecha_hora < @hasta;",
                desde, hasta);
        }

        /// <summary>Producto y Unidades de los más vendidos; ante un empate, por orden alfabético.</summary>
        public static DataTable ProductosMasVendidos(DateTime desde, DateTime hasta, int cantidad)
        {
            return Consultar(@"
SELECT TOP (@cantidad) p.nombre AS Producto, SUM(d.cantidad) AS Unidades
FROM        dbo.Venta_detalle  d
INNER JOIN  dbo.Venta_cabecera c ON c.id_venta    = d.id_venta
INNER JOIN  dbo.Producto       p ON p.id_producto = d.id_producto
WHERE c.estado = 'EMITIDA' AND c.fecha_hora >= @desde AND c.fecha_hora < @hasta
GROUP BY p.id_producto, p.nombre
ORDER BY SUM(d.cantidad) DESC, p.nombre;",
                desde, hasta,
                new SqlParameter("@cantidad", SqlDbType.Int) { Value = cantidad });
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
