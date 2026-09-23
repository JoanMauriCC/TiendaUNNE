using System.Text.RegularExpressions;

namespace TiendaUNNE
{
    /// <summary>
    /// Validaciones de formato reutilizables, expresadas con expresiones regulares.
    /// Viven en la capa de Negocio: los formularios preguntan, pero no deciden.
    /// Las Regex son estáticas y compiladas para no rearmarlas en cada tecla.
    /// </summary>
    public static class Validaciones
    {
        private static readonly Regex RegexEmail = new Regex(
            @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // Solo DNI (sin CUIT/guiones): es el mismo número que se usa para loguearse.
        // Tiene que ser exactamente 8 dígitos, ni más ni menos.
        private static readonly Regex RegexDni = new Regex(
            @"^\d{8}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex RegexTelefono = new Regex(
            @"^[0-9\s\-\+\(\)]{6,30}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // Letras (incluye tildes y ñ) separadas por espacios, apóstrofo o guion:
        // acepta "María José", "O'Connor" o "Pérez-Gómez", rechaza "Juan2" o "-Ana".
        private static readonly Regex RegexNombrePersona = new Regex(
            @"^\p{L}+(?:(?: +|['\-])\p{L}+)*$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex RegexEspaciosRepetidos = new Regex(
            @" {2,}",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static bool EsEmailValido(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return RegexEmail.IsMatch(email.Trim());
        }

        public static bool EsDniValido(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni)) return false;
            return RegexDni.IsMatch(dni.Trim());
        }

        public static bool EsTelefonoValido(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono)) return false;
            return RegexTelefono.IsMatch(telefono.Trim());
        }

        public static bool EsNombrePersonaValido(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return false;
            return RegexNombrePersona.IsMatch(nombre.Trim());
        }

        /// <summary>Recorta los extremos y deja un solo espacio entre palabras.</summary>
        public static string NormalizarEspacios(string texto)
        {
            if (texto == null) return null;
            return RegexEspaciosRepetidos.Replace(texto.Trim(), " ");
        }

        /// <summary>
        /// Caracteres admitidos mientras se tipea un nombre o apellido. El control final
        /// igual lo hace EsNombrePersonaValido, porque el filtro de teclado no frena lo pegado.
        /// </summary>
        public static bool EsCaracterNombrePersonaValido(char caracter)
        {
            return char.IsLetter(caracter) || char.IsControl(caracter)
                || caracter == ' ' || caracter == '\'' || caracter == '-';
        }

        /// <summary>Los mismos caracteres que acepta la Regex del teléfono.</summary>
        public static bool EsCaracterTelefonoValido(char caracter)
        {
            return char.IsDigit(caracter) || char.IsControl(caracter)
                || caracter == ' ' || caracter == '+' || caracter == '-'
                || caracter == '(' || caracter == ')';
        }

        /// <summary>Un email no lleva espacios.</summary>
        public static bool EsCaracterEmailValido(char caracter)
        {
            return char.IsControl(caracter) || !char.IsWhiteSpace(caracter);
        }

        /// <summary>
        /// Caracteres admitidos mientras se tipea un campo numérico. Los de control
        /// (Backspace, Delete) tienen que pasar o el usuario no podría corregir.
        /// Pensado para el evento KeyPress de la capa de Presentación.
        /// </summary>
        public static bool EsCaracterNumericoValido(char caracter)
        {
            return char.IsDigit(caracter) || char.IsControl(caracter);
        }

        /// <summary>Ídem, para el DNI: solo dígitos, sin guiones (no se admite CUIT).</summary>
        public static bool EsCaracterDniValido(char caracter)
        {
            return char.IsDigit(caracter) || char.IsControl(caracter);
        }
    }
}
