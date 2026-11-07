using System;

namespace TiendaUNNE
{
    /// <summary>Por qué la base rechazó una nota de crédito.</summary>
    public enum CausaNotaCredito
    {
        VentaInexistente,
        NoSeAdmiteNotaDeCredito,
        CantidadExcedida
    }

    /// <summary>
    /// La base rechazó una nota de crédito. La capa Datos la lanza con el motivo y, si la cantidad
    /// pedida era demasiada, cuánto se podía devolver; Negocio decide qué mensaje ve el usuario.
    /// </summary>
    public class NotaCreditoInvalidaException : Exception
    {
        public NotaCreditoInvalidaException(CausaNotaCredito causa, int idProducto = 0, decimal disponible = 0)
            : base("Nota de crédito rechazada: " + causa + ".")
        {
            Causa = causa;
            IdProducto = idProducto;
            Disponible = disponible;
        }

        public CausaNotaCredito Causa { get; }
        public int IdProducto { get; }
        public decimal Disponible { get; }
    }
}