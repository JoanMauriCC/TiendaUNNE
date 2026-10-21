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

    /// <summary>Formas de dibujar el gráfico de un reporte.</summary>
    public enum TipoGrafico
    {
        Barras,
        Bastones,
        Linea,
        Escalonado,
        Torta
    }

    /// <summary>
    /// Todo lo que la pantalla de Reportes muestra de un reporte: los indicadores de arriba,
    /// el gráfico y la tabla de detalle. Llega armado desde NegocioReporte.
    /// </summary>
    public sealed class ReporteVista
    {
        public List<IndicadorReporte> Indicadores { get; } = new List<IndicadorReporte>();

        public string TituloGrafico { get; set; }
        public List<string> Etiquetas { get; } = new List<string>();
        public List<double> Valores { get; } = new List<double>();

        /// <summary>Qué gráficos tienen sentido para estos datos; el primero es el que se propone.</summary>
        public List<TipoGrafico> TiposGrafico { get; } = new List<TipoGrafico>();

        /// <summary>Cada valor sumado a todos los anteriores, para el gráfico escalonado.</summary>
        public List<double> ValoresAcumulados { get; } = new List<double>();
        public string TituloAcumulado { get; set; }

        /// <summary>Qué porcentaje del total es cada valor (0 a 100), para la torta.</summary>
        public List<double> Porcentajes { get; } = new List<double>();

        public List<string> Columnas { get; } = new List<string>();
        public List<string[]> Filas { get; } = new List<string[]>();

        /// <summary>Texto de pie: qué período se está viendo o una aclaración.</summary>
        public string Descripcion { get; set; }
    }
}
