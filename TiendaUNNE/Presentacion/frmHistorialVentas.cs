using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Historial de ventas: se elige el período, se selecciona una venta y se vuelve a abrir su
    /// comprobante para guardarlo otra vez en PDF (por si el cliente lo perdió). Solo muestra y
    /// pide; qué se lista y cómo se arma el comprobante lo resuelve NegocioHistorial.
    /// </summary>
    public class frmHistorialVentas : Form
    {
        private static readonly Color Gris = Color.FromArgb(120, 125, 135);
        private static readonly Color Acento = Color.FromArgb(47, 111, 237);

        private readonly CajaSesion _sesion;
        private readonly DataGridView dgv = new DataGridView();
        private readonly ComboBox cboRango = new ComboBox();
        private readonly Button btnReimprimir = new Button();
        private readonly Label lblAyuda = new Label();
        private readonly Label lblResumen = new Label();

        private bool _cargando;

        public frmHistorialVentas(CajaSesion sesion)
        {
            _sesion = sesion;

            Text = "Historial de ventas";
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(880, 500);
            MinimumSize = new Size(720, 380);
            Font = new Font("Segoe UI", 9F);

            // ---- Encabezado: título, resumen y selector de período
            var encabezado = new Panel { Dock = DockStyle.Top, Height = 66 };
            encabezado.Controls.Add(new Label
            {
                Text = "Historial de ventas",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(16, 8)
            });
            lblResumen.ForeColor = Gris;
            lblResumen.AutoSize = true;
            lblResumen.Location = new Point(18, 40);
            encabezado.Controls.Add(lblResumen);

            var lblMostrar = new Label
            {
                Text = "Mostrar:",
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(ClientSize.Width - 16 - 200 - 58, 14)
            };
            encabezado.Controls.Add(lblMostrar);

            cboRango.DropDownStyle = ComboBoxStyle.DropDownList;
            cboRango.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cboRango.Location = new Point(ClientSize.Width - 16 - 200, 10);
            cboRango.Size = new Size(200, 24);
            foreach (RangoHistorial r in Enum.GetValues(typeof(RangoHistorial)))
                cboRango.Items.Add(new OpcionRango(r));
            cboRango.SelectedIndex = 0;
            cboRango.SelectedIndexChanged += (s, e) => CargarVentas();
            encabezado.Controls.Add(cboRango);

            // ---- Barra de abajo: reimprimir y cerrar
            var barra = new Panel { Dock = DockStyle.Bottom, Height = 58 };

            btnReimprimir.Text = "Reimprimir ticket…";
            btnReimprimir.Location = new Point(16, 12);
            btnReimprimir.Size = new Size(170, 34);
            btnReimprimir.FlatStyle = FlatStyle.Flat;
            btnReimprimir.FlatAppearance.BorderSize = 0;
            btnReimprimir.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnReimprimir.UseVisualStyleBackColor = false;
            btnReimprimir.EnabledChanged += (s, e) => PintarBoton();
            btnReimprimir.Click += (s, e) => Reimprimir();
            barra.Controls.Add(btnReimprimir);

            lblAyuda.ForeColor = Gris;
            lblAyuda.Location = new Point(198, 20);
            lblAyuda.Size = new Size(520, 20);
            lblAyuda.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            barra.Controls.Add(lblAyuda);

            var btnCerrar = new Button
            {
                Text = "Cerrar",
                Size = new Size(100, 34),
                Location = new Point(ClientSize.Width - 16 - 100, 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                DialogResult = DialogResult.Cancel
            };
            barra.Controls.Add(btnCerrar);
            CancelButton = btnCerrar;

            // ---- Lista
            dgv.Dock = DockStyle.Fill;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.RowHeadersVisible = false;
            dgv.MultiSelect = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.SelectionChanged += (s, e) => ActualizarBoton();
            dgv.CellFormatting += dgv_CellFormatting;
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) Reimprimir(); };

            Controls.Add(dgv);
            Controls.Add(barra);
            Controls.Add(encabezado);

            Load += (s, e) => CargarVentas();
        }

        /// <summary>Una opción del selector de período, con el texto que define Negocio.</summary>
        private sealed class OpcionRango
        {
            public OpcionRango(RangoHistorial rango) { Rango = rango; }
            public RangoHistorial Rango { get; }
            public override string ToString() => NegocioHistorial.NombreRango(Rango);
        }

        private RangoHistorial RangoElegido
        {
            get
            {
                var o = cboRango.SelectedItem as OpcionRango;
                return o == null ? RangoHistorial.TurnoActual : o.Rango;
            }
        }

        private void CargarVentas()
        {
            if (_cargando) return;
            _cargando = true;

            try
            {
                Cursor = Cursors.WaitCursor;
                DataTable dt = NegocioHistorial.Listar(RangoElegido, _sesion);
                dgv.DataSource = dt;

                Columna("IdVenta", null, 0, false);
                Columna("Fecha", "Fecha", 90, true, "dd/MM HH:mm");
                Columna("Comprobante", "Comprobante", 95, true);
                Columna("Numero", "Número", 110, true);
                Columna("Cliente", "Cliente", 190, true);
                Columna("Total", "Total", 90, true, "N2", DataGridViewContentAlignment.MiddleRight);
                Columna("Estado", "Estado", 85, true);

                int emitidas = 0, anuladas = 0;
                decimal total = 0;
                foreach (DataRow fila in dt.Rows)
                {
                    if (Convert.ToString(fila["Estado"]) == NegocioHistorial.EstadoAnulada) anuladas++;
                    else { emitidas++; total += (decimal)fila["Total"]; }
                }

                lblResumen.Text = string.Format("{0} venta{1} emitida{1} por $ {2}{3}",
                    emitidas, emitidas == 1 ? string.Empty : "s",
                    total.ToString(NegocioVenta.FormatoImporte),
                    anuladas > 0 ? " · " + anuladas + " anulada" + (anuladas == 1 ? string.Empty : "s") : string.Empty);

                dgv.ClearSelection();
                dgv.CurrentCell = null;
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Historial de ventas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo leer el historial de ventas.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                _cargando = false;
            }

            ActualizarBoton();
        }

        private void Columna(string nombre, string titulo, float peso, bool visible,
                             string formato = null, DataGridViewContentAlignment? alineacion = null)
        {
            if (!dgv.Columns.Contains(nombre)) return;

            DataGridViewColumn c = dgv.Columns[nombre];
            c.Visible = visible;
            if (titulo != null) c.HeaderText = titulo;
            if (peso > 0) c.FillWeight = peso;
            if (formato != null) c.DefaultCellStyle.Format = formato;
            if (alineacion.HasValue) c.DefaultCellStyle.Alignment = alineacion.Value;
        }

        /// <summary>Las ventas anuladas (de antes) se ven en gris para no confundirlas con las vigentes.</summary>
        private void dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var fila = dgv.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (fila != null && Convert.ToString(fila["Estado"]) == NegocioHistorial.EstadoAnulada)
                e.CellStyle.ForeColor = Gris;
        }

        private DataRowView VentaElegida()
        {
            return dgv.CurrentRow == null ? null : dgv.CurrentRow.DataBoundItem as DataRowView;
        }

        private void ActualizarBoton()
        {
            bool hay = VentaElegida() != null;
            btnReimprimir.Enabled = hay;
            lblAyuda.Text = hay
                ? "Se abre el comprobante para guardarlo de nuevo en PDF. También con doble clic."
                : "Elegí una venta de la lista para reimprimir su ticket.";
            PintarBoton();
        }

        // Un botón plano deshabilitado no se ve "apagado" por sí solo: se le cambia el color.
        private void PintarBoton()
        {
            btnReimprimir.BackColor = btnReimprimir.Enabled ? Acento : Color.FromArgb(214, 216, 220);
            btnReimprimir.ForeColor = btnReimprimir.Enabled ? Color.White : Color.FromArgb(120, 125, 135);
        }

        private void Reimprimir()
        {
            DataRowView v = VentaElegida();
            if (v == null) return;

            ComprobanteVista comprobante;
            try
            {
                Cursor = Cursors.WaitCursor;
                comprobante = NegocioHistorial.ObtenerComprobante((int)v["IdVenta"]);
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Reimprimir", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo abrir el comprobante.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            using (var ventana = new frmComprobante(comprobante))
                ventana.ShowDialog(this);
        }
    }
}
