using System;
using System.Data;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>Consulta de solo lectura de la tabla Auditoria (no se edita a mano).</summary>
    public partial class frmAuditoria : Form
    {
        public frmAuditoria()
        {
            InitializeComponent();
        }

        private void frmAuditoria_Load(object sender, EventArgs e)
        {
            cboTabla.Items.AddRange(NegocioAuditoria.TablasAuditables().ToArray());
            LimpiarFiltros();          // deja los filtros en su estado inicial y carga la grilla
        }

        private void CargarGrilla()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                string tabla = cboTabla.SelectedItem as string;
                DateTime? desde = dtpDesde.Checked ? dtpDesde.Value.Date : (DateTime?)null;
                DateTime? hasta = dtpHasta.Checked ? dtpHasta.Value.Date : (DateTime?)null;

                DataTable dt = NegocioAuditoria.Listar(tabla, desde, hasta);
                dgvAuditoria.DataSource = dt;
                AplicarFormato();
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Filtros inválidos",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo leer la auditoría.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void AplicarFormato()
        {
            SetHeader("Fecha", "Fecha y hora");
            SetHeader("Usuario", "Usuario");
            SetHeader("Accion", "Acción");
            SetHeader("Tabla", "Tabla");
            SetHeader("ValorAnterior", "Valor anterior");
            SetHeader("ValorNuevo", "Valor nuevo");

            if (dgvAuditoria.Columns.Contains("Fecha"))
            {
                dgvAuditoria.Columns["Fecha"].DefaultCellStyle.Format = NegocioAuditoria.FormatoFechaHora;
                dgvAuditoria.Columns["Fecha"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvAuditoria.Columns["Fecha"].Width = 140;
            }
            if (dgvAuditoria.Columns.Contains("Accion"))
            {
                dgvAuditoria.Columns["Accion"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvAuditoria.Columns["Accion"].Width = 110;
            }
            if (dgvAuditoria.Columns.Contains("Tabla"))
            {
                dgvAuditoria.Columns["Tabla"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvAuditoria.Columns["Tabla"].Width = 90;
            }
        }

        private void SetHeader(string columna, string texto)
        {
            if (dgvAuditoria.Columns.Contains(columna))
                dgvAuditoria.Columns[columna].HeaderText = texto;
        }

        private void LimpiarFiltros()
        {
            cboTabla.SelectedIndex = 0;   // "Todas"

            dtpDesde.Checked = false;
            dtpHasta.Checked = false;
            dtpDesde.Value = NegocioAuditoria.FechaDesdePorDefecto;
            dtpHasta.Value = NegocioAuditoria.FechaHastaPorDefecto;

            CargarGrilla();
        }

        private void btnFiltrar_Click(object sender, EventArgs e)
        {
            CargarGrilla();
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarFiltros();
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            CargarGrilla();
        }
    }
}
