using System;

namespace TiendaUNNE
{
    /// <summary>
    /// Error de regla de negocio con mensaje apto para mostrar al usuario final
    /// (por ejemplo, violación de un UNIQUE traducida a texto claro).
    /// </summary>
    public class ReglaNegocioException : Exception
    {
        public ReglaNegocioException(string mensaje) : base(mensaje) { }
    }
}
