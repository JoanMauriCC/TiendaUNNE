using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace TiendaUNNE
{
    /// <summary>
    /// Acceso a las copias de seguridad: ejecuta BACKUP y RESTORE en SQL Server y lee y copia
    /// los archivos .bak. No decide si algo se puede hacer ni arma nombres: eso es de NegocioBackup.
    ///
    /// Ojo con quién escribe el archivo: lo escribe el SERVICIO de SQL Server, no esta aplicación,
    /// así que la carpeta tiene que ser accesible para ese servicio.
    /// Las rutas viajan siempre como parámetro SQL, nunca pegadas dentro del texto del comando.
    /// </summary>
    public static class ServicioBackup
    {
        private const string ClaveCarpeta = "CarpetaBackups";
        private const string CarpetaPorDefecto = @"C:\TiendaUNNE\Backups";
        private const int TimeoutSegundos = 300;

        /// <summary>Nombre de la base de datos de la aplicación, tomado de la cadena de conexión.</summary>
        public static string NombreBaseDeDatos
            => new SqlConnectionStringBuilder(Db.ConnectionString).InitialCatalog;

        /// <summary>Carpeta de copias configurada en App.config (clave CarpetaBackups).</summary>
        public static string CarpetaConfigurada()
        {
            string valor = ConfigurationManager.AppSettings[ClaveCarpeta];
            return string.IsNullOrWhiteSpace(valor) ? CarpetaPorDefecto : valor.Trim();
        }

        // ---------------------------------------------------------------------
        // Archivos
        // ---------------------------------------------------------------------

        public static void AsegurarCarpeta(string carpeta)
        {
            Directory.CreateDirectory(carpeta);
        }

        /// <summary>Los .bak de la carpeta, del más nuevo al más viejo.</summary>
        public static List<ArchivoBackup> ListarArchivos(string carpeta)
        {
            if (!Directory.Exists(carpeta))
                return new List<ArchivoBackup>();

            return new DirectoryInfo(carpeta)
                .GetFiles("*.bak")
                .OrderByDescending(f => f.LastWriteTime)
                .Select(f => new ArchivoBackup
                {
                    Nombre = f.Name,
                    FechaHora = f.LastWriteTime,
                    TamanioBytes = f.Length
                })
                .ToList();
        }

        public static bool ExisteArchivo(string ruta) => File.Exists(ruta);

        public static void CopiarArchivo(string origen, string destino) => File.Copy(origen, destino, true);

        // ---------------------------------------------------------------------
        // SQL Server
        // ---------------------------------------------------------------------

        /// <summary>
        /// Las operaciones sobre la base se hacen conectados a "master": para restaurar hay
        /// que sacar a todos de TiendaUNNE, y no se puede hacer desde adentro de ella.
        /// El pool se apaga para no dejar conexiones colgadas en esta operación.
        /// </summary>
        private static SqlConnection AbrirConexionMaster()
        {
            var cadena = new SqlConnectionStringBuilder(Db.ConnectionString)
            {
                InitialCatalog = "master",
                Pooling = false
            };

            var cn = new SqlConnection(cadena.ConnectionString);
            cn.Open();
            return cn;
        }

        /// <summary>El nombre de la base va entre corchetes; no viene de una persona, sino de App.config.</summary>
        private static string NombreEntreCorchetes()
            => "[" + NombreBaseDeDatos.Replace("]", "]]") + "]";

        private static SqlCommand Comando(SqlConnection cn, string sql, string ruta = null)
        {
            var cmd = new SqlCommand(sql, cn) { CommandTimeout = TimeoutSegundos };
            if (ruta != null)
                cmd.Parameters.Add("@ruta", SqlDbType.NVarChar, 260).Value = ruta;
            return cmd;
        }

        public static void CrearBackup(string ruta)
        {
            try
            {
                using (var cn = AbrirConexionMaster())
                using (var cmd = Comando(cn,
                    "BACKUP DATABASE " + NombreEntreCorchetes() +
                    " TO DISK = @ruta WITH INIT, CHECKSUM, NAME = N'Copia de seguridad TiendaUNNE';", ruta))
                    cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw ErrorBackupException.Traducir(ex);
            }
        }

        /// <summary>Comprueba que el archivo esté íntegro y se pueda restaurar (no restaura nada).</summary>
        public static void VerificarArchivo(string ruta)
        {
            try
            {
                using (var cn = AbrirConexionMaster())
                using (var cmd = Comando(cn, "RESTORE VERIFYONLY FROM DISK = @ruta WITH CHECKSUM;", ruta))
                    cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw ErrorBackupException.Traducir(ex);
            }
        }

        /// <summary>Nombre de la base de datos con la que se hizo ese archivo.</summary>
        public static string LeerBaseDelArchivo(string ruta)
        {
            try
            {
                using (var cn = AbrirConexionMaster())
                using (var cmd = Comando(cn, "RESTORE HEADERONLY FROM DISK = @ruta;", ruta))
                using (var dr = cmd.ExecuteReader())
                    return dr.Read() ? Convert.ToString(dr["DatabaseName"]) : null;
            }
            catch (SqlException ex)
            {
                throw ErrorBackupException.Traducir(ex);
            }
        }

        /// <summary>
        /// Reemplaza TODA la base por el contenido del archivo. Saca a todos los demás de la
        /// base mientras dura (modo de un solo usuario) y la deja de nuevo para todos al terminar,
        /// salga bien o mal.
        /// </summary>
        public static void Restaurar(string ruta)
        {
            try
            {
                using (var cn = AbrirConexionMaster())
                {
                    string db = NombreEntreCorchetes();

                    using (var cmd = Comando(cn, "ALTER DATABASE " + db + " SET SINGLE_USER WITH ROLLBACK IMMEDIATE;"))
                        cmd.ExecuteNonQuery();

                    try
                    {
                        using (var cmd = Comando(cn,
                            "RESTORE DATABASE " + db + " FROM DISK = @ruta WITH REPLACE, CHECKSUM;", ruta))
                            cmd.ExecuteNonQuery();
                    }
                    finally
                    {
                        try
                        {
                            using (var cmd = Comando(cn, "ALTER DATABASE " + db + " SET MULTI_USER;"))
                                cmd.ExecuteNonQuery();
                        }
                        catch (SqlException)
                        {
                            // Si la base quedó restaurándose no hay nada más que hacer acá:
                            // el error original (si lo hubo) es el que importa.
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw ErrorBackupException.Traducir(ex);
            }
            finally
            {
                // Las conexiones que la aplicación tenía abiertas apuntaban a la base de antes.
                SqlConnection.ClearAllPools();
            }
        }
    }
}
