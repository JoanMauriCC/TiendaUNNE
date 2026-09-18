using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Sección de productos: formulario de alta/edición arriba, siempre visible, y listado
    /// abajo. Es un UserControl: se muestra dentro del panel de contenido de frmPrincipal.
    /// No hay botones "Nuevo"/"Editar": seleccionar una fila de la grilla carga ese producto
    /// en el formulario; "Limpiar" lo deja listo para un alta.
    /// Las filas se pintan como semáforo (amarillo = stock bajo, rojo = sin stock), pero qué
    /// significa cada nivel lo decide NegocioProducto.EstadoDeStock: acá solo se muestra.
    /// </summary>
    public partial class ucProductos : UserControl
    {
        private static readonly Color ColorStockBajo = Color.FromArgb(255, 243, 205);
        private static readonly Color ColorSinStock = Color.FromArgb(253, 228, 228);

        private ProductoEditModel _original;   // distinto de null = editando ese producto
        private bool _actualizandoGrilla;       // true mientras se reacomoda la selección
        private bool _verInactivos;             // false = activos, true = dados de baja

        // Ancho fijo del riel de acciones de la derecha (Buscar + tarjetas) y márgenes;
        // las 3 columnas del formulario se reparten todo lo demás.
        private const int RailAncho = 260;
        private const int RailMargen = 24;
        private const int FormMargen = 12;
        private const int FormGapColumna = 14;

        public ucProductos()
        {
            InitializeComponent();
            panelFormulario.Resize += (s, e) => ReubicarFormulario();
        }

        private bool EsAlta => _original == null;

        private void ucProductos_Load(object sender, EventArgs e)
        {
            CargarCategorias();
            LimpiarFormulario();
            ActualizarTarjetaBaja();
            CargarGrilla();
            ReubicarFormulario();
        }

        /// <summary>
        /// Reparte el ancho disponible del panel: las 3 columnas del formulario crecen
        /// parejo y el riel de acciones queda pegado al borde derecho. Se llama al cargar
        /// y cada vez que cambia el tamaño de la ventana.
        /// </summary>
        private void ReubicarFormulario()
        {
            int anchoPanel = panelFormulario.ClientSize.Width;
            if (anchoPanel <= 0) return;

            int railX = anchoPanel - RailAncho - FormMargen;
            int anchoForm = railX - RailMargen - FormMargen;
            int colAncho = Math.Max(90, (anchoForm - 2 * FormGapColumna) / 3);

            int col1 = FormMargen;
            int col2 = col1 + colAncho + FormGapColumna;
            int col3 = col2 + colAncho + FormGapColumna;

            // Si la ventana es demasiado angosta, el riel no se mete debajo de la columna 3.
            railX = Math.Max(railX, col3 + colAncho + RailMargen);

            // Fila 1: el nombre ocupa 2 columnas; la categoría, la tercera.
            UbicarCampo(lblNombre, txtNombre, col1, 2 * colAncho + FormGapColumna);
            UbicarCampo(lblCategoria, cboCategoria, col3, colAncho);

            // Fila 2: precio, stock y stock mínimo.
            UbicarCampo(lblPrecioVenta, numPrecioVenta, col1, colAncho);
            UbicarCampo(lblStock, numStock, col2, colAncho);
            UbicarCampo(lblStockMinimo, numStockMinimo, col3, colAncho);

            // Fila 3: la descripción ocupa las 3 columnas.
            UbicarCampo(lblDescripcion, txtDescripcion, col1, 3 * colAncho + 2 * FormGapColumna);

            lblObligatorios.Left = col1;

            int anchoBotones = btnGuardar.Width + 6 + btnLimpiar.Width;
            int xBotones = col2 + (colAncho - anchoBotones) / 2;
            btnGuardar.Left = xBotones;
            btnLimpiar.Left = xBotones + btnGuardar.Width + 6;

            lblBuscar.Left = railX;
            txtBuscar.Left = railX;
            tarjetaVerInactivos.Left = railX;
            tarjetaBaja.Left = railX;
            tarjetaActualizar.Left = railX;
        }

        private static void UbicarCampo(Label etiqueta, Control campo, int x, int ancho)
        {
            etiqueta.Left = x;
            campo.Left = x;
            campo.Width = ancho;
        }

        // -----------------------------------------------------------------
        // Formulario: alta / edición
        // -----------------------------------------------------------------

        private void CargarCategorias()
        {
            cboCategoria.DataSource = NegocioCategoria.ListarActivas();
            cboCategoria.DisplayMember = "Nombre";
            cboCategoria.ValueMember = "Id";
            cboCategoria.SelectedIndex = -1;
        }

        /// <summary>Deja el formulario listo para cargar un producto nuevo y sin nada seleccionado.</summary>
        private void LimpiarFormulario()
        {
            _original = null;

            lblTituloForm.Text = "Nuevo producto";

            txtNombre.Clear();
            cboCategoria.SelectedIndex = -1;
            numPrecioVenta.Value = 0;
            numStock.Value = 0;
            numStockMinimo.Value = 0;
            txtDescripcion.Clear();

            // ClearSelection() sola no borra CurrentRow: entre esta línea y la siguiente, la
            // grilla dispararía SelectionChanged todavía "viendo" la fila vieja, y como
            // _original ya es null, CargarSeleccionEnFormulario la volvería a cargar y pisaría
            // el vaciado. Se suprime ese evento intermedio con la misma bandera de CargarGrilla().
            _actualizandoGrilla = true;
            dgvProductos.ClearSelection();
            dgvProductos.CurrentCell = null;
            _actualizandoGrilla = false;

            ActualizarBotones();
            txtNombre.Focus();
        }

        /// <summary>Carga en el formulario los datos del producto seleccionado para editarlo.</summary>
        private void CargarEnFormulario(ProductoEditModel m)
        {
            _original = m;

            lblTituloForm.Text = "Editar producto";

            txtNombre.Text = m.Nombre;
            cboCategoria.SelectedValue = m.IdCategoria;
            numPrecioVenta.Value = NegocioProducto.AcotarPrecio(m.PrecioVenta);
            numStock.Value = NegocioProducto.AcotarStock(m.Stock);
            numStockMinimo.Value = NegocioProducto.AcotarStock(m.StockMinimo);
            txtDescripcion.Text = m.Descripcion;
        }

        /// <summary>Trae de la base y carga el producto que está seleccionado en la grilla.</summary>
        private void CargarSeleccionEnFormulario()
        {
            int? id = ProductoSeleccionadoId();
            if (!id.HasValue)
                return;

            // Ya está cargado (p.ej. la grilla se refrescó pero la selección no cambió): no repetir el viaje.
            if (!EsAlta && _original.IdProducto == id.Value)
                return;

            try
            {
                Cursor = Cursors.WaitCursor;
                CargarEnFormulario(NegocioProducto.ObtenerParaEdicion(id.Value));
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Productos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudieron cargar los datos del producto.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private ProductoEditModel ArmarModelo()
        {
            var categoria = cboCategoria.SelectedItem as CategoriaItem;

            return new ProductoEditModel
            {
                IdProducto = EsAlta ? 0 : _original.IdProducto,
                Nombre = txtNombre.Text,
                Descripcion = txtDescripcion.Text,
                PrecioVenta = numPrecioVenta.Value,
                Stock = numStock.Value,
                StockMinimo = numStockMinimo.Value,
                IdCategoria = categoria == null ? 0 : categoria.Id,
                NombreCategoria = categoria == null ? null : categoria.Nombre
            };
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                NegocioProducto.Guardar(ArmarModelo(), SesionActual.Usuario.IdUsuario);

                LimpiarFormulario();
                CargarGrilla();
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo guardar",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo guardar el producto.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        // -----------------------------------------------------------------
        // Listado
        // -----------------------------------------------------------------

        private void CargarGrilla()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                _actualizandoGrilla = true;

                int? idSeleccionado = ProductoSeleccionadoId();

                DataTable dt = NegocioProducto.Listar(activos: !_verInactivos);
                dgvProductos.DataSource = dt;

                if (dgvProductos.Columns.Contains("IdProducto"))
                    dgvProductos.Columns["IdProducto"].Visible = false;

                AplicarFormato();
                AplicarFiltroBusqueda();

                // Si había algo seleccionado (y sigue existiendo) se reselecciona; si no,
                // la grilla queda sin selección y el formulario tal como esté.
                if (!(idSeleccionado.HasValue && SeleccionarFilaPorId(idSeleccionado.Value)))
                {
                    dgvProductos.ClearSelection();
                    dgvProductos.CurrentCell = null;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo leer el listado de productos.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _actualizandoGrilla = false;
                Cursor = Cursors.Default;
            }

            ActualizarBotones();
            CargarSeleccionEnFormulario();
        }

        private void AplicarFormato()
        {
            SetHeader("Nombre", "Nombre");
            SetHeader("Categoria", "Categoría");
            SetHeader("PrecioVenta", "Precio venta");
            SetHeader("Stock", "Stock");
            SetHeader("StockMinimo", "Stock mínimo");
            SetHeader("Estado", "Estado");
            SetHeader("Activo", "Activo");

            SetFormatoNumerico("PrecioVenta", NegocioProducto.FormatoPrecio);
            SetFormatoNumerico("Stock", NegocioProducto.FormatoStock);
            SetFormatoNumerico("StockMinimo", NegocioProducto.FormatoStock);

            // Estado va pegado al stock mínimo, no al final de la grilla.
            if (dgvProductos.Columns.Contains("Estado") && dgvProductos.Columns.Contains("StockMinimo"))
                dgvProductos.Columns["Estado"].DisplayIndex = dgvProductos.Columns["StockMinimo"].DisplayIndex + 1;
        }

        private void SetHeader(string columna, string texto)
        {
            if (dgvProductos.Columns.Contains(columna))
                dgvProductos.Columns[columna].HeaderText = texto;
        }

        private void SetFormatoNumerico(string columna, string formato)
        {
            if (!dgvProductos.Columns.Contains(columna)) return;
            dgvProductos.Columns[columna].DefaultCellStyle.Format = formato;
            dgvProductos.Columns[columna].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        /// <summary>Semáforo: pinta la fila según la columna Estado que arma Negocio.</summary>
        private void dgvProductos_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow fila = dgvProductos.Rows[e.RowIndex];
            if (fila.Selected) return;   // la fila seleccionada conserva el color de selección

            var drv = fila.DataBoundItem as DataRowView;
            if (drv == null || !drv.Row.Table.Columns.Contains("Estado")) return;

            string estado = Convert.ToString(drv["Estado"]);
            if (estado == NegocioProducto.EstadoSinStock)
                e.CellStyle.BackColor = ColorSinStock;
            else if (estado == NegocioProducto.EstadoStockBajo)
                e.CellStyle.BackColor = ColorStockBajo;
        }

        /// <summary>Filtra la grilla por lo tipeado en Buscar (nombre o categoría).</summary>
        private void AplicarFiltroBusqueda()
        {
            var dt = dgvProductos.DataSource as DataTable;
            if (dt == null) return;

            string texto = txtBuscar.Text.Trim();
            if (texto.Length == 0)
            {
                dt.DefaultView.RowFilter = string.Empty;
                return;
            }

            string escapado = texto.Replace("'", "''");
            dt.DefaultView.RowFilter = string.Format(
                "Nombre LIKE '%{0}%' OR Categoria LIKE '%{0}%'", escapado);
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltroBusqueda();
        }

        private DataGridViewColumn PrimeraColumnaVisible()
        {
            foreach (DataGridViewColumn col in dgvProductos.Columns)
                if (col.Visible)
                    return col;
            return null;
        }

        private bool SeleccionarFilaPorId(int idProducto)
        {
            var col = PrimeraColumnaVisible();
            foreach (DataGridViewRow fila in dgvProductos.Rows)
            {
                if (IdDeFila(fila) == idProducto)
                {
                    dgvProductos.ClearSelection();
                    fila.Selected = true;
                    if (col != null)
                        dgvProductos.CurrentCell = fila.Cells[col.Index];
                    return true;
                }
            }
            return false;
        }

        // -----------------------------------------------------------------
        // Selección actual
        // -----------------------------------------------------------------

        private DataGridViewRow FilaSeleccionada()
        {
            if (dgvProductos.CurrentRow != null)
                return dgvProductos.CurrentRow;
            if (dgvProductos.SelectedRows.Count > 0)
                return dgvProductos.SelectedRows[0];
            return null;
        }

        /// <summary>Lee el id desde el DataRowView enlazado, sin depender de la columna oculta.</summary>
        private static int? IdDeFila(DataGridViewRow fila)
        {
            var drv = fila == null ? null : fila.DataBoundItem as DataRowView;
            if (drv == null || drv["IdProducto"] == DBNull.Value)
                return null;
            return Convert.ToInt32(drv["IdProducto"]);
        }

        private int? ProductoSeleccionadoId()
        {
            return IdDeFila(FilaSeleccionada());
        }

        private string ProductoSeleccionadoDescripcion()
        {
            var fila = FilaSeleccionada();
            var drv = fila == null ? null : fila.DataBoundItem as DataRowView;
            return drv == null ? string.Empty : Convert.ToString(drv["Nombre"]);
        }

        private void ActualizarBotones()
        {
            tarjetaBaja.Enabled = ProductoSeleccionadoId().HasValue;
        }

        /// <summary>Pone la tarjeta en modo "Dar de baja" o "Dar de alta" según lo que se está viendo.</summary>
        private void ActualizarTarjetaBaja()
        {
            if (_verInactivos)
            {
                tarjetaBaja.Icono = IconoAccion.AltaProducto;
                tarjetaBaja.Titulo = "Dar de alta";
                tarjetaBaja.Descripcion = "Reactiva al producto seleccionado";
            }
            else
            {
                tarjetaBaja.Icono = IconoAccion.BajaProducto;
                tarjetaBaja.Titulo = "Dar de baja";
                tarjetaBaja.Descripcion = "Al producto seleccionado";
            }
            tarjetaBaja.Invalidate();
        }

        // -----------------------------------------------------------------
        // Acciones
        // -----------------------------------------------------------------

        private void tarjetaBaja_Click(object sender, EventArgs e)
        {
            if (_verInactivos)
                DarDeAlta();
            else
                DarDeBaja();
        }

        private void DarDeBaja()
        {
            int? id = ProductoSeleccionadoId();
            if (!id.HasValue) return;

            var r = MessageBox.Show(
                "¿Seguro que querés dar de baja el producto \"" + ProductoSeleccionadoDescripcion() + "\"?",
                "Confirmar baja", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (r != DialogResult.Yes) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                NegocioProducto.DarDeBaja(id.Value, SesionActual.Usuario.IdUsuario);

                // Si estaba en el formulario el producto que acabamos de dar de baja,
                // no lo dejamos ahí a mitad de edición.
                if (!EsAlta && _original.IdProducto == id.Value)
                    LimpiarFormulario();

                CargarGrilla();
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo dar de baja",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo dar de baja el producto.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void DarDeAlta()
        {
            int? id = ProductoSeleccionadoId();
            if (!id.HasValue) return;

            var r = MessageBox.Show(
                "¿Reactivar el producto \"" + ProductoSeleccionadoDescripcion() + "\"?",
                "Confirmar alta", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (r != DialogResult.Yes) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                NegocioProducto.DarDeAlta(id.Value, SesionActual.Usuario.IdUsuario);
                CargarGrilla();
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo dar de alta",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo dar de alta el producto.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void tarjetaActualizar_Click(object sender, EventArgs e)
        {
            CargarGrilla();
        }

        /// <summary>Alterna entre ver los productos activos o los dados de baja.</summary>
        private void tarjetaVerInactivos_Click(object sender, EventArgs e)
        {
            _verInactivos = !_verInactivos;
            tarjetaVerInactivos.Activa = _verInactivos;
            tarjetaVerInactivos.Titulo = _verInactivos ? "Mostrando: Inactivos" : "Mostrando: Activos";
            tarjetaVerInactivos.Descripcion = _verInactivos
                ? "Tocá para ver los activos"
                : "Tocá para ver los dados de baja";
            tarjetaVerInactivos.Invalidate();
            ActualizarTarjetaBaja();
            CargarGrilla();
        }

        private void dgvProductos_SelectionChanged(object sender, EventArgs e)
        {
            if (_actualizandoGrilla) return;   // evita recargar el formulario en medio de un refresco

            ActualizarBotones();
            CargarSeleccionEnFormulario();
        }
    }
}
