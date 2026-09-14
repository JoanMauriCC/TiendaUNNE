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

        // Acepta DNI suelto (7 a 11 dígitos) o CUIT con guiones: 20-12345678-9
        private static readonly Regex RegexDniCuit = new Regex(
            @"^(\d{7,11}|\d{2}-\d{7,8}-\d)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex RegexTelefono = new Regex(
            @"^[0-9\s\-\+\(\)]{6,30}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // Letras (incluye tildes y ñ) separadas por espacios, apóstrofo o guion:
        // acepta "María José", "O'Connor" o "Pérez-Gómez", rechaza "Juan2" o "-Ana".
        private static readonly Regex RegexNombrePersona = new Regex(
            @"^\p{L}+(?:(?: +|['\-])\p{L}+)*$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // Letras sin tilde, números, punto, guion y guion bajo. Sin espacios.
        private static readonly Regex RegexNombreUsuario = new Regex(
            @"^[A-Za-z0-9._\-]+$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex RegexEspaciosRepetidos = new Regex(
            @" {2,}",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static bool EsEmailValido(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return RegexEmail.IsMatch(email.Trim());
        }

        public static bool EsDniCuitValido(string dniCuit)
        {
            if (string.IsNullOrWhiteSpace(dniCuit)) return false;
            return RegexDniCuit.IsMatch(dniCuit.Trim());
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

        public static bool EsNombreUsuarioValido(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario)) return false;
            return RegexNombreUsuario.IsMatch(nombreUsuario.Trim());
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

        public static bool EsCaracterNombreUsuarioValido(char caracter)
        {
            return (caracter >= 'a' && caracter <= 'z')
                || (caracter >= 'A' && caracter <= 'Z')
                || char.IsDigit(caracter) || char.IsControl(caracter)
                || caracter == '.' || caracter == '_' || caracter == '-';
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

        /// <summary>Ídem, pero para DNI/CUIT, donde además se permite el guion.</summary>
        public static bool EsCaracterDniCuitValido(char caracter)
        {
            return char.IsDigit(caracter) || char.IsControl(caracter) || caracter == '-';
        }
    }
}
