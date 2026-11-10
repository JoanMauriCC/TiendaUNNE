using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace TiendaUNNE
{
    /// <summary>
    /// Reglas del asistente de ventas con inteligencia artificial: solo lo usa el administrador,
    /// qué se le puede preguntar, con qué instrucciones responde y qué mensaje ve el usuario si
    /// algo falla. La conversación con Google Gemini la hace ServicioGemini (Datos).
    /// </summary>
    public static class NegocioAsistente
    {
        public const int LargoMaximoPregunta = 1000;

        /// <summary>Cuántos productos del ranking de más vendidos se le pasan por período.</summary>
        private const int ProductosPorRanking = 10;

        /// <summary>Tope de productos del catálogo que se le pasan (los activos, ordenados por nombre).</summary>
        private const int MaximoProductosCatalogo = 150;

        private static readonly CultureInfo Argentina = CultureInfo.GetCultureInfo("es-AR");

        /// <summary>¿Está cargada la clave de Gemini en esta computadora?</summary>
        public static bool EstaConfigurado()
        {
            return ServicioGemini.Clave() != null;
        }

        public const string AyudaConfiguracion =
            "Para usar el asistente hace falta una clave de Google Gemini en la variable de entorno " +
            "GEMINI_API_KEY de Windows. Los pasos están en INSTRUCCIONES_INSTALACION.txt.";

        /// <summary>
        /// Manda la pregunta, junto con la conversación anterior, y devuelve la respuesta del asistente.
        /// La conversación no se modifica: la pantalla agrega la pregunta y la respuesta si sale bien.
        /// </summary>
        public static async Task<string> PreguntarAsync(IList<MensajeChat> conversacion, string pregunta)
        {
            NegocioSeguridad.ValidarAcceso(OpcionMenu.Asistente);

            pregunta = (pregunta ?? string.Empty).Trim();
            if (pregunta.Length == 0)
                throw new ReglaNegocioException("Escribí una pregunta.");
            if (pregunta.Length > LargoMaximoPregunta)
                throw new ReglaNegocioException("La pregunta no puede superar los " + LargoMaximoPregunta + " caracteres.");

            var mensajes = new List<MensajeChat>(conversacion ?? new List<MensajeChat>());
            mensajes.Add(new MensajeChat { Rol = RolMensaje.Usuario, Texto = pregunta });

            // Los datos del local se leen en cada pregunta (siempre al día) y fuera de la pantalla,
            // para que no se trabe mientras consulta la base.
            string datos;
            try
            {
                datos = await Task.Run(() => DatosDelLocal(DateTime.Today));
            }
            catch (Exception)
            {
                throw new ReglaNegocioException("No se pudieron leer las ventas de la base para armar la respuesta.");
            }

            string instrucciones = Instrucciones() + Environment.NewLine + Environment.NewLine + datos;

            try
            {
                try
                {
                    return await ServicioGemini.GenerarAsync(ServicioGemini.Modelo(), instrucciones, mensajes);
                }
                catch (ErrorAsistenteException ex) when (EsPasajero(ex.Causa))
                {
                    // El modelo principal está saturado o tardó demasiado: se reintenta una vez con el de respaldo.
                    return await ServicioGemini.GenerarAsync(ServicioGemini.ModeloRespaldo(), instrucciones, mensajes);
                }
            }
            catch (ErrorAsistenteException ex)
            {
                throw Traducir(ex);
            }
        }

        /// <summary>Errores que suelen pasar solos (saturación, demora): vale la pena probar con otro modelo.</summary>
        private static bool EsPasajero(CausaErrorAsistente causa)
        {
            return causa == CausaErrorAsistente.ServicioNoDisponible
                || causa == CausaErrorAsistente.TiempoAgotado
                || causa == CausaErrorAsistente.LimiteSuperado;
        }

        /// <summary>El «personaje» del asistente: le dice a Gemini quién es y cómo tiene que responder.</summary>
        private static string Instrucciones()
        {
            return string.Format(CultureInfo.GetCultureInfo("es-AR"),
                "Sos el asistente de ventas de TiendaUNNE, un comercio minorista de Corrientes, Argentina, " +
                "que vende indumentaria, calzado, accesorios, bebidas y almacén. Hablás con el administrador " +
                "del local. Respondé en español rioplatense, claro y concreto, como un experto en ventas y " +
                "marketing de comercios chicos: sugerí promociones, combos, productos para incorporar y " +
                "acciones para la temporada. Hoy es {0:dddd d 'de' MMMM 'de' yyyy}; recordá que en Argentina " +
                "las estaciones son las del hemisferio sur. Abajo tenés los DATOS DEL LOCAL sacados del sistema: " +
                "basá tus respuestas en ellos y citá los números cuando sirvan. No inventes cifras: si algo no " +
                "está en los datos, decilo. Cuando sugieras productos, distinguí los que ya están en el catálogo " +
                "de los que serían nuevos para incorporar. Escribí en texto plano, sin formato Markdown (nada de " +
                "asteriscos ni numerales); para listas usá guiones al principio de la línea.",
                DateTime.Today);
        }

        // ---------------------------------------------------------------------
        // Datos del local que se le pasan a Gemini
        // ---------------------------------------------------------------------

        /// <summary>
        /// Un resumen en texto de las ventas y del catálogo, sacado de la base: totales y más vendidos de
        /// la última semana, el último mes y el último año, ventas por categoría, ventas por mes y los
        /// productos a la venta con su stock. Son solo números agregados y nombres de productos: nunca
        /// datos de clientes ni de usuarios. Mismo criterio que Reportes: ventas emitidas y notas de
        /// crédito restando.
        /// </summary>
        public static string DatosDelLocal(DateTime hoy)
        {
            DateTime manana = hoy.Date.AddDays(1);
            var periodos = new[]
            {
                new { Nombre = "últimos 7 días", Desde = manana.AddDays(-7) },
                new { Nombre = "últimos 30 días", Desde = manana.AddDays(-30) },
                new { Nombre = "últimos 365 días", Desde = manana.AddDays(-365) }
            };

            var t = new StringBuilder();
            t.AppendLine("DATOS DEL LOCAL (sacados del sistema el " + hoy.ToString("dd/MM/yyyy", Argentina) +
                         "). Montos en pesos argentinos, netos de notas de crédito; las ventas anuladas no cuentan.");

            t.AppendLine().AppendLine("Resumen de ventas:");
            foreach (var p in periodos)
            {
                DataRow r = ServicioReporte.ResumenVentas(p.Desde, manana).Rows[0];
                t.AppendLine("- " + p.Nombre + ": " + Convert.ToInt32(r["Cantidad"]) + " ventas, " +
                             Pesos(r["Total"]));
            }

            foreach (var p in periodos)
            {
                t.AppendLine().AppendLine("Más vendidos, " + p.Nombre + " (unidades e importe):");
                DataTable ranking = ServicioReporte.VentasPorProducto(p.Desde, manana, ProductosPorRanking);
                if (ranking.Rows.Count == 0) t.AppendLine("- Sin ventas en este período.");
                foreach (DataRow r in ranking.Rows)
                    t.AppendLine("- " + r["Producto"] + " (" + r["Categoria"] + "): " +
                                 Cantidad(r["Unidades"]) + " u., " + Pesos(r["Importe"]));
            }

            foreach (var p in new[] { periodos[1], periodos[2] })
            {
                t.AppendLine().AppendLine("Ventas por categoría, " + p.Nombre + ":");
                DataTable categorias = ServicioReporte.VentasPorCategoria(p.Desde, manana);
                if (categorias.Rows.Count == 0) t.AppendLine("- Sin ventas en este período.");
                foreach (DataRow r in categorias.Rows)
                    t.AppendLine("- " + r["Categoria"] + ": " + Cantidad(r["Unidades"]) + " u., " + Pesos(r["Importe"]));
            }

            t.AppendLine().AppendLine("Ventas por mes (últimos 12 meses):");
            DateTime inicioMeses = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-11);
            DataTable meses = ServicioReporte.VentasPorMes(inicioMeses, manana);
            if (meses.Rows.Count == 0) t.AppendLine("- Sin ventas en este período.");
            foreach (DataRow r in meses.Rows)
                t.AppendLine("- " + Convert.ToInt32(r["Mes"]).ToString("00") + "/" + r["Anio"] + ": " +
                             Convert.ToInt32(r["Ventas"]) + " ventas, " + Pesos(r["Total"]));

            t.AppendLine().AppendLine("Catálogo actual (precio, disponible para vender, estado del stock):");
            DataTable catalogo = NegocioProducto.Listar(activos: true);
            int mostrados = 0;
            foreach (DataRow r in catalogo.Rows)
            {
                if (mostrados++ >= MaximoProductosCatalogo) break;
                string estado = Convert.ToString(r["Estado"]);
                t.AppendLine("- " + r["Nombre"] + " (" + r["Categoria"] + "): " + Pesos(r["PrecioVenta"]) +
                             ", disponible " + Cantidad(r["Disponible"]) +
                             (string.IsNullOrEmpty(estado) ? string.Empty : ", " + estado.ToLower(Argentina)));
            }
            if (catalogo.Rows.Count > MaximoProductosCatalogo)
                t.AppendLine("- (y " + (catalogo.Rows.Count - MaximoProductosCatalogo) + " productos más)");

            return t.ToString();
        }

        private static string Pesos(object valor)
        {
            return "$ " + Convert.ToDecimal(valor).ToString("N0", Argentina);
        }

        private static string Cantidad(object valor)
        {
            return Convert.ToDecimal(valor).ToString(NegocioVenta.FormatoCantidadTicket, Argentina);
        }

        private static ReglaNegocioException Traducir(ErrorAsistenteException ex)
        {
            switch (ex.Causa)
            {
                case CausaErrorAsistente.SinClave:
                    return new ReglaNegocioException(AyudaConfiguracion);
                case CausaErrorAsistente.ClaveInvalida:
                    return new ReglaNegocioException(
                        "Google rechazó la clave de Gemini. Revisá que esté bien copiada en GEMINI_API_KEY y que " +
                        "esté habilitada para la API de Gemini en Google AI Studio.");
                case CausaErrorAsistente.ModeloInexistente:
                    return new ReglaNegocioException(
                        "El modelo de Gemini configurado (" + ServicioGemini.Modelo() + ") no existe o ya no está " +
                        "disponible. Cambialo en App.config (GeminiModelo).");
                case CausaErrorAsistente.LimiteSuperado:
                    return new ReglaNegocioException(
                        "Se superó el límite de consultas gratuitas de Gemini. Esperá un minuto y probá de nuevo.");
                case CausaErrorAsistente.SinConexion:
                    return new ReglaNegocioException("No hay conexión a internet o no se pudo llegar a Google.");
                case CausaErrorAsistente.TiempoAgotado:
                    return new ReglaNegocioException("Gemini tardó demasiado en responder. Probá de nuevo.");
                case CausaErrorAsistente.ServicioNoDisponible:
                    return new ReglaNegocioException("El servicio de Gemini no está disponible en este momento. Probá más tarde.");
                case CausaErrorAsistente.SinRespuesta:
                    return new ReglaNegocioException("Gemini no devolvió una respuesta para esa pregunta. Probá escribirla de otra forma.");
                default:
                    return new ReglaNegocioException("No se pudo consultar al asistente: " + ex.Message);
            }
        }
    }
}
