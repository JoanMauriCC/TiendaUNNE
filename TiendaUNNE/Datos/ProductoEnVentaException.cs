using System;

namespace TiendaUNNE
{
    /// <summary>
    /// La base rechazó la baja de un producto porque hay una venta en curso que lo tiene en el ticket.
    /// La capa Datos la lanza con la cantidad reservada; la capa Negocio decide qué mensaje ve el usuario.
    /// </summary>
    public class ProductoEnVentaException : Exception
    {
        public ProductoEnVentaException(decimal cantidadReservada)
            : base("El producto está reservado por una venta en curso (" + cantidadReservada + ").")
        {
            CantidadReservada = cantidadReservada;
        }

        public decimal CantidadReservada { get; }
    }
}
