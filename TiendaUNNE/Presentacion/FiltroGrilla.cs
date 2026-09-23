using System.Collections.Generic;
using System.Linq;

namespace TiendaUNNE
{
    /// <summary>
    /// Arma el filtro de texto de las grillas (DataView.RowFilter) sin que lo que tipea la
    /// persona pueda romperlo. Un RowFilter lo interpreta el DataView como una expresión:
    /// una comilla suelta, un "[" o un "%" tirarían una excepción al escribir en el buscador.
    /// </summary>
    public static class FiltroGrilla
    {
        /// <summary>
        /// Filtro "cualquiera de estas columnas contiene el texto", o vacío si no hay texto.
        /// </summary>
        public static string Contiene(string texto, params string[] columnas)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            string patron = Escapar(texto.Trim());
            IEnumerable<string> condiciones = columnas.Select(c => c + " LIKE '%" + patron + "%'");
            return string.Join(" OR ", condiciones);
        }

        /// <summary>
        /// En LIKE, * % [ ] son comodines: se encierran entre corchetes para que valgan como
        /// caracteres comunes. La comilla simple se duplica, como en cualquier literal de texto.
        /// </summary>
        private static string Escapar(string texto)
        {
            var salida = new System.Text.StringBuilder();
            foreach (char c in texto)
            {
                switch (c)
                {
                    case '\'': salida.Append("''"); break;
                    case '*': salida.Append("[*]"); break;
                    case '%': salida.Append("[%]"); break;
                    case '[': salida.Append("[[]"); break;
                    case ']': salida.Append("[]]"); break;
                    default: salida.Append(c); break;
                }
            }
            return salida.ToString();
        }
    }
}
