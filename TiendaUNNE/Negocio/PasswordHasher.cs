using System;
using System.Security.Cryptography;

namespace TiendaUNNE
{
    /// <summary>
    /// Hash de contraseñas con PBKDF2 (Rfc2898DeriveBytes + SHA-256).
    /// El hash se guarda en Usuario.hash_password y el salt aleatorio en Usuario.salt
    /// (ambas columnas VARBINARY). No se guarda nunca la contraseña en claro.
    /// Requiere .NET Framework 4.7.2+ (o .NET 6+).
    /// </summary>
    public static class PasswordHasher
    {
        private const int TamanioSalt = 16;      // 128 bits
        private const int TamanioHash = 32;      // 256 bits
        private const int Iteraciones = 150_000;

        /// <summary>Genera un hash + salt nuevos para una contraseña.</summary>
        public static void Generar(string password, out byte[] hash, out byte[] salt)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("La contraseña no puede estar vacía.", nameof(password));

            salt = new byte[TamanioSalt];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);

            hash = DerivarClave(password, salt);
        }

        /// <summary>Verifica una contraseña contra el hash y salt almacenados.</summary>
        public static bool Verificar(string password, byte[] hashAlmacenado, byte[] salt)
        {
            if (string.IsNullOrEmpty(password) || hashAlmacenado == null || salt == null)
                return false;

            byte[] hashCalculado = DerivarClave(password, salt);
            return ComparacionTiempoFijo(hashCalculado, hashAlmacenado);
        }

        private static byte[] DerivarClave(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iteraciones, HashAlgorithmName.SHA256))
                return pbkdf2.GetBytes(TamanioHash);
        }

        // Comparación de tiempo constante para no filtrar información por timing.
        private static bool ComparacionTiempoFijo(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int diferencia = 0;
            for (int i = 0; i < a.Length; i++)
                diferencia |= a[i] ^ b[i];
            return diferencia == 0;
        }
    }
}
