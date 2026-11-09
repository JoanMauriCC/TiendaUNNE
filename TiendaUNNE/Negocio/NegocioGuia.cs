namespace TiendaUNNE
{
    /// <summary>
    /// Contenido y reglas de la ayuda (visita guiada): qué capítulo explica cada pantalla y
    /// cuándo se muestra la Bienvenida sola. No sabe de controles: cada paso señala una parte
    /// de la pantalla con una clave de AnclaGuia, y la pantalla la traduce a su control.
    /// </summary>
    public static class NegocioGuia
    {
        /// <summary>
        /// El capítulo que se recorre al pedir ayuda en una pantalla. Si la pantalla todavía no
        /// tiene capítulo propio, se ofrece la Bienvenida, que explica la ventana en general.
        /// </summary>
        public static CapituloGuia CapituloDe(OpcionMenu pantalla)
        {
            switch (pantalla)
            {
                default:
                    return Bienvenida();
            }
        }

        /// <summary>La Bienvenida se muestra sola la primera vez que el usuario entra al sistema.</summary>
        public static bool DebeMostrarBienvenida(UsuarioLogueado usuario)
        {
            return usuario != null && usuario.PrimerIngreso;
        }

        public static CapituloGuia Bienvenida()
        {
            var c = new CapituloGuia { Nombre = "Bienvenida" };

            Paso(c, null, "Bienvenido a TiendaUNNE",
                "Esta guía te muestra en un minuto cómo está organizado el sistema. " +
                "Podés volver a verla cuando quieras con el botón ? de arriba o con la tecla F1.");

            Paso(c, AnclaGuia.Menu, "El menú",
                "Desde acá llegás a cada módulo. Solo ves los que tu rol puede usar: " +
                "si falta alguno, es porque tu usuario no tiene permiso para esa parte.");

            Paso(c, AnclaGuia.Contenido, "Tu espacio de trabajo",
                "Acá se abre el módulo que elijas. En Inicio tenés accesos rápidos: " +
                "te movés entre ellos con las flechas y los abrís con Enter.");

            Paso(c, AnclaGuia.Titulo, "Dónde estás",
                "El título dice en qué módulo estás. En los formularios, Alt más la letra " +
                "subrayada de cada campo te lleva directo a ese campo.");

            Paso(c, AnclaGuia.Ayuda, "La ayuda",
                "En cada pantalla, este botón (o F1) recorre esa pantalla paso a paso.");

            Paso(c, AnclaGuia.Sesion, "Tu sesión",
                "Acá ves con qué usuario y rol entraste. Cerrá la sesión al terminar, " +
                "sobre todo si la computadora es compartida.");

            return c;
        }

        private static void Paso(CapituloGuia capitulo, string ancla, string titulo, string texto)
        {
            capitulo.Pasos.Add(new PasoGuia { Ancla = ancla, Titulo = titulo, Texto = texto });
        }
    }
}
