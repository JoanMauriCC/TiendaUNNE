using System;
using System.Data;
using System.Globalization;

namespace TiendaUNNE
{
    /// <summary>
    /// Reglas de negocio de Producto: valida, normaliza, acota valores fuera de rango,
    /// arma los textos de auditoría y decide si corresponde alta o edición.
    /// También concentra los formatos de precio y stock que antes estaban sueltos en la grilla.
    /// </summary>
    public static class NegocioProducto
    {
        public const int LargoMaximoNombre = 150;
        public const int LargoMaximoDescripcion = 500;

        public const decimal PrecioMinimo = 0m;
        public const decimal PrecioMaximo = 1000000000m;

        // Rango permitido para cualquier cantidad de stock (incluido el stock mínimo de aviso).
        // No confundir con ProductoEditModel.StockMinimo, que es el umbral de "stock bajo".
        public const decimal StockMinimo = 0m;
        public const decimal StockMaximo = 1000000000m;

        /// <summary>El precio se muestra con 2 decimales y el stock con 3.</summary>
        public const string FormatoPrecio = "N2";
        public const string FormatoStock = "N3";

        // ---------------------------------------------------------------------
        // Consultas
        // ---------------------------------------------------------------------

        /// <summary>Texto de la columna Estado cuando el stock alcanzó el mínimo.</summary>
        public const string EstadoStockBajo = "Stock bajo";

        /// <summary>Texto de la columna Estado cuando el stock se agotó.</summary>
        public const string EstadoSinStock = "Sin stock";

        /// <summary>
        /// Productos activos o dados de baja, según <paramref name="activos"/>. Le agrega
        /// la columna Estado ("Sin stock", "Stock bajo" o vacío) que usa la grilla para
        /// el semáforo, así la pantalla no decide qué significa cada nivel.
        /// </summary>
        public static DataTable Listar(bool activos)
        {
            DataTable dt = ServicioProducto.Listar(activos);

            dt.Columns.Add("Estado", typeof(string));
            foreach (DataRow fila in dt.Rows)
                fila["Estado"] = EstadoDeStock((decimal)fila["Stock"], (decimal)fila["StockMinimo"]);

            return dt;
        }

        /// <summary>
        /// Sin stock si no queda nada; stock bajo si quedan como mucho las unidades del
        /// mínimo; vacío si está bien. Un mínimo en 0 significa "no avisar por stock bajo".
        /// </summary>
        public static string EstadoDeStock(decimal stock, decimal stockMinimo)
        {
            if (stock <= 0) return EstadoSinStock;
            if (stock <= stockMinimo) return EstadoStockBajo;
            return string.Empty;
        }

        public static ProductoEditModel ObtenerParaEdicion(int idProducto)
        {
            var producto = ServicioProducto.Obtener(idProducto);
            if (producto == null)
                throw new ReglaNegocioException("El producto ya no existe.");

            return producto;
        }

        // ---------------------------------------------------------------------
        // Alta y edición
        // ---------------------------------------------------------------------

