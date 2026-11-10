using System;

namespace TiendaUNNE
{
    /// <summary>Por qué falló una consulta al servicio de inteligencia artificial.</summary>
    public enum CausaErrorAsistente
    {
        SinClave,
        ClaveInvalida,
        ModeloInexistente,
        LimiteSuperado,
        SinConexion,
        TiempoAgotado,
        ServicioNoDisponible,
        SinRespuesta,
        Otro
    }

    /// <summary>
    /// Error al hablar con Gemini, ya clasificado. Datos solo dice qué pasó; el mensaje que ve
    /// el usuario lo arma NegocioAsistente.
    /// </summary>
    public sealed class ErrorAsistenteException : Exception
    {
        public ErrorAsistenteException(CausaErrorAsistente causa, string detalle, Exception interna = null)
            : base(detalle, interna)
        {
            Causa = causa;
        }

        public CausaErrorAsistente Causa { get; }
    }
}
