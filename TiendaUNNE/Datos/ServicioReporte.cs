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

        private static DataTable Consultar(string sql, DateTime desde, DateTime hasta)
        {
            var dt = new DataTable();
            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@desde", SqlDbType.DateTime2).Value = desde;
                cmd.Parameters.Add("@hasta", SqlDbType.DateTime2).Value = hasta;

                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(dt);
            }
            return dt;
        }
    }
}
