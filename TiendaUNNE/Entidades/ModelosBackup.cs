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
}
