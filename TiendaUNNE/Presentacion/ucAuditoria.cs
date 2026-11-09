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
    public partial class ucAuditoria : UserControl, IPantallaConGuia
    {
        private static readonly Color ColorAntes = Color.FromArgb(180, 35, 24);
        private static readonly Color ColorDespues = Color.FromArgb(21, 128, 61);

        // Movimientos sensibles (notas de crédito, restauraciones, diferencias de caja): fondo ámbar.
        private static readonly Color FondoSensible = Color.FromArgb(254, 243, 199);
        private static readonly Color TextoSensible = Color.FromArgb(120, 53, 15);

        // Panel de detalle, debajo de la lista: se arma por código.
        private readonly Panel panelDetalle = new Panel();
        private readonly Label lblDetalleTitulo = new Label();
        private readonly Label lblDetalleResumen = new Label();
        private readonly DataGridView dgvDetalle = new DataGridView();
        private readonly TextBox txtDetalle = new TextBox();
        private readonly Label lblLeyenda = new Label();

        public ucAuditoria()
        {
            InitializeComponent();
            ArmarDetalle();
        }

        private void ArmarDetalle()
        {
            panelDetalle.Dock = DockStyle.Bottom;
            panelDetalle.Height = 205;
            panelDetalle.Padding = new Padding(0, 6, 0, 0);

            var encabezado = new Panel { Dock = DockStyle.Top, Height = 64 };
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

            // Leyenda de los movimientos resaltados, a la derecha del buscador.
            lblLeyenda.Text = " Movimiento sensible ";
            lblLeyenda.AutoSize = true;
            lblLeyenda.BackColor = FondoSensible;
            lblLeyenda.ForeColor = TextoSensible;
            panelFiltros.Controls.Add(lblLeyenda);

            dgvAuditoria.CellFormatting += dgvAuditoria_CellFormatting;

            // CurrentCellChanged y no SelectionChanged: este último salta antes de que la fila actual cambie.
            dgvAuditoria.CurrentCellChanged += (s, e) => MostrarDetalle();
            dgvAuditoria.DataSourceChanged += (s, e) => MostrarDetalle();
        }

        /// <summary>Los movimientos sensibles se pintan de ámbar (salvo el que está seleccionado, que mantiene el azul).</summary>
        private void dgvAuditoria_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var fila = dgvAuditoria.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (fila == null || string.IsNullOrEmpty(Convert.ToString(fila["Sensible"]))) return;

            e.CellStyle.BackColor = FondoSensible;
            e.CellStyle.ForeColor = TextoSensible;
            e.CellStyle.SelectionBackColor = Color.FromArgb(180, 83, 9);
            e.CellStyle.SelectionForeColor = Color.White;
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
            string sensible = Convert.ToString(fila["Sensible"]);
            lblDetalleResumen.Text = string.Format("{0:dd/MM/yyyy HH:mm:ss}  ·  {1}  ·  {2} · {3}",
                fila["Fecha"], fila["Usuario"], fila["Modulo"], fila["Accion"]) +
                (sensible.Length > 0 ? "\r\nMovimiento sensible: " + sensible : string.Empty);
            lblDetalleResumen.ForeColor = sensible.Length > 0 ? TextoSensible : Color.FromArgb(91, 98, 112);

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
            cboAccion.Items.AddRange(NegocioAuditoria.Acciones().ToArray());
            CargarUsuarios();

            // La leyenda va pegada al buscador, con la escala real de la pantalla.
            lblLeyenda.Location = new Point(txtBuscar.Right + 12, txtBuscar.Top + 3);

            // Los combos filtran al elegir; el buscador, con Enter; las fechas, con Filtrar o con los atajos.
            cboTabla.SelectionChangeCommitted += (s, ev) => CargarGrilla();
            cboUsuario.SelectionChangeCommitted += (s, ev) => CargarGrilla();
            cboAccion.SelectionChangeCommitted += (s, ev) => CargarGrilla();
            txtBuscar.KeyDown += txtBuscar_KeyDown;

            LimpiarFiltros();          // deja los filtros en su estado inicial y carga la grilla
        }

        /// <summary>Llena el combo de usuarios con quienes tienen movimientos; conserva el elegido si sigue estando.</summary>
        private void CargarUsuarios()
        {
            int? elegido = (cboUsuario.SelectedItem as UsuarioAuditoria)?.IdUsuario;

            cboUsuario.Items.Clear();
            cboUsuario.Items.AddRange(NegocioAuditoria.Usuarios().ToArray());

            cboUsuario.SelectedIndex = 0;
            for (int i = 0; i < cboUsuario.Items.Count; i++)
                if (((UsuarioAuditoria)cboUsuario.Items[i]).IdUsuario == elegido && elegido.HasValue)
                    cboUsuario.SelectedIndex = i;
        }

        private void txtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            CargarGrilla();
        }

        /// <summary>Fija «desde hoy menos N días» y «hasta hoy», y filtra.</summary>
        private void AtajoDeFechas(int dias)
        {
            dtpDesde.Value = NegocioAuditoria.DesdeHaceDias(dias);
            dtpHasta.Value = DateTime.Today;
            dtpDesde.Checked = true;
            dtpHasta.Checked = true;
            CargarGrilla();
        }

        private void btnHoy_Click(object sender, EventArgs e) { AtajoDeFechas(1); }
        private void btnSemana_Click(object sender, EventArgs e) { AtajoDeFechas(7); }
        private void btnMes_Click(object sender, EventArgs e) { AtajoDeFechas(30); }

        private void CargarGrilla()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                var filtro = new FiltroAuditoria
                {
                    Tabla = (cboTabla.SelectedItem as ModuloAuditoria)?.Tabla,
                    IdUsuario = (cboUsuario.SelectedItem as UsuarioAuditoria)?.IdUsuario,
                    Accion = (cboAccion.SelectedItem as AccionAuditoria)?.Valor,
                    Desde = dtpDesde.Checked ? dtpDesde.Value.Date : (DateTime?)null,
                    Hasta = dtpHasta.Checked ? dtpHasta.Value.Date : (DateTime?)null,
                    Texto = txtBuscar.Text
                };

                DataTable dt = NegocioAuditoria.Listar(filtro);
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
            foreach (string oculta in new[] { "ValorAnterior", "ValorNuevo", "Tabla", "Sensible" })
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
            cboTabla.SelectedIndex = 0;   // "Todos"
            cboUsuario.SelectedIndex = 0;
            cboAccion.SelectedIndex = 0;
            txtBuscar.Clear();

            // Primero el valor y después el tilde: asignar el valor puede volver a tildar la casilla.
            dtpDesde.Value = NegocioAuditoria.FechaDesdePorDefecto;
            dtpHasta.Value = NegocioAuditoria.FechaHastaPorDefecto;
            dtpDesde.Checked = false;
            dtpHasta.Checked = false;

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
            CargarUsuarios();   // por si alguien nuevo hizo algo desde que se abrió la pantalla
            CargarGrilla();
        }

        // -----------------------------------------------------------------
        // Ayuda: el control de cada parte que señala la guía (NegocioGuia)
        // -----------------------------------------------------------------

        public Control Ancla(string clave)
        {
            switch (clave)
            {
                case AnclaGuia.AuditoriaFiltros: return panelFiltros;
                case AnclaGuia.AuditoriaRapidos: return btnHoy;
                case AnclaGuia.AuditoriaLista: return dgvAuditoria;
                case AnclaGuia.AuditoriaDetalle: return panelDetalle;
                default: return null;
            }
        }

    }
}
