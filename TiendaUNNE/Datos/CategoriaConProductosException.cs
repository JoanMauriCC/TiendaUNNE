using System;

namespace TiendaUNNE
{
    /// <summary>
    /// La base rechazó la baja de una categoría porque todavía tiene productos activos.
    /// La capa Datos la lanza con la cantidad; la capa Negocio decide qué mensaje ve el usuario.
    /// </summary>
    public class CategoriaConProductosException : Exception
    {
        public CategoriaConProductosException(int productosActivos)
            : base("La categoría tiene " + productosActivos + " producto(s) activo(s).")
        {
            ProductosActivos = productosActivos;
        }

        public int ProductosActivos { get; }
    }
}
