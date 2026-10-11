using System;
using System.Data.SqlClient;

namespace TiendaUNNE
{
    /// <summary>Por qué falló una operación de copia de seguridad, según lo que informó SQL Server.</summary>
    public enum CausaErrorBackup
    {
        /// <summary>El servicio de SQL Server no puede leer o escribir en la carpeta de copias.</summary>
        SinAccesoACarpeta,

        /// <summary>El usuario de Windows no tiene permisos en SQL Server para esta operación.</summary>
        SinPermisosSql,

        Otra
    }

    /// <summary>
    /// Error de SQL Server al hacer o restaurar una copia, ya clasificado. Existe para que
    /// Negocio pueda explicarle algo útil a la persona sin tener que conocer los números
    /// de error de SQL Server (eso es cosa de la capa de Datos, igual que DuplicadoException).
    /// </summary>
    public class ErrorBackupException : Exception
    {
        public ErrorBackupException(CausaErrorBackup causa, string detalle, Exception interna)
            : base(detalle, interna)
        {
            Causa = causa;
        }

        public CausaErrorBackup Causa { get; }

        public static ErrorBackupException Traducir(SqlException ex)
        {
            foreach (SqlError error in ex.Errors)
            {
                // 3201: no se pudo abrir el dispositivo de backup (carpeta inexistente o sin permiso).
                if (error.Number == 3201)
                    return new ErrorBackupException(CausaErrorBackup.SinAccesoACarpeta, error.Message, ex);

                // 229/262/15247: permiso denegado para BACKUP, RESTORE o ALTER DATABASE.
                if (error.Number == 229 || error.Number == 262 || error.Number == 15247)
                    return new ErrorBackupException(CausaErrorBackup.SinPermisosSql, error.Message, ex);
            }

            return new ErrorBackupException(CausaErrorBackup.Otra, ex.Message, ex);
        }
    }
}
