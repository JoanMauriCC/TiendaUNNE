namespace TiendaUNNE
{
    /// <summary>Quién escribió un mensaje del chat del asistente.</summary>
    public enum RolMensaje
    {
        Usuario,
        Asistente
    }

    /// <summary>Una pregunta lista para tocar: el título corto del botón y la pregunta que manda.</summary>
    public sealed class PreguntaSugerida
    {
        public string Titulo { get; set; }
        public string Pregunta { get; set; }
    }

    /// <summary>Un mensaje de la conversación con el asistente de ventas. Solo datos.</summary>
    public sealed class MensajeChat
    {
        public RolMensaje Rol { get; set; }
        public string Texto { get; set; }
    }
}
