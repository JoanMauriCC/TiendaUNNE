using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace TiendaUNNE
{
    /// <summary>
    /// Reglas de los reportes: valida el rango de fechas, decide cómo se agrupa según su largo
    /// y arma lo que la pantalla muestra (indicadores, gráfico y detalle) con datos de las
    /// ventas guardadas. La pantalla solo dibuja el resultado.
    /// </summary>
    public static class NegocioReporte
    {
        public const string FormatoImporte = "N2";

        /// <summary>Hasta cuántos días de rango se agrupa por día; más que eso, por mes.</summary>
        public const int DiasMaximosPorDia = 31;

        /// <summary>Cuántos productos muestra el gráfico de los más vendidos.</summary>
        public const int CantidadMasVendidos = 5;

        private const int LargoMaximoEtiqueta = 16;

        public static void ValidarRango(DateTime desde, DateTime hasta)
        {
            if (desde.Date > hasta.Date)
                throw new ReglaNegocioException("La fecha \"Desde\" no puede ser posterior a \"Hasta\".");
        }

        // ---------------------------------------------------------------------
        // Ventas
        // ---------------------------------------------------------------------

        /// <summary>
        /// Ventas emitidas entre dos fechas, ambas incluidas. Un solo día se ve por hora y con
        /// cada ticket; un rango corto, por día; uno largo, por mes.
        /// </summary>
        public static ReporteVista Ventas(DateTime desde, DateTime hasta)
        {
            ValidarRango(desde, hasta);

            DateTime inicio = desde.Date;
            DateTime fin = hasta.Date.AddDays(1);   // "hasta" cuenta el día completo

            DataRow resumen = ServicioReporte.ResumenVentas(inicio, fin).Rows[0];
            int cantidad = Convert.ToInt32(resumen["Cantidad"]);
            decimal total = Convert.ToDecimal(resumen["Total"]);

            var reporte = new ReporteVista
            {
                Descripcion = string.Format("Ventas emitidas del {0:dd/MM/yyyy} al {1:dd/MM/yyyy}.{2}",
                    desde, hasta, cantidad == 0 ? " No hay ventas en este período." : string.Empty)
            };

            reporte.Indicadores.Add(new IndicadorReporte("Ventas", cantidad.ToString("N0")));
            reporte.Indicadores.Add(new IndicadorReporte("Total vendido", Importe(total)));
            reporte.Indicadores.Add(new IndicadorReporte("Ticket promedio",
                Importe(cantidad == 0 ? 0 : total / cantidad)));

            int dias = (fin - inicio).Days;
            if (dias == 1)
                ArmarVentasDeUnDia(reporte, inicio, fin);
            else if (dias <= DiasMaximosPorDia)
                ArmarVentasPorDia(reporte, inicio, fin);
            else
                ArmarVentasPorMes(reporte, inicio, fin);

            return reporte;
        }

        private static void ArmarVentasDeUnDia(ReporteVista reporte, DateTime inicio, DateTime fin)
        {
            reporte.TituloGrafico = "Ventas por hora";
            foreach (DataRow fila in ServicioReporte.VentasPorHora(inicio, fin).Rows)
            {
                reporte.Etiquetas.Add(fila["Hora"] + "h");
                reporte.Valores.Add(Convert.ToDouble(fila["Ventas"]));
            }

            reporte.Columnas.AddRange(new[] { "Venta", "Hora", "Total" });
            foreach (DataRow fila in ServicioReporte.DetalleVentas(inicio, fin).Rows)
            {
                reporte.Filas.Add(new[]
                {
                    "Nº " + Convert.ToInt64(fila["Numero"]).ToString(NegocioVenta.FormatoNumeroTicket),
                    Convert.ToDateTime(fila["Fecha"]).ToString("HH:mm"),
                    Importe(Convert.ToDecimal(fila["Total"]))
                });
            }
        }

        private static void ArmarVentasPorDia(ReporteVista reporte, DateTime inicio, DateTime fin)
        {
            reporte.TituloGrafico = "Ventas por día";
            reporte.Columnas.AddRange(new[] { "Período", "Ventas", "Total" });

            foreach (DataRow fila in ServicioReporte.VentasPorDia(inicio, fin).Rows)
            {
                DateTime dia = Convert.ToDateTime(fila["Fecha"]);
                AgregarPeriodo(reporte, dia.ToString("dd/MM"), dia.ToString("dd/MM/yyyy"), fila);
            }
        }

        private static void ArmarVentasPorMes(ReporteVista reporte, DateTime inicio, DateTime fin)
        {
            reporte.TituloGrafico = "Ventas por mes";
            reporte.Columnas.AddRange(new[] { "Período", "Ventas", "Total" });

            foreach (DataRow fila in ServicioReporte.VentasPorMes(inicio, fin).Rows)
            {
                var mes = new DateTime(Convert.ToInt32(fila["Anio"]), Convert.ToInt32(fila["Mes"]), 1);
                AgregarPeriodo(reporte, mes.ToString("MM/yy"), mes.ToString("MM/yyyy"), fila);
            }
        }

        /// <summary>Suma un período al gráfico (etiqueta corta) y a la tabla (etiqueta completa).</summary>
        private static void AgregarPeriodo(ReporteVista reporte, string etiquetaCorta,
                                           string etiquetaCompleta, DataRow fila)
        {
            int ventas = Convert.ToInt32(fila["Ventas"]);

            reporte.Etiquetas.Add(etiquetaCorta);
            reporte.Valores.Add(ventas);
            reporte.Filas.Add(new[]
            {
                etiquetaCompleta,
                ventas.ToString("N0"),
                Importe(Convert.ToDecimal(fila["Total"]))
            });
        }

        // ---------------------------------------------------------------------
        // Productos
        // ---------------------------------------------------------------------

        /// <summary>
        /// Qué se vendió entre dos fechas (unidades, productos distintos y los más vendidos) y
        /// qué productos hay que reponer. El stock es el de hoy: no depende del rango, y qué
        /// cuenta como «bajo» o «agotado» lo decide NegocioProducto, igual que en su pantalla.
        /// </summary>
        public static ReporteVista Productos(DateTime desde, DateTime hasta)
        {
            ValidarRango(desde, hasta);

            DateTime inicio = desde.Date;
            DateTime fin = hasta.Date.AddDays(1);   // "hasta" cuenta el día completo

            DataRow resumen = ServicioReporte.ResumenProductosVendidos(inicio, fin).Rows[0];

            List<DataRow> paraReponer = NegocioProducto.Listar(activos: true).AsEnumerable()
                .Where(f => Convert.ToString(f["Estado"]).Length > 0)
                .OrderBy(f => Convert.ToDecimal(f["Stock"]))
                .ThenBy(f => Convert.ToString(f["Nombre"]))
                .ToList();

            var reporte = new ReporteVista
            {
                TituloGrafico = "Más vendidos (unidades)",
                Descripcion = string.Format(
                    "Ventas del {0:dd/MM/yyyy} al {1:dd/MM/yyyy}. La tabla muestra el stock de hoy, sin importar las fechas.",
                    desde, hasta)
            };

            reporte.Indicadores.Add(new IndicadorReporte("Unidades vendidas",
                Cantidad(Convert.ToDecimal(resumen["Unidades"]))));
            reporte.Indicadores.Add(new IndicadorReporte("Productos distintos",
                Convert.ToInt32(resumen["Distintos"]).ToString("N0")));
            reporte.Indicadores.Add(new IndicadorReporte("Stock bajo o agotado",
                paraReponer.Count.ToString("N0")));

            foreach (DataRow fila in ServicioReporte.ProductosMasVendidos(inicio, fin, CantidadMasVendidos).Rows)
            {
                reporte.Etiquetas.Add(Abreviar(Convert.ToString(fila["Producto"])));
                reporte.Valores.Add(Convert.ToDouble(fila["Unidades"]));
            }

            reporte.Columnas.AddRange(new[] { "Producto", "Stock", "Estado" });
            foreach (DataRow fila in paraReponer)
            {
                reporte.Filas.Add(new[]
                {
                    Convert.ToString(fila["Nombre"]),
                    Cantidad(Convert.ToDecimal(fila["Stock"])),
                    Convert.ToString(fila["Estado"])
                });
            }

            return reporte;
        }

        /// <summary>Un nombre largo se corta con «…» para que no se pise con el de al lado en el gráfico.</summary>
        private static string Abreviar(string nombre)
        {
            return nombre.Length <= LargoMaximoEtiqueta
                ? nombre
                : nombre.Substring(0, LargoMaximoEtiqueta - 1) + "…";
        }

        private static string Cantidad(decimal valor)
        {
            return valor.ToString(NegocioVenta.FormatoCantidadTicket);
        }

        private static string Importe(decimal valor)
        {
            return "$ " + valor.ToString(FormatoImporte);
        }
    }
}
