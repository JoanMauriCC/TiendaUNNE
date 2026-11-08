using System;
using System.Data;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Consulta de solo lectura de la tabla Auditoria (no se edita a mano). Es un
    /// UserControl: se muestra dentro del panel de contenido de frmPrincipal.
    /// </summary>
    public partial class ucAuditoria : UserControl
    {
        public ucAuditoria()
        {
            InitializeComponent();
        }

        private void ucAuditoria_Load(object sender, EventArgs e)
        {
            cboTabla.Items.AddRange(NegocioAuditoria.Modulos().ToArray());
            dgvAuditoria.CellToolTipTextNeeded += dgvAuditoria_CellToolTipTextNeeded;
            LimpiarFiltros();          // deja los filtros en su estado inicial y carga la grilla
        }

        // Mientras no hay panel de detalle, el texto completo del movimiento se ve al apoyar el mouse en el resumen.
        private void dgvAuditoria_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex < 0 || dgvAuditoria.Columns[e.ColumnIndex].Name != "Resumen") return;

            var fila = dgvAuditoria.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (fila == null) return;

            string antes = Convert.ToString(fila["ValorAnterior"]);
            string despues = Convert.ToString(fila["ValorNuevo"]);
            e.ToolTipText = (antes.Length > 0 ? "Antes: " + antes + "\n" : string.Empty) +
                            (despues.Length > 0 ? "Después: " + despues : string.Empty);
        }

        private void CargarGrilla()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                var modulo = cboTabla.SelectedItem as ModuloAuditoria;
                string tabla = modulo == null ? null : modulo.Tabla;
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
            SetHeader("Modulo", "Módulo");
            SetHeader("Resumen", "Qué pasó");

            // Los textos completos y la tabla técnica quedan guardados en la fila, pero no se muestran.
            foreach (string oculta in new[] { "ValorAnterior", "ValorNuevo", "Tabla" })
                if (dgvAuditoria.Columns.Contains(oculta))
                    dgvAuditoria.Columns[oculta].Visible = false;

            // Orden de lectura: cuándo, quién, qué hizo, dónde y el resumen.
            string[] orden = { "Fecha", "Usuario", "Accion", "Modulo", "Resumen" };
            for (int i = 0; i < orden.Length; i++)
                if (dgvAuditoria.Columns.Contains(orden[i]))
                    dgvAuditoria.Columns[orden[i]].DisplayIndex = i;

            FijarAncho("Fecha", 115, NegocioAuditoria.FormatoFechaHora);
            FijarAncho("Usuario", 140);
            FijarAncho("Accion", 100);
            FijarAncho("Modulo", 95);
            // «Resumen» ocupa el resto del ancho.
        }

        private void FijarAncho(string columna, int ancho, string formato = null)
        {
            if (!dgvAuditoria.Columns.Contains(columna)) return;

            DataGridViewColumn c = dgvAuditoria.Columns[columna];
            c.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            c.Width = ancho;
            if (formato != null) c.DefaultCellStyle.Format = formato;
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
