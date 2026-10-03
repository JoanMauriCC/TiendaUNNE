using System.Collections.Generic;

namespace TiendaUNNE
{
    /// <summary>Un número destacado del reporte: título y valor ya formateado.</summary>
    public sealed class IndicadorReporte
    {
        public IndicadorReporte(string titulo, string valor)
        {
            Titulo = titulo;
            Valor = valor;
        }

        public string Titulo { get; }
        public string Valor { get; }
    }

    /// <summary>
    /// Todo lo que la pantalla de Reportes muestra de un reporte: los indicadores de arriba,
    /// el gráfico de barras y la tabla de detalle. Llega armado desde NegocioReporte.
    /// </summary>
    public sealed class ReporteVista
    {
        public List<IndicadorReporte> Indicadores { get; } = new List<IndicadorReporte>();

        public string TituloGrafico { get; set; }
        public List<string> Etiquetas { get; } = new List<string>();
        public List<double> Valores { get; } = new List<double>();

        public List<string> Columnas { get; } = new List<string>();
        public List<string[]> Filas { get; } = new List<string[]>();

        /// <summary>Texto de pie: qué período se está viendo o una aclaración.</summary>
        public string Descripcion { get; set; }
    }
}
