using System;
using System.Collections.Generic;
using System.Data;

namespace TiendaUNNE
{
    /// <summary>
    /// Reglas de la consulta de auditoría: qué tablas se pueden filtrar, qué período
    /// se propone por defecto, cómo se interpreta el rango de fechas y qué combinaciones
    /// de filtros son válidas.
    /// </summary>
    public static class NegocioAuditoria
    {
        /// <summary>Opción del combo que significa "sin filtro de tabla".</summary>
        public const string TodasLasTablas = "Todas";

        /// <summary>Formato con el que se muestra la fecha y hora de cada movimiento.</summary>
        public const string FormatoFechaHora = "dd/MM/yyyy HH:mm:ss";

        /// <summary>Al abrir la pantalla se propone el último mes.</summary>
        public static DateTime FechaDesdePorDefecto => DateTime.Today.AddMonths(-1);
        public static DateTime FechaHastaPorDefecto => DateTime.Today;

        /// <summary>Tablas sobre las que el sistema registra movimientos.</summary>
        public static List<string> TablasAuditables()
        {
            return new List<string> { TodasLasTablas, "Usuario", "Categoria", "Producto" };
        }

        /// <summary>
        /// Devuelve los movimientos que cumplen los filtros. El rango se interpreta por
        /// día completo: "hasta el 5" incluye todo el día 5, así que se consulta como
        /// menor al 6 a las 00:00.
        /// </summary>
        public static DataTable Listar(string tabla, DateTime? desde, DateTime? hasta)
        {
            if (desde.HasValue && hasta.HasValue && desde.Value.Date > hasta.Value.Date)
                throw new ReglaNegocioException(
                    "La fecha 'desde' no puede ser posterior a la fecha 'hasta'.");

            string filtroTabla =
                string.IsNullOrWhiteSpace(tabla) ||
                string.Equals(tabla, TodasLasTablas, StringComparison.OrdinalIgnoreCase)
                    ? null
                    : tabla;

            DateTime? desdeInclusive = desde.HasValue ? desde.Value.Date : (DateTime?)null;
            DateTime? hastaExclusive = hasta.HasValue ? hasta.Value.Date.AddDays(1) : (DateTime?)null;

            return ServicioAuditoria.Listar(filtroTabla, desdeInclusive, hastaExclusive);
        }
    }
}
