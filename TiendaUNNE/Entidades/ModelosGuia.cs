using System.Collections.Generic;

namespace TiendaUNNE
{
    /// <summary>
    /// Un paso de la visita guiada: qué parte de la pantalla se señala (Ancla) y qué se dice.
    /// El ancla es una clave, no un control: Negocio no sabe de botones ni grillas, y cada
    /// pantalla traduce la clave a su control. Sin ancla (null) el paso se muestra centrado.
    /// </summary>
    public sealed class PasoGuia
    {
        public string Ancla { get; set; }
        public string Titulo { get; set; }
        public string Texto { get; set; }
    }

    /// <summary>Un capítulo de la guía: los pasos que explican una pantalla.</summary>
    public sealed class CapituloGuia
    {
        public string Nombre { get; set; }
        public List<PasoGuia> Pasos { get; } = new List<PasoGuia>();
    }

    /// <summary>
    /// Las claves de las partes que la guía puede señalar. Están acá, en Entidades, porque las
    /// usan las dos capas: Negocio las pone en los pasos y Presentación las traduce a controles.
    /// </summary>
    public static class AnclaGuia
    {
        // Ventana principal (presentes en todas las pantallas)
        public const string Menu = "Principal.Menu";
        public const string Titulo = "Principal.Titulo";
        public const string Ayuda = "Principal.Ayuda";
        public const string Sesion = "Principal.Sesion";
        public const string Contenido = "Principal.Contenido";
    }
}
