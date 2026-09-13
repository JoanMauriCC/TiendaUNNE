using System;
using System.Data;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>Listado de usuarios activos (Persona + Usuario + Perfil) con alta/edición/baja.</summary>
    public partial class frmUsuarios : Form
    {
        public frmUsuarios()
        {
            InitializeComponent();
        }

        private void frmUsuarios_Load(object sender, EventArgs e)
        {
            CargarGrilla();
        }

        private void CargarGrilla()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                int? idSeleccionado = UsuarioSeleccionadoId();

                DataTable dt = NegocioUsuario.ListarActivos();
                dgvUsuarios.DataSource = dt;

                if (dgvUsuarios.Columns.Contains("IdUsuario"))
                    dgvUsuarios.Columns["IdUsuario"].Visible = false;

                AplicarEncabezados();

                // Reposiciona la selección en una celda VISIBLE (nunca sobre la columna oculta).
                if (!(idSeleccionado.HasValue && SeleccionarFilaPorId(idSeleccionado.Value)))
                    SeleccionarPrimeraFila();

                ActualizarBotones();
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo leer el listado de usuarios.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void AplicarEncabezados()
        {
            SetHeader("DniCuit", "DNI/CUIT");
            SetHeader("Apellido", "Apellido");
            SetHeader("Nombre", "Nombre");
            SetHeader("Telefono", "Teléfono");
            SetHeader("Email", "Email");
            SetHeader("Usuario", "Usuario");
            SetHeader("Perfil", "Perfil");
            SetHeader("Activo", "Activo");
        }

        private void SetHeader(string columna, string texto)
        {
            if (dgvUsuarios.Columns.Contains(columna))
                dgvUsuarios.Columns[columna].HeaderText = texto;
        }

        private DataGridViewColumn PrimeraColumnaVisible()
        {
            foreach (DataGridViewColumn col in dgvUsuarios.Columns)
                if (col.Visible)
                    return col;
            return null;
        }

        private void SeleccionarPrimeraFila()
        {
            if (dgvUsuarios.Rows.Count == 0)
            {
                dgvUsuarios.ClearSelection();
                return;
            }

            var col = PrimeraColumnaVisible();
            dgvUsuarios.ClearSelection();
            dgvUsuarios.Rows[0].Selected = true;
            if (col != null)
                dgvUsuarios.CurrentCell = dgvUsuarios.Rows[0].Cells[col.Index];
        }

        private bool SeleccionarFilaPorId(int idUsuario)
        {
            var col = PrimeraColumnaVisible();
            foreach (DataGridViewRow fila in dgvUsuarios.Rows)
            {
                if (IdDeFila(fila) == idUsuario)
                {
                    dgvUsuarios.ClearSelection();
                    fila.Selected = true;
                    if (col != null)
                        dgvUsuarios.CurrentCell = fila.Cells[col.Index];
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
            if (dgvUsuarios.CurrentRow != null)
                return dgvUsuarios.CurrentRow;
            if (dgvUsuarios.SelectedRows.Count > 0)
                return dgvUsuarios.SelectedRows[0];
            return null;
        }

        /// <summary>Lee el id desde el DataRowView enlazado, sin depender de la columna oculta.</summary>
        private static int? IdDeFila(DataGridViewRow fila)
        {
            var drv = fila == null ? null : fila.DataBoundItem as DataRowView;
            if (drv == null || drv["IdUsuario"] == DBNull.Value)
                return null;
            return Convert.ToInt32(drv["IdUsuario"]);
        }

        private int? UsuarioSeleccionadoId()
        {
            return IdDeFila(FilaSeleccionada());
        }

        private string UsuarioSeleccionadoDescripcion()
        {
            var drv = FilaSeleccionada() == null ? null : FilaSeleccionada().DataBoundItem as DataRowView;
            if (drv == null) return string.Empty;
            return string.Format("{0}, {1} ({2})", drv["Apellido"], drv["Nombre"], drv["Usuario"]);
        }

        private void ActualizarBotones()
        {
            bool haySeleccion = UsuarioSeleccionadoId().HasValue;
            btnEditar.Enabled = haySeleccion;
            btnBaja.Enabled = haySeleccion;
        }

        // -----------------------------------------------------------------
        // Acciones
        // -----------------------------------------------------------------

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            using (var editor = new frmUsuarioEditor(null))
            {
                if (editor.ShowDialog(this) == DialogResult.OK)
                    CargarGrilla();
            }
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            int? id = UsuarioSeleccionadoId();
            if (!id.HasValue) return;

            using (var editor = new frmUsuarioEditor(id.Value))
            {
                if (editor.ShowDialog(this) == DialogResult.OK)
                    CargarGrilla();
            }
        }

        private void btnBaja_Click(object sender, EventArgs e)
        {
            int? id = UsuarioSeleccionadoId();
            if (!id.HasValue) return;

            var r = MessageBox.Show(
                "¿Seguro que querés dar de baja a " + UsuarioSeleccionadoDescripcion() + "?",
                "Confirmar baja", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (r != DialogResult.Yes) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                NegocioUsuario.DarDeBaja(id.Value, SesionActual.Usuario.IdUsuario);
                CargarGrilla();
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo dar de baja",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo dar de baja el usuario.",
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

        // Ambos eventos mantienen el estado de los botones: SelectionChanged cubre el
        // cambio de fila; CellClick cubre el clic sobre la fila que ya estaba seleccionada.
        private void dgvUsuarios_SelectionChanged(object sender, EventArgs e)
        {
            ActualizarBotones();
        }

        private void dgvUsuarios_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ActualizarBotones();
        }

        private void dgvUsuarios_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && btnEditar.Enabled)
                btnEditar_Click(sender, e);
        }
    }
}
