using System;
using System.Data;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Sección de categorías con barra compacta: una sola fila para cargar (nombre y
    /// descripción) y el resto de la pantalla para el listado. Es un UserControl: se muestra
    /// dentro del panel de contenido de frmPrincipal.
    /// No hay botones "Nuevo"/"Editar": seleccionar una fila de la grilla carga esa categoría
    /// en el formulario; "Limpiar" lo deja listo para un alta.
    /// </summary>
    public partial class ucCategorias : UserControl
    {
        private CategoriaEditModel _original;   // distinto de null = editando esa categoría
        private bool _actualizandoGrilla;        // true mientras se reacomoda la selección
        private bool _verInactivas;              // false = activas, true = dadas de baja

        private const int Margen = 12;
        private const int Gap = 12;

        public ucCategorias()
        {
            InitializeComponent();
            panelFormulario.Resize += (s, e) => ReubicarFormulario();
            panelBarra.Resize += (s, e) => ReubicarBarra();
        }

        private bool EsAlta => _original == null;

        // -----------------------------------------------------------------
        // Atajos de teclado (ver AtajosTeclado para la lista completa)
        // -----------------------------------------------------------------

        /// <summary>
        /// Se intercepta acá y no en cada control porque Windows procesa Enter y Esc como
        /// teclas de diálogo antes de que el control llegue a verlas. Un UserControl no
        /// tiene AcceptButton/CancelButton como una ventana, por eso se resuelve así.
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Esc en el buscador: primero borra lo que se escribió, sin tocar el formulario.
            if (keyData == Keys.Escape && ActiveControl == txtBuscar && txtBuscar.Text.Length > 0)
            {
                txtBuscar.Clear();
                return true;
            }

            switch (AtajosTeclado.Interpretar(keyData, ActiveControl, panelFormulario, txtBuscar))
            {
                case AccionAtajo.Guardar:
                    btnGuardar.PerformClick();
                    return true;
                case AccionAtajo.Limpiar:
                    btnLimpiar.PerformClick();
                    return true;
                case AccionAtajo.Buscar:
                    txtBuscar.Focus();
                    txtBuscar.SelectAll();
                    return true;
                case AccionAtajo.Actualizar:
                    CargarGrilla();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ucCategorias_Load(object sender, EventArgs e)
        {
            LimpiarFormulario();
            ActualizarBotonBaja();
            CargarGrilla();
            ReubicarFormulario();
            ReubicarBarra();
        }

        // -----------------------------------------------------------------
        // Distribución: los campos y botones se reparten el ancho disponible
        // -----------------------------------------------------------------

        /// <summary>Guardar/Limpiar pegados a la derecha; Nombre y Descripción se reparten el resto.</summary>
        private void ReubicarFormulario()
        {
            int ancho = panelFormulario.ClientSize.Width;
            if (ancho <= 0) return;

            btnLimpiar.Left = ancho - Margen - btnLimpiar.Width;
            btnGuardar.Left = btnLimpiar.Left - 8 - btnGuardar.Width;

            int disponible = btnGuardar.Left - Gap - Margen;
            int anchoNombre = Math.Max(140, (disponible - Gap) * 35 / 100);
            int anchoDescripcion = Math.Max(140, disponible - Gap - anchoNombre);

            lblNombre.Left = Margen;
            txtNombre.Left = Margen;
            txtNombre.Width = anchoNombre;

            int xDescripcion = Margen + anchoNombre + Gap;
            lblDescripcion.Left = xDescripcion;
            txtDescripcion.Left = xDescripcion;
            txtDescripcion.Width = anchoDescripcion;
        }

        /// <summary>Botones pegados a la derecha; el buscador ocupa lo que sobra a la izquierda.</summary>
        private void ReubicarBarra()
        {
            int ancho = panelBarra.ClientSize.Width;
            if (ancho <= 0) return;

            btnActualizar.Left = ancho - Margen - btnActualizar.Width;
            btnBaja.Left = btnActualizar.Left - Gap - btnBaja.Width;
            rdoInactivas.Left = btnBaja.Left - Gap - rdoInactivas.Width;
            rdoActivas.Left = rdoInactivas.Left - 4 - rdoActivas.Width;

            txtBuscar.Width = Math.Max(120, rdoActivas.Left - Gap - txtBuscar.Left);
        }

        // -----------------------------------------------------------------
        // Formulario: alta / edición
        // -----------------------------------------------------------------

        /// <summary>Deja el formulario listo para cargar una categoría nueva y sin nada seleccionado.</summary>
        private void LimpiarFormulario()
        {
            _original = null;

            lblTituloForm.Text = "Nueva categoría";
            txtNombre.Clear();
            txtDescripcion.Clear();

            // ClearSelection() sola no borra CurrentRow: entre esta línea y la siguiente, la
            // grilla dispararía SelectionChanged todavía "viendo" la fila vieja, y como
            // _original ya es null, CargarSeleccionEnFormulario la volvería a cargar y pisaría
            // el vaciado. Se suprime ese evento intermedio con la misma bandera de CargarGrilla().
            _actualizandoGrilla = true;
            dgvCategorias.ClearSelection();
            dgvCategorias.CurrentCell = null;
            _actualizandoGrilla = false;

            ActualizarBotones();
            txtNombre.Focus();
        }

        /// <summary>Carga en el formulario los datos de la categoría seleccionada para editarla.</summary>
        private void CargarEnFormulario(CategoriaEditModel m)
        {
            _original = m;

            lblTituloForm.Text = "Editar categoría";
            txtNombre.Text = m.Nombre;
            txtDescripcion.Text = m.Descripcion;
        }

        /// <summary>Trae de la base y carga la categoría que está seleccionada en la grilla.</summary>
        private void CargarSeleccionEnFormulario()
        {
            int? id = CategoriaSeleccionadaId();
            if (!id.HasValue)
                return;

            // Ya está cargada (p.ej. la grilla se refrescó pero la selección no cambió): no repetir el viaje.
            if (!EsAlta && _original.IdCategoria == id.Value)
                return;

            try
            {
                Cursor = Cursors.WaitCursor;
                CargarEnFormulario(NegocioCategoria.ObtenerParaEdicion(id.Value));
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Categorías", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudieron cargar los datos de la categoría.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private CategoriaEditModel ArmarModelo()
        {
            return new CategoriaEditModel
            {
                IdCategoria = EsAlta ? 0 : _original.IdCategoria,
                Nombre = txtNombre.Text,
                Descripcion = txtDescripcion.Text
            };
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                NegocioCategoria.Guardar(ArmarModelo(), SesionActual.Usuario.IdUsuario);

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
                MessageBox.Show("No se pudo guardar la categoría.",
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

                int? idSeleccionada = CategoriaSeleccionadaId();

                DataTable dt = NegocioCategoria.Listar(activas: !_verInactivas);
                dgvCategorias.DataSource = dt;

                if (dgvCategorias.Columns.Contains("IdCategoria"))
                    dgvCategorias.Columns["IdCategoria"].Visible = false;

                AplicarFormato();
                AplicarFiltroBusqueda();

                // Si había algo seleccionado (y sigue existiendo) se reselecciona; si no,
                // la grilla queda sin selección y el formulario tal como esté.
                if (!(idSeleccionada.HasValue && SeleccionarFilaPorId(idSeleccionada.Value)))
                {
                    dgvCategorias.ClearSelection();
                    dgvCategorias.CurrentCell = null;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo leer el listado de categorías.",
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
            SetHeader("Descripcion", "Descripción");
            SetHeader("Productos", "Productos");
            SetHeader("Activo", "Activo");

            if (dgvCategorias.Columns.Contains("Productos"))
            {
                dgvCategorias.Columns["Productos"].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleRight;
                dgvCategorias.Columns["Productos"].HeaderCell.Style.Alignment =
                    DataGridViewContentAlignment.MiddleRight;
            }
        }

        private void SetHeader(string columna, string texto)
        {
            if (dgvCategorias.Columns.Contains(columna))
                dgvCategorias.Columns[columna].HeaderText = texto;
        }

        /// <summary>Filtra la grilla por lo tipeado en Buscar (nombre o descripción).</summary>
        private void AplicarFiltroBusqueda()
        {
            var dt = dgvCategorias.DataSource as DataTable;
            if (dt == null) return;

            dt.DefaultView.RowFilter = FiltroGrilla.Contiene(txtBuscar.Text, "Nombre", "Descripcion");
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltroBusqueda();
        }

        private DataGridViewColumn PrimeraColumnaVisible()
        {
            foreach (DataGridViewColumn col in dgvCategorias.Columns)
                if (col.Visible)
                    return col;
            return null;
        }

        private bool SeleccionarFilaPorId(int idCategoria)
        {
            var col = PrimeraColumnaVisible();
            foreach (DataGridViewRow fila in dgvCategorias.Rows)
            {
                if (IdDeFila(fila) == idCategoria)
                {
                    dgvCategorias.ClearSelection();
                    fila.Selected = true;
                    if (col != null)
                        dgvCategorias.CurrentCell = fila.Cells[col.Index];
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
            if (dgvCategorias.CurrentRow != null)
                return dgvCategorias.CurrentRow;
            if (dgvCategorias.SelectedRows.Count > 0)
                return dgvCategorias.SelectedRows[0];
            return null;
        }

        /// <summary>Lee el id desde el DataRowView enlazado, sin depender de la columna oculta.</summary>
        private static int? IdDeFila(DataGridViewRow fila)
        {
            var drv = fila == null ? null : fila.DataBoundItem as DataRowView;
            if (drv == null || drv["IdCategoria"] == DBNull.Value)
                return null;
            return Convert.ToInt32(drv["IdCategoria"]);
        }

        private int? CategoriaSeleccionadaId()
        {
            return IdDeFila(FilaSeleccionada());
        }

        private string CategoriaSeleccionadaNombre()
        {
            var fila = FilaSeleccionada();
            var drv = fila == null ? null : fila.DataBoundItem as DataRowView;
            return drv == null ? string.Empty : Convert.ToString(drv["Nombre"]);
        }

        private void ActualizarBotones()
        {
            btnBaja.Enabled = CategoriaSeleccionadaId().HasValue;
        }

        /// <summary>El botón dice "Dar de baja" viendo activas y "Dar de alta" viendo inactivas.</summary>
        private void ActualizarBotonBaja()
        {
            btnBaja.Text = _verInactivas ? "Da&r de alta" : "Da&r de baja";
        }

        // -----------------------------------------------------------------
        // Acciones
        // -----------------------------------------------------------------

        private void btnBaja_Click(object sender, EventArgs e)
        {
            if (_verInactivas)
                DarDeAlta();
            else
                DarDeBaja();
        }

        private void DarDeBaja()
        {
            int? id = CategoriaSeleccionadaId();
            if (!id.HasValue) return;

            var r = MessageBox.Show(
                "¿Seguro que querés dar de baja la categoría \"" + CategoriaSeleccionadaNombre() + "\"?",
                "Confirmar baja", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (r != DialogResult.Yes) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                NegocioCategoria.DarDeBaja(id.Value, SesionActual.Usuario.IdUsuario);

                // Si estaba en el formulario la categoría que acabamos de dar de baja,
                // no la dejamos ahí a mitad de edición.
                if (!EsAlta && _original.IdCategoria == id.Value)
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
                MessageBox.Show("No se pudo dar de baja la categoría.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void DarDeAlta()
        {
            int? id = CategoriaSeleccionadaId();
            if (!id.HasValue) return;

            var r = MessageBox.Show(
                "¿Reactivar la categoría \"" + CategoriaSeleccionadaNombre() + "\"?",
                "Confirmar alta", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (r != DialogResult.Yes) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                NegocioCategoria.DarDeAlta(id.Value, SesionActual.Usuario.IdUsuario);
                CargarGrilla();
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo dar de alta",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo dar de alta la categoría.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            CargarGrilla();
        }

        /// <summary>Alterna entre ver las categorías activas o las dadas de baja.</summary>
        private void rdoInactivas_CheckedChanged(object sender, EventArgs e)
        {
            _verInactivas = rdoInactivas.Checked;
            ActualizarBotonBaja();
            CargarGrilla();
        }

        private void dgvCategorias_SelectionChanged(object sender, EventArgs e)
        {
            if (_actualizandoGrilla) return;   // evita recargar el formulario en medio de un refresco

            ActualizarBotones();
            CargarSeleccionEnFormulario();
        }
    }
}
