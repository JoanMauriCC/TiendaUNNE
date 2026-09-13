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