        public static void Guardar(ProductoEditModel m, int idUsuarioSesion)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));

            Validar(m);
            Normalizar(m);

            try
            {
                if (m.EsAlta)
                {
                    ServicioProducto.Crear(m, idUsuarioSesion, Resumen(m));
                }
                else
                {
                    var anterior = ServicioProducto.Obtener(m.IdProducto);
                    if (anterior == null)
                        throw new ReglaNegocioException("El producto ya no existe.");

                    ServicioProducto.Actualizar(m, idUsuarioSesion, Resumen(anterior), Resumen(m));
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

        public static void DarDeBaja(int idProducto, int idUsuarioSesion)
        {
            var anterior = ServicioProducto.Obtener(idProducto);
            if (anterior == null)
                throw new ReglaNegocioException("El producto ya no existe.");

            int filas = ServicioProducto.DarDeBaja(idProducto, idUsuarioSesion, Resumen(anterior));

            if (filas == 0)
                throw new ReglaNegocioException("El producto ya estaba dado de baja o no existe.");
        }

        // ---------------------------------------------------------------------
        // Reactivación
        // ---------------------------------------------------------------------

        public static void DarDeAlta(int idProducto, int idUsuarioSesion)
        {
            var anterior = ServicioProducto.Obtener(idProducto);
            if (anterior == null)
                throw new ReglaNegocioException("El producto ya no existe.");

            int filas = ServicioProducto.DarDeAlta(idProducto, idUsuarioSesion, Resumen(anterior));

            if (filas == 0)
                throw new ReglaNegocioException("El producto ya estaba activo.");
        }

        // ---------------------------------------------------------------------
        // Validación, normalización y acotado
        // ---------------------------------------------------------------------

        private static void Validar(ProductoEditModel m)
        {
            if (string.IsNullOrWhiteSpace(m.Nombre))
                throw new ReglaNegocioException("Ingresá el nombre del producto.");

            if (m.Nombre.Trim().Length > LargoMaximoNombre)
                throw new ReglaNegocioException(
                    "El nombre no puede superar los " + LargoMaximoNombre + " caracteres.");

            if (!string.IsNullOrWhiteSpace(m.Descripcion) &&
                m.Descripcion.Trim().Length > LargoMaximoDescripcion)
                throw new ReglaNegocioException(
                    "La descripción no puede superar los " + LargoMaximoDescripcion + " caracteres.");

            if (m.IdCategoria <= 0)
                throw new ReglaNegocioException("Elegí una categoría.");

            if (m.PrecioVenta < PrecioMinimo || m.Stock < StockMinimo || m.StockMinimo < StockMinimo)
                throw new ReglaNegocioException("El precio, el stock y el stock mínimo no pueden ser negativos.");

            if (m.PrecioVenta > PrecioMaximo)
                throw new ReglaNegocioException("El precio de venta supera el máximo permitido.");

            if (m.Stock > StockMaximo || m.StockMinimo > StockMaximo)
                throw new ReglaNegocioException("El stock o el stock mínimo supera el máximo permitido.");
        }

        private static void Normalizar(ProductoEditModel m)
        {
            m.Nombre = m.Nombre.Trim();
            m.Descripcion = string.IsNullOrWhiteSpace(m.Descripcion) ? null : m.Descripcion.Trim();
        }

        /// <summary>
        /// Encierra un precio dentro del rango permitido. Lo usa el editor al cargar un
        /// producto existente, para que un valor viejo fuera de rango no rompa el control.
        /// </summary>
        public static decimal AcotarPrecio(decimal valor) => Acotar(valor, PrecioMinimo, PrecioMaximo);

        /// <summary>Ídem AcotarPrecio pero para el stock.</summary>
        public static decimal AcotarStock(decimal valor) => Acotar(valor, StockMinimo, StockMaximo);

        private static decimal Acotar(decimal valor, decimal minimo, decimal maximo)
        {
            if (valor < minimo) return minimo;
            if (valor > maximo) return maximo;
            return valor;
        }

        // ---------------------------------------------------------------------
        // Auditoría y traducción de errores
        // ---------------------------------------------------------------------

        private static string Resumen(ProductoEditModel m)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Nombre={0}; Categoría={1}; Desc={2}; PVenta={3}; Stock={4}; StockMin={5}",
                m.Nombre == null ? "-" : m.Nombre.Trim(),
                string.IsNullOrWhiteSpace(m.NombreCategoria) ? ("id " + m.IdCategoria) : m.NombreCategoria,
                string.IsNullOrWhiteSpace(m.Descripcion) ? "-" : m.Descripcion.Trim(),
                m.PrecioVenta, m.Stock, m.StockMinimo);
        }

        private static ReglaNegocioException TraducirDuplicado(DuplicadoException ex)
        {
            if (ex.Restriccion.IndexOf("Producto", StringComparison.OrdinalIgnoreCase) >= 0)
                return new ReglaNegocioException("Ya existe un producto con esos datos.");

            return new ReglaNegocioException("Ya existe un registro con esos datos (valor duplicado).");
        }
    }
}
