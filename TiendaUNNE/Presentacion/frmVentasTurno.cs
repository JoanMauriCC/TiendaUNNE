using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Lista de las ventas del turno abierto, desde donde un Administrador o un Supervisor puede
    /// anular una. Solo muestra y pide el motivo: qué se puede anular y cómo lo resuelve NegocioAnulacion.
    /// </summary>
    public class frmVentasTurno : Form
    {
        private static readonly Color Gris = Color.FromArgb(120, 125, 135);

        private readonly CajaSesion _sesion;
        private readonly DataGridView dgv = new DataGridView();
        private readonly Button btnAnular = new Button();
        private readonly Label lblAyuda = new Label();
        private readonly Label lblResumen = new Label();

        /// <summary>True si se anuló alguna venta mientras la ventana estuvo abierta (cambió el stock).</summary>
        public bool HuboAnulaciones { get; private set; }

        public frmVentasTurno(CajaSesion sesion)
        {
            _sesion = sesion;

            Text = "Ventas del turno";
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(860, 480);
            MinimumSize = new Size(700, 380);
            Font = new Font("Segoe UI", 9F);

            var encabezado = new Panel { Dock = DockStyle.Top, Height = 62, Padding = new Padding(16, 10, 16, 0) };
            encabezado.Controls.Add(new Label
            {
                Text = "Ventas del turno",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(16, 8)
            });
            lblResumen.ForeColor = Gris;
            lblResumen.AutoSize = true;
            lblResumen.Location = new Point(18, 38);
            encabezado.Controls.Add(lblResumen);

            var barra = new Panel { Dock = DockStyle.Bottom, Height = 58 };
            btnAnular.Text = "Anular venta…";
            btnAnular.Location = new Point(16, 12);
            btnAnular.Size = new Size(160, 34);
            btnAnular.FlatStyle = FlatStyle.Flat;
            btnAnular.FlatAppearance.BorderSize = 0;
            btnAnular.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnAnular.UseVisualStyleBackColor = false;
            btnAnular.EnabledChanged += (s, e) => PintarBoton();
            btnAnular.Click += btnAnular_Click;
            barra.Controls.Add(btnAnular);

            lblAyuda.ForeColor = Gris;
            lblAyuda.Location = new Point(188, 20);
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

            Controls.Add(dgv);
            Controls.Add(barra);
            Controls.Add(encabezado);

            Load += (s, e) => CargarVentas();
        }

        private void CargarVentas()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                DataTable dt = NegocioAnulacion.ListarDelTurno(_sesion);
                dgv.DataSource = dt;

                Columna("IdVenta", null, 0, false);
                Columna("Fecha", "Fecha", 90, true, "dd/MM HH:mm");
                Columna("Comprobante", "Comprobante", 90, true);
                Columna("Numero", "Número", 105, true);
                Columna("Cliente", "Cliente", 160, true);
                Columna("Total", "Total", 80, true, "N2", DataGridViewContentAlignment.MiddleRight);
                Columna("Estado", "Estado", 80, true);
                Columna("Motivo", "Motivo de anulación", 190, true);

                int emitidas = 0, anuladas = 0;
                decimal total = 0;
                foreach (DataRow fila in dt.Rows)
                {
                    if (Convert.ToString(fila["Estado"]) == NegocioAnulacion.EstadoAnulada) anuladas++;
                    else { emitidas++; total += (decimal)fila["Total"]; }
                }

                lblResumen.Text = string.Format("{0} · {1} venta{2} emitida{2} por $ {3}{4}",
                    _sesion.NombreCaja, emitidas, emitidas == 1 ? string.Empty : "s",
                    total.ToString(NegocioVenta.FormatoImporte),
                    anuladas > 0 ? " · " + anuladas + " anulada" + (anuladas == 1 ? string.Empty : "s") : string.Empty);

                dgv.ClearSelection();
                dgv.CurrentCell = null;
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Ventas del turno", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo leer la lista de ventas.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
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

        /// <summary>Las ventas anuladas se ven en gris para que no se confundan con las vigentes.</summary>
        private void dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var fila = dgv.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (fila != null && Convert.ToString(fila["Estado"]) == NegocioAnulacion.EstadoAnulada)
                e.CellStyle.ForeColor = Gris;
        }

        private DataRowView VentaElegida()
        {
            return dgv.CurrentRow == null ? null : dgv.CurrentRow.DataBoundItem as DataRowView;
        }

        private void ActualizarBoton()
        {
            DataRowView v = VentaElegida();

            if (!NegocioAnulacion.PuedeAnular)
            {
                btnAnular.Enabled = false;
                lblAyuda.Text = "Solo un Administrador o un Supervisor puede anular ventas.";
            }
            else if (v == null)
            {
                btnAnular.Enabled = false;
                lblAyuda.Text = "Elegí una venta de la lista para anularla.";
            }
            else if (Convert.ToString(v["Estado"]) == NegocioAnulacion.EstadoAnulada)
            {
                btnAnular.Enabled = false;
                lblAyuda.Text = "Esa venta ya está anulada.";
            }
            else
            {
                btnAnular.Enabled = true;
                lblAyuda.Text = string.Empty;
            }

            PintarBoton();
        }

        // Un botón plano deshabilitado no se ve "apagado" por sí solo: se le cambia el color.
        private void PintarBoton()
        {
            btnAnular.BackColor = btnAnular.Enabled ? Color.FromArgb(185, 28, 28) : Color.FromArgb(214, 216, 220);
            btnAnular.ForeColor = btnAnular.Enabled ? Color.White : Color.FromArgb(120, 125, 135);
        }

        private void btnAnular_Click(object sender, EventArgs e)
        {
            DataRowView v = VentaElegida();
            if (v == null) return;

            int idVenta = (int)v["IdVenta"];
            string descripcion = string.Format("{0} {1} · $ {2}",
                v["Comprobante"], v["Numero"], ((decimal)v["Total"]).ToString(NegocioVenta.FormatoImporte));

            string motivo;
            using (var pedido = new frmMotivoAnulacion(descripcion))
            {
                if (pedido.ShowDialog(this) != DialogResult.OK) return;
                motivo = pedido.Motivo;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                NegocioAnulacion.Anular(idVenta, motivo);
                HuboAnulaciones = true;
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo anular", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo anular la venta.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            CargarVentas();
        }
    }
}
