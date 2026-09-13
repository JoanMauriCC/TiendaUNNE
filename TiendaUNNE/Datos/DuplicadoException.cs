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
        /// SQL Server nombra la restricción entre comillas simples al principio del mensaje,
        /// tanto en inglés como en español, así que alcanza con tomar el primer entrecomillado.
        /// </summary>
        private static string NombreRestriccion(string mensaje)
        {
            int inicio = mensaje.IndexOf('\'');
            if (inicio < 0) return string.Empty;

            int fin = mensaje.IndexOf('\'', inicio + 1);
            if (fin < 0) return string.Empty;

            return mensaje.Substring(inicio + 1, fin - inicio - 1);
        }
    }
}
