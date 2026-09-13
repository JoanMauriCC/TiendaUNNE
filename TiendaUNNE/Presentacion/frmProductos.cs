using System;
using System.Data;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>Listado de productos activos con alta/edición/baja lógica.</summary>
    public partial class frmProductos : Form
    {
        public frmProductos()
        {
            InitializeComponent();
        }

        private void frmProductos_Load(object sender, EventArgs e)
        {
            CargarGrilla();
        }

        private void CargarGrilla()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                int? idSeleccionado = ProductoSeleccionadoId();

                DataTable dt = NegocioProducto.ListarParaGrilla();
                dgvProductos.DataSource = dt;

                if (dgvProductos.Columns.Contains("IdProducto"))
                    dgvProductos.Columns["IdProducto"].Visible = false;

                AplicarFormato();

                if (!(idSeleccionado.HasValue && SeleccionarFilaPorId(idSeleccionado.Value)))
                    SeleccionarPrimeraFila();

                ActualizarBotones();
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo leer el listado de productos.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void AplicarFormato()
        {
            SetHeader("Nombre", "Nombre");
            SetHeader("Categoria", "Categoría");
            SetHeader("PrecioVenta", "Precio venta");
            SetHeader("Stock", "Stock");
            SetHeader("Activo", "Activo");

            SetFormatoNumerico("PrecioVenta", NegocioProducto.FormatoPrecio);
            SetFormatoNumerico("Stock", NegocioProducto.FormatoStock);
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

        private DataGridViewColumn PrimeraColumnaVisible()
        {
            foreach (DataGridViewColumn col in dgvProductos.Columns)
                if (col.Visible)
                    return col;
            return null;
        }

        private void SeleccionarPrimeraFila()
        {
            if (dgvProductos.Rows.Count == 0)
            {
                dgvProductos.ClearSelection();
                return;
            }

            var col = PrimeraColumnaVisible();
            dgvProductos.ClearSelection();
            dgvProductos.Rows[0].Selected = true;
            if (col != null)
                dgvProductos.CurrentCell = dgvProductos.Rows[0].Cells[col.Index];
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
            bool haySeleccion = ProductoSeleccionadoId().HasValue;
            btnEditar.Enabled = haySeleccion;
            btnBaja.Enabled = haySeleccion;
        }

        // -----------------------------------------------------------------
        // Acciones
        // -----------------------------------------------------------------

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            using (var editor = new frmProductoEditor(null))
            {
                if (editor.ShowDialog(this) == DialogResult.OK)
                    CargarGrilla();
            }
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            int? id = ProductoSeleccionadoId();
            if (!id.HasValue) return;

            using (var editor = new frmProductoEditor(id.Value))
            {
                if (editor.ShowDialog(this) == DialogResult.OK)
                    CargarGrilla();
            }
        }

        private void btnBaja_Click(object sender, EventArgs e)
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

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            CargarGrilla();
        }

        private void dgvProductos_SelectionChanged(object sender, EventArgs e)
        {
            ActualizarBotones();
        }

        private void dgvProductos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ActualizarBotones();
        }

        private void dgvProductos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && btnEditar.Enabled)
                btnEditar_Click(sender, e);
        }
    }
}
