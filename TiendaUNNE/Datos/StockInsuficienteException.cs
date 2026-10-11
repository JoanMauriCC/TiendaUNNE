using System;

namespace TiendaUNNE
{
    /// <summary>
    /// La base rechazó una reserva o una venta porque no queda stock libre del producto.
    /// La capa Datos la lanza con la cantidad que sí está disponible; la capa Negocio
    /// decide qué mensaje ve el usuario.
    /// </summary>
    public class StockInsuficienteException : Exception
    {
        public StockInsuficienteException(int idProducto, decimal disponible)
            : base("No hay stock libre suficiente del producto " + idProducto + ".")
        {
            IdProducto = idProducto;
            Disponible = disponible;
        }

        public int IdProducto { get; }

        /// <summary>Stock que queda libre para quien intentó reservar (nunca negativo).</summary>
        public decimal Disponible { get; }
    }
}
