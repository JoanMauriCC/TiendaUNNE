using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace TiendaUNNE
{
    /// <summary>
    /// Acceso al servicio de inteligencia artificial Google Gemini (API REST generateContent).
    /// Es acceso a datos igual que la base, solo que externo: arma el pedido HTTP, lo manda y
    /// devuelve el texto de la respuesta. Qué se le pregunta y con qué instrucciones lo decide
    /// NegocioAsistente.
    ///
    /// La clave NO está en el código ni en App.config (que está en GitHub): se lee de la variable
    /// de entorno GEMINI_API_KEY del usuario de Windows. El modelo sí se configura en App.config.
    /// </summary>
    public static class ServicioGemini
    {
        private const string VariableClave = "GEMINI_API_KEY";
        private const string ClaveModelo = "GeminiModelo";
        private const string ClaveModeloRespaldo = "GeminiModeloRespaldo";
        private const string ClaveBuscarEnGoogle = "GeminiBuscarEnGoogle";
        private const string ModeloPorDefecto = "gemini-3.6-flash";
        private const string ModeloRespaldoPorDefecto = "gemini-3.5-flash-lite";
        private const string UrlBase = "https://generativelanguage.googleapis.com/v1beta/models/";

        // Un solo HttpClient para toda la aplicación, como recomienda .NET. El tiempo es por intento:
        // si se agota, todavía queda el modelo de respaldo.
        private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(40) };

        /// <summary>
        /// La clave de la variable de entorno. Se busca primero en el proceso y, si Visual Studio
        /// se abrió antes de crearla, directamente en las variables del usuario de Windows.
        /// </summary>
        public static string Clave()
        {
            string clave = Environment.GetEnvironmentVariable(VariableClave);
            if (string.IsNullOrWhiteSpace(clave))
                clave = Environment.GetEnvironmentVariable(VariableClave, EnvironmentVariableTarget.User);
            return string.IsNullOrWhiteSpace(clave) ? null : clave.Trim();
        }

        public static string Modelo()
        {
            return Configurado(ClaveModelo, ModeloPorDefecto);
        }

        /// <summary>El modelo a usar si el principal está saturado (Google a veces responde «high demand»).</summary>
        public static string ModeloRespaldo()
        {
            return Configurado(ClaveModeloRespaldo, ModeloRespaldoPorDefecto);
        }

        private static string Configurado(string clave, string porDefecto)
        {
            string valor = ConfigurationManager.AppSettings[clave];
            return string.IsNullOrWhiteSpace(valor) ? porDefecto : valor.Trim();
        }

        /// <summary>La búsqueda en Google solo funciona con facturación activada: viene apagada.</summary>
        public static bool BuscarEnGoogle()
        {
            return string.Equals(ConfigurationManager.AppSettings[ClaveBuscarEnGoogle], "true",
                                 StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Manda la conversación con sus instrucciones y devuelve el texto de la respuesta.
        /// Lanza ErrorAsistenteException con la causa ya clasificada si algo sale mal.
        /// </summary>
        public static async Task<string> GenerarAsync(string modelo, string instrucciones, IList<MensajeChat> conversacion)
        {
            string clave = Clave();
            if (clave == null)
                throw new ErrorAsistenteException(CausaErrorAsistente.SinClave, "No está la variable " + VariableClave + ".");

            string json = new JavaScriptSerializer().Serialize(ArmarPedido(instrucciones, conversacion));

            using (var pedido = new HttpRequestMessage(HttpMethod.Post, UrlBase + modelo + ":generateContent"))
            {
                pedido.Headers.Add("x-goog-api-key", clave);
                pedido.Content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage respuesta;
                string cuerpo;
                try
                {
                    respuesta = await Http.SendAsync(pedido).ConfigureAwait(false);
                    cuerpo = await respuesta.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                catch (TaskCanceledException ex)
                {
                    throw new ErrorAsistenteException(CausaErrorAsistente.TiempoAgotado, "No respondió a tiempo.", ex);
                }
                catch (HttpRequestException ex)
                {
                    throw new ErrorAsistenteException(CausaErrorAsistente.SinConexion, ex.Message, ex);
                }

                using (respuesta)
                {
                    if (!respuesta.IsSuccessStatusCode)
                        throw Clasificar(respuesta.StatusCode, cuerpo);

                    return LeerTexto(cuerpo);
                }
            }
        }

        // ---------------------------------------------------------------------
        // JSON
        // ---------------------------------------------------------------------

        private static Dictionary<string, object> ArmarPedido(string instrucciones, IList<MensajeChat> conversacion)
        {
            var contenidos = new List<object>();
            foreach (MensajeChat m in conversacion)
            {
                contenidos.Add(new Dictionary<string, object>
                {
                    { "role", m.Rol == RolMensaje.Usuario ? "user" : "model" },
                    { "parts", new object[] { new Dictionary<string, object> { { "text", m.Texto } } } }
                });
            }

            var pedido = new Dictionary<string, object>
            {
                { "systemInstruction", new Dictionary<string, object>
                    {
                        { "parts", new object[] { new Dictionary<string, object> { { "text", instrucciones } } } }
                    }
                },
                { "contents", contenidos }
            };

            if (BuscarEnGoogle())
                pedido["tools"] = new object[] { new Dictionary<string, object> { { "google_search", new Dictionary<string, object>() } } };

            return pedido;
        }

        /// <summary>El texto está en candidates[0].content.parts[*].text.</summary>
        private static string LeerTexto(string cuerpo)
        {
            var raiz = new JavaScriptSerializer().DeserializeObject(cuerpo) as Dictionary<string, object>;

            var candidatos = Valor(raiz, "candidates") as IList;
            var primero = candidatos != null && candidatos.Count > 0 ? candidatos[0] as Dictionary<string, object> : null;
            var contenido = Valor(primero, "content") as Dictionary<string, object>;
            var partes = Valor(contenido, "parts") as IList;

            var texto = new StringBuilder();
            if (partes != null)
                foreach (object parte in partes)
                {
                    object t = Valor(parte as Dictionary<string, object>, "text");
                    if (t != null) texto.Append(t);
                }

            if (texto.Length == 0)
                throw new ErrorAsistenteException(CausaErrorAsistente.SinRespuesta,
                    "La respuesta vino vacía (motivo: " + (Valor(primero, "finishReason") ?? "desconocido") + ").");

            return texto.ToString().Trim();
        }

        private static ErrorAsistenteException Clasificar(HttpStatusCode estado, string cuerpo)
        {
            string detalle = MensajeDeError(cuerpo) ?? ("HTTP " + (int)estado);
            bool hablaDeClave = detalle.IndexOf("API key", StringComparison.OrdinalIgnoreCase) >= 0;

            switch ((int)estado)
            {
                case 400:
                    return new ErrorAsistenteException(hablaDeClave ? CausaErrorAsistente.ClaveInvalida : CausaErrorAsistente.Otro, detalle);
                case 401:
                case 403:
                    return new ErrorAsistenteException(CausaErrorAsistente.ClaveInvalida, detalle);
                case 404:
                    return new ErrorAsistenteException(CausaErrorAsistente.ModeloInexistente, detalle);
                case 429:
                    return new ErrorAsistenteException(CausaErrorAsistente.LimiteSuperado, detalle);
                default:
                    return new ErrorAsistenteException((int)estado >= 500 ? CausaErrorAsistente.ServicioNoDisponible : CausaErrorAsistente.Otro, detalle);
            }
        }

        /// <summary>Google devuelve los errores como {"error": {"message": "..."}}.</summary>
        private static string MensajeDeError(string cuerpo)
        {
            try
            {
                var raiz = new JavaScriptSerializer().DeserializeObject(cuerpo) as Dictionary<string, object>;
                return Valor(Valor(raiz, "error") as Dictionary<string, object>, "message") as string;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static object Valor(Dictionary<string, object> objeto, string clave)
        {
            object v;
            return objeto != null && objeto.TryGetValue(clave, out v) ? v : null;
        }
    }
}
