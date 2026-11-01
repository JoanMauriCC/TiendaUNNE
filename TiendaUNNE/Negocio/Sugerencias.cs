using System.Collections.Generic;
using System.Linq;

namespace TiendaUNNE
{
    /// <summary>
    /// Reglas comunes de las sugerencias que se ofrecen mientras se tipea un DNI o un nombre:
    /// desde cuántos caracteres se busca y cómo se rotulan los registros dados de baja.
    /// </summary>
    public static class Sugerencias
    {
        /// <summary>Con menos caracteres que esto no se busca: casi todo coincidiría.</summary>
        public const int Minimo = 2;

        public const string RotuloBaja = "  (dado de baja)";

        /// <summary>El texto listo para buscar, o null si todavía es muy corto.</summary>
        public static string Preparar(string texto)
        {
            string t = Validaciones.NormalizarEspacios(texto);
            return string.IsNullOrEmpty(t) || t.Length < Minimo ? null : t;
        }

        /// <summary>Marca en el texto de cada sugerencia a los registros que están dados de baja.</summary>
        public static List<SugerenciaItem> Rotular(List<SugerenciaItem> lista)
        {
            foreach (SugerenciaItem s in lista.Where(x => !x.Activo))
                s.Texto += RotuloBaja;
            return lista;
        }
    }
}