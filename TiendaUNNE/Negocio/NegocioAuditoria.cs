using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace TiendaUNNE
{
    /// <summary>
    /// Reglas de la consulta de auditoría: qué módulos se pueden filtrar, qué período se propone por
    /// defecto, cómo se interpreta el rango de fechas, qué combinaciones de filtros son válidas y cómo
    /// se resume cada movimiento para que se entienda sin leer los textos completos.
    /// </summary>
    public static class NegocioAuditoria
    {
        /// <summary>Formato corto de la fecha y hora en la lista (el detalle completo va aparte).</summary>
        public const string FormatoFechaHora = "dd/MM/yy HH:mm";

        /// <summary>Al abrir la pantalla se propone el último mes.</summary>
        public static DateTime FechaDesdePorDefecto => DateTime.Today.AddMonths(-1);
        public static DateTime FechaHastaPorDefecto => DateTime.Today;

        /// <summary>Módulos sobre los que el sistema registra movimientos, con su nombre para el usuario.</summary>
        public static List<ModuloAuditoria> Modulos()
        {
            return new List<ModuloAuditoria>
            {
                new ModuloAuditoria { Nombre = "Todos", Tabla = null },
                new ModuloAuditoria { Nombre = "Usuarios", Tabla = "Usuario" },
                new ModuloAuditoria { Nombre = "Productos", Tabla = "Producto" },
                new ModuloAuditoria { Nombre = "Categorías", Tabla = "Categoria" },
                new ModuloAuditoria { Nombre = "Clientes", Tabla = "Cliente" },
                new ModuloAuditoria { Nombre = "Caja", Tabla = "Caja_sesion" },
                new ModuloAuditoria { Nombre = "Ventas", Tabla = "Venta_cabecera" },
                new ModuloAuditoria { Nombre = "Backup", Tabla = NegocioBackup.TablaAuditoria }
            };
        }

        /// <summary>Opciones del filtro «Usuario»: «Todos» y quienes tienen movimientos registrados.</summary>
        public static List<UsuarioAuditoria> Usuarios()
        {
            var lista = new List<UsuarioAuditoria> { new UsuarioAuditoria { Nombre = "Todos", IdUsuario = null } };
            foreach (DataRow f in ServicioAuditoria.ListarUsuarios().Rows)
                lista.Add(new UsuarioAuditoria
                {
                    Nombre = Convert.ToString(f["Nombre"]),
                    IdUsuario = Convert.ToInt32(f["IdUsuario"])
                });
            return lista;
        }

        /// <summary>Opciones del filtro «Acción».</summary>
        public static List<AccionAuditoria> Acciones()
        {
            return new List<AccionAuditoria>
            {
                new AccionAuditoria { Nombre = "Todas", Valor = null },
                new AccionAuditoria { Nombre = "Alta", Valor = "ALTA" },
                new AccionAuditoria { Nombre = "Baja", Valor = "BAJA" },
                new AccionAuditoria { Nombre = "Modificación", Valor = "MODIFICACION" }
            };
        }

        /// <summary>Primer día de «los últimos N días» contando hoy: 1 = hoy, 7 = la última semana.</summary>
        public static DateTime DesdeHaceDias(int dias)
        {
            return DateTime.Today.AddDays(-(Math.Max(1, dias) - 1));
        }

        /// <summary>Nombre del módulo para una tabla; si no se conoce, se muestra la tabla tal cual.</summary>
        public static string NombreModulo(string tabla)
        {
            foreach (ModuloAuditoria m in Modulos())
                if (m.Tabla != null && string.Equals(m.Tabla, tabla, StringComparison.OrdinalIgnoreCase))
                    return m.Nombre;
            return tabla;
        }

        /// <summary>La acción guardada en la base (ALTA, BAJA, MODIFICACION) escrita para leer.</summary>
        public static string NombreAccion(string accion)
        {
            switch ((accion ?? string.Empty).ToUpperInvariant())
            {
                case "ALTA": return "Alta";
                case "BAJA": return "Baja";
                case "MODIFICACION": return "Modificación";
                default: return accion;
            }
        }

        /// <summary>El detalle de un movimiento de la lista: solo lo que cambió, o el texto completo.</summary>
        public static DetalleAuditoria Detalle(string accion, string tabla, string anterior, string nuevo)
        {
            return ResumenAuditoria.Detalle(accion, tabla, anterior, nuevo);
        }

        /// <summary>
        /// Devuelve los movimientos que cumplen los filtros, ya resumidos para la lista. El rango se
        /// interpreta por día completo: "hasta el 5" incluye todo el día 5, así que se consulta como
        /// menor al 6 a las 00:00. Cada fila trae también los textos completos (ValorAnterior y
        /// ValorNuevo) por si hace falta ver el detalle.
        /// </summary>
        public static DataTable Listar(FiltroAuditoria filtro)
        {
            if (filtro == null) filtro = new FiltroAuditoria();

            if (filtro.Desde.HasValue && filtro.Hasta.HasValue && filtro.Desde.Value.Date > filtro.Hasta.Value.Date)
                throw new ReglaNegocioException(
                    "La fecha 'desde' no puede ser posterior a la fecha 'hasta'.");

            string filtroTabla = string.IsNullOrWhiteSpace(filtro.Tabla) ? null : filtro.Tabla;

            DateTime? desdeInclusive = filtro.Desde.HasValue ? filtro.Desde.Value.Date : (DateTime?)null;
            DateTime? hastaExclusive = filtro.Hasta.HasValue ? filtro.Hasta.Value.Date.AddDays(1) : (DateTime?)null;

            DataTable crudo = ServicioAuditoria.Listar(filtroTabla, filtro.IdUsuario, filtro.Accion,
                                                       desdeInclusive, hastaExclusive);

            // Buscar texto: cada palabra escrita tiene que aparecer en algún lado del movimiento (resumen,
            // quién, módulo o los textos completos), sin importar mayúsculas ni tildes. Tiene que coincidir
            // con el COMIENZO de una palabra: «rios» encuentra «Ríos», pero no «usuarios».
            List<Regex> palabras = Validaciones.SinTildes(filtro.Texto)
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => new Regex(@"(?<![\p{L}\p{N}])" + Regex.Escape(p), RegexOptions.CultureInvariant))
                .ToList();

            var lista = new DataTable();
            lista.Columns.Add("Fecha", typeof(DateTime));
            lista.Columns.Add("Usuario", typeof(string));
            lista.Columns.Add("Accion", typeof(string));
            lista.Columns.Add("Modulo", typeof(string));
            lista.Columns.Add("Resumen", typeof(string));
            lista.Columns.Add("ValorAnterior", typeof(string));
            lista.Columns.Add("ValorNuevo", typeof(string));
            lista.Columns.Add("Tabla", typeof(string));
            lista.Columns.Add("Sensible", typeof(string));   // por qué es sensible; vacío si no lo es

            foreach (DataRow f in crudo.Rows)
            {
                string tablaFila = Convert.ToString(f["Tabla"]);
                string accion = Convert.ToString(f["Accion"]);
                string anterior = f["ValorAnterior"] == DBNull.Value ? null : Convert.ToString(f["ValorAnterior"]);
                string nuevo = f["ValorNuevo"] == DBNull.Value ? null : Convert.ToString(f["ValorNuevo"]);
                string usuario = f["Usuario"] == DBNull.Value ? "(sin usuario)" : Convert.ToString(f["Usuario"]);

                string modulo = NombreModulo(tablaFila);
                string resumen = ResumenAuditoria.De(accion, tablaFila, anterior, nuevo);

                if (palabras.Count > 0)
                {
                    string pajar = Validaciones.SinTildes(
                        usuario + " " + NombreAccion(accion) + " " + modulo + " " + resumen + " " + anterior + " " + nuevo);
                    if (!palabras.All(p => p.IsMatch(pajar))) continue;
                }

                lista.Rows.Add(
                    f["Fecha"],
                    usuario,
                    NombreAccion(accion),
                    modulo,
                    resumen,
                    anterior,
                    nuevo,
                    tablaFila,
                    ResumenAuditoria.Sensible(accion, tablaFila, anterior, nuevo));
            }

            return lista;
        }
    }
}
