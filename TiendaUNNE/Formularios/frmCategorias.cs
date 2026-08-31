using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>Listado de categorías activas con alta/edición/baja lógica.</summary>
    public partial class frmCategorias : Form
    {
        public frmCategorias()
        {
            InitializeComponent();
        }

        private void frmCategorias_Load(object sender, EventArgs e)
        {
            CargarGrilla();
        }

        private void CargarGrilla()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                int? idSeleccionado = CategoriaSeleccionadaId();

                DataTable dt = ServicioCategoria.ListarParaGrilla();
                dgvCategorias.DataSource = dt;

                if (dgvCategorias.Columns.Contains("IdCategoria"))
                    dgvCategorias.Columns["IdCategoria"].Visible = false;

                AplicarEncabezados();

                if (!(idSeleccionado.HasValue && SeleccionarFilaPorId(idSeleccionado.Value)))
                    SeleccionarPrimeraFila();

                ActualizarBotones();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("No se pudo leer el listado de categorías.\n\n" + ex.Message,
                    "Base de datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void AplicarEncabezados()
        {
            SetHeader("Nombre", "Nombre");
            SetHeader("Descripcion", "Descripción");
            SetHeader("Activo", "Activo");
        }

        private void SetHeader(string columna, string texto)
        {
            if (dgvCategorias.Columns.Contains(columna))
                dgvCategorias.Columns[columna].HeaderText = texto;
        }

        private DataGridViewColumn PrimeraColumnaVisible()
        {
            foreach (DataGridViewColumn col in dgvCategorias.Columns)
                if (col.Visible)
                    return col;
            return null;
        }

        private void SeleccionarPrimeraFila()
        {
            if (dgvCategorias.Rows.Count == 0)
            {
                dgvCategorias.ClearSelection();
                return;
            }

            var col = PrimeraColumnaVisible();
            dgvCategorias.ClearSelection();
            dgvCategorias.Rows[0].Selected = true;
            if (col != null)
                dgvCategorias.CurrentCell = dgvCategorias.Rows[0].Cells[col.Index];
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

        private string CategoriaSeleccionadaDescripcion()
        {
            var fila = FilaSeleccionada();
            var drv = fila == null ? null : fila.DataBoundItem as DataRowView;
            return drv == null ? string.Empty : Convert.ToString(drv["Nombre"]);
        }

        private void ActualizarBotones()
        {
            bool haySeleccion = CategoriaSeleccionadaId().HasValue;
            btnEditar.Enabled = haySeleccion;
            btnBaja.Enabled = haySeleccion;
        }

        // -----------------------------------------------------------------
        // Acciones
        // -----------------------------------------------------------------

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            using (var editor = new frmCategoriaEditor(null))
            {
                if (editor.ShowDialog(this) == DialogResult.OK)
                    CargarGrilla();
            }
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            int? id = CategoriaSeleccionadaId();
            if (!id.HasValue) return;

            using (var editor = new frmCategoriaEditor(id.Value))
            {
                if (editor.ShowDialog(this) == DialogResult.OK)
                    CargarGrilla();
            }
        }

        private void btnBaja_Click(object sender, EventArgs e)
        {
            int? id = CategoriaSeleccionadaId();
            if (!id.HasValue) return;

            var r = MessageBox.Show(
                "¿Seguro que querés dar de baja la categoría \"" + CategoriaSeleccionadaDescripcion() + "\"?",
                "Confirmar baja", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (r != DialogResult.Yes) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                ServicioCategoria.DarDeBaja(id.Value, SesionActual.Usuario.IdUsuario);
                CargarGrilla();
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo dar de baja",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error de base de datos.\n\n" + ex.Message,
                    "Base de datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void dgvCategorias_SelectionChanged(object sender, EventArgs e)
        {
            ActualizarBotones();
        }

        private void dgvCategorias_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ActualizarBotones();
        }

        private void dgvCategorias_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && btnEditar.Enabled)
                btnEditar_Click(sender, e);
        }
    }
}
