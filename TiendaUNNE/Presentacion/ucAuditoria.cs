using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Consulta de solo lectura de la tabla Auditoria (no se edita a mano). Es un
    /// UserControl: se muestra dentro del panel de contenido de frmPrincipal.
    /// </summary>
    public partial class ucAuditoria : UserControl
    {
        private static readonly Color ColorAntes = Color.FromArgb(180, 35, 24);
        private static readonly Color ColorDespues = Color.FromArgb(21, 128, 61);

        // Panel de detalle, debajo de la lista: se arma por código.
        private readonly Panel panelDetalle = new Panel();
        private readonly Label lblDetalleTitulo = new Label();
        private readonly Label lblDetalleResumen = new Label();
        private readonly DataGridView dgvDetalle = new DataGridView();
        private readonly TextBox txtDetalle = new TextBox();

        public ucAuditoria()
        {
            InitializeComponent();
            ArmarDetalle();
        }

        private void ArmarDetalle()
        {
            panelDetalle.Dock = DockStyle.Bottom;
            panelDetalle.Height = 190;
            panelDetalle.Padding = new Padding(0, 6, 0, 0);

            var encabezado = new Panel { Dock = DockStyle.Top, Height = 46 };
            lblDetalleTitulo.Font = new Font(Font, FontStyle.Bold);
            lblDetalleTitulo.AutoSize = false;
            lblDetalleTitulo.Dock = DockStyle.Top;
            lblDetalleTitulo.Height = 22;
            lblDetalleResumen.AutoSize = false;
            lblDetalleResumen.Dock = DockStyle.Fill;
            lblDetalleResumen.ForeColor = Color.FromArgb(91, 98, 112);
            encabezado.Controls.Add(lblDetalleResumen);
            encabezado.Controls.Add(lblDetalleTitulo);

            dgvDetalle.Dock = DockStyle.Fill;
            dgvDetalle.ReadOnly = true;
            dgvDetalle.AllowUserToAddRows = false;
            dgvDetalle.AllowUserToDeleteRows = false;
            dgvDetalle.AllowUserToResizeRows = false;
            dgvDetalle.RowHeadersVisible = false;
            dgvDetalle.MultiSelect = false;
            dgvDetalle.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDetalle.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDetalle.BackgroundColor = Color.White;
            dgvDetalle.BorderStyle = BorderStyle.FixedSingle;
            dgvDetalle.Columns.Add("Campo", "Dato");
            dgvDetalle.Columns.Add("Antes", "Antes");
            dgvDetalle.Columns.Add("Despues", "Ahora");
            dgvDetalle.Columns["Campo"].FillWeight = 22;
            dgvDetalle.Columns["Antes"].FillWeight = 39;
            dgvDetalle.Columns["Despues"].FillWeight = 39;
            dgvDetalle.Columns["Antes"].DefaultCellStyle.ForeColor = ColorAntes;
            dgvDetalle.Columns["Despues"].DefaultCellStyle.ForeColor = ColorDespues;
            dgvDetalle.SelectionChanged += (s, e) => dgvDetalle.ClearSelection();

            txtDetalle.Dock = DockStyle.Fill;
            txtDetalle.Multiline = true;
            txtDetalle.ReadOnly = true;
            txtDetalle.ScrollBars = ScrollBars.Vertical;
            txtDetalle.BackColor = Color.White;
            txtDetalle.BorderStyle = BorderStyle.FixedSingle;
            txtDetalle.Visible = false;

            panelDetalle.Controls.Add(dgvDetalle);
            panelDetalle.Controls.Add(txtDetalle);
            panelDetalle.Controls.Add(encabezado);
            Controls.Add(panelDetalle);

            // CurrentCellChanged y no SelectionChanged: este último salta antes de que la fila actual cambie.
            dgvAuditoria.CurrentCellChanged += (s, e) => MostrarDetalle();
            dgvAuditoria.DataSourceChanged += (s, e) => MostrarDetalle();
        }

        /// <summary>Muestra, debajo de la lista, el detalle del movimiento elegido.</summary>
        private void MostrarDetalle()
        {
            var fila = dgvAuditoria.CurrentRow == null ? null : dgvAuditoria.CurrentRow.DataBoundItem as DataRowView;
            if (fila == null)
            {
                lblDetalleTitulo.Text = "Detalle";
                lblDetalleResumen.Text = "Elegí un movimiento de la lista para ver qué cambió.";
                dgvDetalle.Rows.Clear();
                dgvDetalle.Visible = false;
                txtDetalle.Visible = false;
                return;
            }

            lblDetalleTitulo.Text = Convert.ToString(fila["Resumen"]);
            lblDetalleResumen.Text = string.Format("{0:dd/MM/yyyy HH:mm:ss}  ·  {1}  ·  {2} · {3}",
                fila["Fecha"], fila["Usuario"], fila["Modulo"], fila["Accion"]);

            DetalleAuditoria detalle = NegocioAuditoria.Detalle(
                Convert.ToString(fila["Accion"]), Convert.ToString(fila["Tabla"]),
                Convert.ToString(fila["ValorAnterior"]), Convert.ToString(fila["ValorNuevo"]));

            dgvDetalle.Rows.Clear();

            if (detalle.TieneFilas)
            {
                foreach (FilaDetalle f in detalle.Filas)
                    dgvDetalle.Rows.Add(f.Campo, f.Antes, f.Despues);
                dgvDetalle.ClearSelection();
                dgvDetalle.Visible = true;
                txtDetalle.Visible = false;
                return;
            }

            var texto = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(detalle.TextoAntes))
                texto.AppendLine("ANTES").AppendLine(detalle.TextoAntes).AppendLine();
            if (!string.IsNullOrEmpty(detalle.TextoDespues))
                texto.AppendLine(string.IsNullOrEmpty(detalle.TextoAntes) ? "REGISTRO" : "DESPUÉS").Append(detalle.TextoDespues);

            txtDetalle.Text = texto.ToString();
            txtDetalle.SelectionStart = 0;
            txtDetalle.Visible = true;
            dgvDetalle.Visible = false;
        }

        private void ucAuditoria_Load(object sender, EventArgs e)
        {
            cboTabla.Items.AddRange(NegocioAuditoria.Modulos().ToArray());
            LimpiarFiltros();          // deja los filtros en su estado inicial y carga la grilla
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
