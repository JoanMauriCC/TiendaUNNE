using System;

namespace TiendaUNNE
{
    /// <summary>
    /// Un archivo de copia de seguridad (.bak) de la carpeta de copias. Solo datos:
    /// qué significa "copia manual" o "antes de restaurar" lo decide NegocioBackup.
    /// </summary>
    public sealed class ArchivoBackup
    {
        public string Nombre { get; set; }
        public DateTime FechaHora { get; set; }
        public long TamanioBytes { get; set; }
    }

    /// <summary>
    /// Las dos "llaves" que hacen falta para restaurar, como en un submarino: la contraseña
    /// de quien tiene la sesión abierta y el DNI y la contraseña de una segunda persona.
    /// Solo datos: si alcanzan para restaurar lo decide NegocioBackup.
    /// </summary>
    public sealed class LlavesRestauracion
    {
        public string PasswordPropia { get; set; }
        public string DniSegundo { get; set; }
        public string PasswordSegundo { get; set; }
    }
}
