using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace TiendaUNNE
{
    /// <summary>
    /// Reglas de las copias de seguridad. Una copia contiene TODA la base (datos personales y
    /// contraseñas cifradas), así que acá se concentran las defensas:
    ///   - solo el Administrador, verificado en cada operación (no solo al mostrar el menú);
    ///   - los nombres de archivo los genera el sistema y la pantalla solo puede señalar uno de
    ///     la carpeta de copias, nunca escribir una ruta: se valida el nombre con una expresión
    ///     regular y así no hay forma de salir de la carpeta ni de pasar rutas de red;
    ///   - restaurar exige haber escrito la palabra de confirmación y hace antes una copia de
    ///     seguridad automática del estado actual, por si fue un error.
    /// </summary>
    public static class NegocioBackup
    {
        public const string PalabraConfirmacion = "RESTAURAR";
        public const string TipoManual = "Manual";
        public const string TipoPrevio = "Antes de restaurar";

        private const string Prefijo = "TiendaUNNE_";
        private const string SufijoPrevio = "_previo";

        // TiendaUNNE_2026-10-11_1530.bak  o  TiendaUNNE_2026-10-11_153042_previo.bak
        private static readonly Regex PatronNombre = new Regex(
            @"^TiendaUNNE_\d{4}-\d{2}-\d{2}_\d{4}(\d{2})?(_previo)?\.bak$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // ---------------------------------------------------------------------
        // Acceso y carpeta
        // ---------------------------------------------------------------------

        private static void ExigirAdministrador() => NegocioSeguridad.ValidarAcceso(OpcionMenu.Backup);

        /// <summary>Carpeta de copias. Tiene que ser un disco local: las rutas de red se rechazan.</summary>
        public static string ObtenerCarpeta()
        {
            string carpeta = ServicioBackup.CarpetaConfigurada();
            ValidarRutaLocal(carpeta, "La carpeta de copias");
            return Path.GetFullPath(carpeta);
        }

        private static void ValidarRutaLocal(string ruta, string etiqueta)
        {
            // Una letra de unidad y una barra ("C:\..."). Las rutas de red (\\servidor\...)
            // harían que SQL Server intente autenticarse contra otra máquina.
            if (string.IsNullOrWhiteSpace(ruta) || !Regex.IsMatch(ruta.Trim(), @"^[A-Za-z]:\\"))
                throw new ReglaNegocioException(
                    etiqueta + " tiene que estar en un disco de esta computadora (por ejemplo C:\\...).");
        }

        // ---------------------------------------------------------------------
        // Listado
        // ---------------------------------------------------------------------

        /// <summary>Copias de la carpeta, de la más nueva a la más vieja, listas para la grilla.</summary>
        public static DataTable Listar()
        {
            ExigirAdministrador();

            var dt = new DataTable();
            dt.Columns.Add("Archivo", typeof(string));
            dt.Columns.Add("FechaHora", typeof(DateTime));
            dt.Columns.Add("Tamano", typeof(string));
            dt.Columns.Add("Tipo", typeof(string));

            foreach (ArchivoBackup a in ServicioBackup.ListarArchivos(ObtenerCarpeta()))
            {
                if (!PatronNombre.IsMatch(a.Nombre))
                    continue;   // un .bak ajeno que alguien dejó en la carpeta: no es una copia nuestra

                dt.Rows.Add(a.Nombre, a.FechaHora, FormatoTamanio(a.TamanioBytes),
                    EsPrevio(a.Nombre) ? TipoPrevio : TipoManual);
            }

            return dt;
        }

        private static bool EsPrevio(string nombre)
            => nombre.EndsWith(SufijoPrevio + ".bak", StringComparison.OrdinalIgnoreCase);

        public static string FormatoTamanio(long bytes)
        {
            double mb = bytes / 1048576.0;
            return mb >= 1
                ? mb.ToString("N1", CultureInfo.CurrentCulture) + " MB"
                : Math.Max(1, bytes / 1024).ToString(CultureInfo.CurrentCulture) + " KB";
        }

        /// <summary>"11/10/2026 15:30 · hace 2 h" para la última copia, o aviso si todavía no hay ninguna.</summary>
        public static string TextoUltimaCopia(DataTable copias)
        {
            if (copias == null || copias.Rows.Count == 0)
                return "Todavía no hay copias";

            DateTime fecha = (DateTime)copias.Rows[0]["FechaHora"];
            return fecha.ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture) + " · " + DescribirAntiguedad(fecha);
        }

        private static string DescribirAntiguedad(DateTime fecha)
        {
            TimeSpan t = DateTime.Now - fecha;
            if (t.TotalMinutes < 1) return "hace instantes";
            if (t.TotalMinutes < 60) return "hace " + (int)t.TotalMinutes + " min";
            if (t.TotalHours < 24) return "hace " + (int)t.TotalHours + " h";
            return "hace " + (int)t.TotalDays + " días";
        }

        // ---------------------------------------------------------------------
        // Crear
        // ---------------------------------------------------------------------

        /// <summary>Hace una copia del estado actual de la base y devuelve el nombre del archivo.</summary>
        public static string Crear()
        {
            ExigirAdministrador();
            return CrearArchivo("yyyy-MM-dd_HHmm", string.Empty);
        }

        private static string CrearArchivo(string formatoFecha, string sufijo)
        {
            string carpeta = ObtenerCarpeta();
            string nombre = Prefijo + DateTime.Now.ToString(formatoFecha, CultureInfo.InvariantCulture) + sufijo + ".bak";
            string ruta = Path.Combine(carpeta, nombre);

            try
            {
                ServicioBackup.AsegurarCarpeta(carpeta);

                // Sin esto, WITH INIT pisaría en silencio la copia hecha un instante antes.
                if (ServicioBackup.ExisteArchivo(ruta))
                    throw new ReglaNegocioException("Ya se hizo una copia en este mismo minuto. Esperá un momento y probá de nuevo.");

                ServicioBackup.CrearBackup(ruta);
                ServicioBackup.VerificarArchivo(ruta);
            }
            catch (ErrorBackupException ex)
            {
                throw Traducir(ex, carpeta);
            }
            catch (IOException)
            {
                throw new ReglaNegocioException("No se pudo acceder a la carpeta de copias: " + carpeta);
            }
            catch (UnauthorizedAccessException)
            {
                throw new ReglaNegocioException("No hay permiso para usar la carpeta de copias: " + carpeta);
            }

            return nombre;
        }

        // ---------------------------------------------------------------------
        // Restaurar
        // ---------------------------------------------------------------------

        public static bool ConfirmacionValida(string texto)
            => string.Equals((texto ?? string.Empty).Trim(), PalabraConfirmacion, StringComparison.Ordinal);

        /// <summary>
        /// Reemplaza TODA la base por la copia indicada. Antes comprueba que el archivo sea de
        /// esta base y esté íntegro, y guarda una copia automática del estado actual.
        /// Después de esto la aplicación tiene que reiniciarse.
        /// </summary>
        public static void Restaurar(string nombreArchivo, string confirmacion)
        {
            ExigirAdministrador();

            if (!ConfirmacionValida(confirmacion))
                throw new ReglaNegocioException("Para restaurar tenés que escribir " + PalabraConfirmacion + ".");

            string ruta = RutaDeCopia(nombreArchivo);
            string carpeta = ObtenerCarpeta();

            try
            {
                string baseDelArchivo = ServicioBackup.LeerBaseDelArchivo(ruta);
                if (!string.Equals(baseDelArchivo, ServicioBackup.NombreBaseDeDatos, StringComparison.OrdinalIgnoreCase))
                    throw new ReglaNegocioException("Esa copia no corresponde a la base " + ServicioBackup.NombreBaseDeDatos + ".");

                ServicioBackup.VerificarArchivo(ruta);

                // Red de seguridad: si fue un error, esta copia permite volver atrás.
                CrearArchivo("yyyy-MM-dd_HHmmss", SufijoPrevio);

                ServicioBackup.Restaurar(ruta);
            }
            catch (ErrorBackupException ex)
            {
                throw Traducir(ex, carpeta);
            }
        }

        // ---------------------------------------------------------------------
        // Guardar una copia en otro lugar (pendrive, otra carpeta)
        // ---------------------------------------------------------------------

        public static void CopiarA(string nombreArchivo, string destino)
        {
            ExigirAdministrador();

            string origen = RutaDeCopia(nombreArchivo);

            ValidarRutaLocal(destino, "El destino");
            if (!string.Equals(Path.GetExtension(destino), ".bak", StringComparison.OrdinalIgnoreCase))
                throw new ReglaNegocioException("El archivo de destino tiene que terminar en .bak.");

            try
            {
                ServicioBackup.CopiarArchivo(origen, Path.GetFullPath(destino));
            }
            catch (IOException)
            {
                throw new ReglaNegocioException("No se pudo guardar la copia en ese lugar. Revisá que haya espacio y que no esté en uso.");
            }
            catch (UnauthorizedAccessException)
            {
                throw new ReglaNegocioException("No hay permiso para guardar en ese lugar.");
            }
        }

        // ---------------------------------------------------------------------
        // Auxiliares
        // ---------------------------------------------------------------------

        /// <summary>
        /// De un nombre a la ruta completa, solo si es una copia nuestra que existe en la carpeta.
        /// La pantalla pasa nombres, nunca rutas: cualquier cosa que no encaje con el patrón se rechaza.
        /// </summary>
        private static string RutaDeCopia(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre) || !PatronNombre.IsMatch(nombre))
                throw new ReglaNegocioException("Elegí una copia de la lista.");

            string ruta = Path.Combine(ObtenerCarpeta(), nombre);
            if (!ServicioBackup.ExisteArchivo(ruta))
                throw new ReglaNegocioException("Esa copia ya no está en la carpeta. Actualizá el listado.");

            return ruta;
        }

        private static ReglaNegocioException Traducir(ErrorBackupException ex, string carpeta)
        {
            switch (ex.Causa)
            {
                case CausaErrorBackup.SinAccesoACarpeta:
                    return new ReglaNegocioException(
                        "SQL Server no pudo usar la carpeta de copias (" + carpeta + "). " +
                        "Hay que darle permiso de escritura al servicio de SQL Server en esa carpeta.");

                case CausaErrorBackup.SinPermisosSql:
                    return new ReglaNegocioException(
                        "Tu usuario de Windows no tiene permisos en SQL Server para hacer o restaurar copias.");

                default:
                    return new ReglaNegocioException("No se pudo completar la operación: " + ex.Message);
            }
        }
    }
}
