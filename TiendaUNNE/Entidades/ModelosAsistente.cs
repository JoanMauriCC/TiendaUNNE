namespace TiendaUNNE
{
    /// <summary>Quién escribió un mensaje del chat del asistente.</summary>
    public enum RolMensaje
    {
        Usuario,
        Asistente
    }

    /// <summary>Un mensaje de la conversación con el asistente de ventas. Solo datos.</summary>
    public sealed class MensajeChat
    {
        public RolMensaje Rol { get; set; }
        public string Texto { get; set; }
    }
}
