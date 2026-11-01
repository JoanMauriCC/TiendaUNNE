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

    /// <summary>Lo que el propio archivo .bak dice de sí mismo (encabezado leído por SQL Server).</summary>
    public sealed class EncabezadoBackup
    {
        public string BaseDeDatos { get; set; }
        public string Servidor { get; set; }
        public string UsuarioSql { get; set; }
        public int VersionMayorSql { get; set; }
        public bool ConChecksum { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
    }

    /// <summary>
    /// Todo lo que se muestra de la copia elegida en la parte de abajo de la pantalla de Backup.
    /// Los datos del encabezado pueden faltar si no se pudo leer el archivo: ver EncabezadoLeido.
    /// </summary>
    public sealed class DetalleBackup
    {
        public string Archivo { get; set; }
        public string Tipo { get; set; }
        public string Ubicacion { get; set; }
        public DateTime FechaHora { get; set; }
        public string Tamano { get; set; }

        public bool EncabezadoLeido { get; set; }
        public string BaseDeDatos { get; set; }
        public string Servidor { get; set; }
        public string UsuarioSql { get; set; }
        public string VersionSql { get; set; }
        public bool ConChecksum { get; set; }
        public TimeSpan Duracion { get; set; }
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
