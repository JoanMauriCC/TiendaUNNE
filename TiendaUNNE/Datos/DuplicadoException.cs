using System;
using System.Data.SqlClient;

namespace TiendaUNNE
{
    /// <summary>
    /// Violación de una restricción UNIQUE/PK detectada por la base de datos.
    /// La capa Datos la lanza indicando QUÉ restricción se violó; la capa Negocio
    /// decide qué mensaje ve el usuario. Así Negocio no necesita conocer SqlException.
    /// </summary>
    public class DuplicadoException : Exception
    {
        public DuplicadoException(string restriccion)
            : base("Valor duplicado (restricción: " + restriccion + ").")
        {
            Restriccion = restriccion ?? string.Empty;
        }

        /// <summary>Nombre del índice o constraint que rechazó el valor, por ej. "UQ_Categoria_nombre".</summary>
        public string Restriccion { get; }

        /// <summary>
        /// Convierte una SqlException de clave duplicada en DuplicadoException.
        /// Si el error es de otra naturaleza devuelve la excepción original sin tocarla.
        /// </summary>
        public static Exception Traducir(SqlException ex)
        {
            // 2627 = violación de PRIMARY KEY / UNIQUE constraint, 2601 = índice único.
            if (ex.Number != 2627 && ex.Number != 2601)
                return ex;

            return new DuplicadoException(NombreRestriccion(ex.Message));
        }

        /// <summary>
        /// SQL Server nombra lo que se violó entre comillas simples, tanto en inglés como en español.
        /// Según el caso el nombre del índice o de la restricción está en el primer entrecomillado
        /// (una restricción UNIQUE: «'UQ_Categoria_nombre'») o en el segundo (un índice único:
        /// «object 'dbo.Cliente' with unique index 'UQ_Cliente_cuit'»). Como no se sabe en cuál,
        /// se devuelven todos los nombres juntos y Negocio busca el que le interesa.
        /// </summary>
        private static string NombreRestriccion(string mensaje)
        {
            var nombres = new System.Collections.Generic.List<string>();

            int inicio = mensaje.IndexOf('\'');
            while (inicio >= 0)
            {
                int fin = mensaje.IndexOf('\'', inicio + 1);
                if (fin < 0) break;

                nombres.Add(mensaje.Substring(inicio + 1, fin - inicio - 1));
                inicio = mensaje.IndexOf('\'', fin + 1);
            }

            return string.Join(" ", nombres);
        }
    }
}
