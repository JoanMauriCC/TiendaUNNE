using System;
using System.Collections.Generic;
using System.Data;

namespace TiendaUNNE
{
    /// <summary>
    /// Reglas de negocio de Categoria: valida, normaliza, arma los textos de auditoría
    /// y decide si corresponde un alta o una edición. La capa de Presentación llama acá,
    /// nunca directo a ServicioCategoria.
    /// </summary>
    public static class NegocioCategoria
    {
        public const int LargoMaximoNombre = 100;
        public const int LargoMaximoDescripcion = 200;

        // ---------------------------------------------------------------------
        // Consultas
        // ---------------------------------------------------------------------

        /// <summary>Categorías activas o dadas de baja, según <paramref name="activas"/>.</summary>
        public static DataTable Listar(bool activas) => ServicioCategoria.Listar(activas);

        public static List<CategoriaItem> ListarActivas() => ServicioCategoria.ListarActivas();

        public static CategoriaEditModel ObtenerParaEdicion(int idCategoria)
        {
            var categoria = ServicioCategoria.Obtener(idCategoria);
            if (categoria == null)
                throw new ReglaNegocioException("La categoría ya no existe.");

            return categoria;
        }

        // ---------------------------------------------------------------------
        // Alta y edición
        // ---------------------------------------------------------------------

        /// <summary>
        /// Valida, normaliza y persiste. Decide sola si es alta o edición según el Id,
        /// así el formulario no tiene que saberlo.
        /// </summary>
        public static void Guardar(CategoriaEditModel m, int idUsuarioSesion)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));

            Validar(m);
            Normalizar(m);

            try
            {
                if (m.EsAlta)
                {
                    ServicioCategoria.Crear(m, idUsuarioSesion, Resumen(m));
                }
                else
                {
                    var anterior = ServicioCategoria.Obtener(m.IdCategoria);
                    if (anterior == null)
                        throw new ReglaNegocioException("La categoría ya no existe.");

                    ServicioCategoria.Actualizar(m, idUsuarioSesion, Resumen(anterior), Resumen(m));
                }
            }
            catch (DuplicadoException ex)
            {
                throw TraducirDuplicado(ex);
            }
        }

        // ---------------------------------------------------------------------
        // Baja lógica
        // ---------------------------------------------------------------------

        public static void DarDeBaja(int idCategoria, int idUsuarioSesion)
        {
            var anterior = ServicioCategoria.Obtener(idCategoria);
            if (anterior == null)
                throw new ReglaNegocioException("La categoría ya no existe.");

            int filas = ServicioCategoria.DarDeBaja(idCategoria, idUsuarioSesion, Resumen(anterior));

            if (filas == 0)
                throw new ReglaNegocioException("La categoría ya estaba dada de baja o no existe.");
        }

        // ---------------------------------------------------------------------
        // Reactivación
        // ---------------------------------------------------------------------

        public static void DarDeAlta(int idCategoria, int idUsuarioSesion)
        {
            var anterior = ServicioCategoria.Obtener(idCategoria);
            if (anterior == null)
                throw new ReglaNegocioException("La categoría ya no existe.");

            int filas = ServicioCategoria.DarDeAlta(idCategoria, idUsuarioSesion, Resumen(anterior));

            if (filas == 0)
                throw new ReglaNegocioException("La categoría ya estaba activa.");
        }

        // ---------------------------------------------------------------------
        // Validación y normalización
        // ---------------------------------------------------------------------

        private static void Validar(CategoriaEditModel m)
        {
            if (string.IsNullOrWhiteSpace(m.Nombre))
                throw new ReglaNegocioException("Ingresá el nombre de la categoría.");

            if (m.Nombre.Trim().Length > LargoMaximoNombre)
                throw new ReglaNegocioException(
                    "El nombre no puede superar los " + LargoMaximoNombre + " caracteres.");

            if (!string.IsNullOrWhiteSpace(m.Descripcion) &&
                m.Descripcion.Trim().Length > LargoMaximoDescripcion)
                throw new ReglaNegocioException(
                    "La descripción no puede superar los " + LargoMaximoDescripcion + " caracteres.");
        }

        /// <summary>
        /// Deja los datos listos para la base: sin espacios sobrantes y con null
        /// (en vez de cadena vacía) en los campos opcionales que quedaron en blanco.
        /// </summary>
        private static void Normalizar(CategoriaEditModel m)
        {
            m.Nombre = m.Nombre.Trim();
            m.Descripcion = string.IsNullOrWhiteSpace(m.Descripcion) ? null : m.Descripcion.Trim();
        }

        // ---------------------------------------------------------------------
        // Auditoría y traducción de errores
        // ---------------------------------------------------------------------

        private static string Resumen(CategoriaEditModel m)
        {
            return string.Format("Nombre={0}; Descripción={1}",
                m.Nombre == null ? "-" : m.Nombre.Trim(),
                string.IsNullOrWhiteSpace(m.Descripcion) ? "-" : m.Descripcion.Trim());
        }

        private static ReglaNegocioException TraducirDuplicado(DuplicadoException ex)
        {
            if (ex.Restriccion.IndexOf("UQ_Categoria_nombre", StringComparison.OrdinalIgnoreCase) >= 0)
                return new ReglaNegocioException("Ya existe una categoría con ese nombre.");

            return new ReglaNegocioException("Ya existe un registro con esos datos (valor duplicado).");
        }
    }
}
