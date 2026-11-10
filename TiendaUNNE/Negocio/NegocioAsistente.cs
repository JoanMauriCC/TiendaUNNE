using System;
using System.Collections.Generic;
using System.Globalization;
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

            try
            {
                try
                {
                    return await ServicioGemini.GenerarAsync(ServicioGemini.Modelo(), Instrucciones(), mensajes);
                }
                catch (ErrorAsistenteException ex) when (EsPasajero(ex.Causa))
                {
                    // El modelo principal está saturado o tardó demasiado: se reintenta una vez con el de respaldo.
                    return await ServicioGemini.GenerarAsync(ServicioGemini.ModeloRespaldo(), Instrucciones(), mensajes);
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
                "las estaciones son las del hemisferio sur. No inventes cifras de ventas del local: si no " +
                "tenés el dato, decilo. Escribí en texto plano, sin formato Markdown (nada de asteriscos ni " +
                "numerales); para listas usá guiones al principio de la línea.",
                DateTime.Today);
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
