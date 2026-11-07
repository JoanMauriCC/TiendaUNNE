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
        private readonly Button btnNota = new Button();
        private readonly Button btnCerrar = new Button();
        private readonly Label lblMostrar = new Label();
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

            lblMostrar.Text = "Mostrar:";
            lblMostrar.AutoSize = true;
            lblMostrar.Location = new Point(0, 14);
            encabezado.Controls.Add(lblMostrar);

            // Lo de la derecha se acomoda en Reubicar(): el anclaje a la derecha lo dejaba fuera de pantalla.
            cboRango.DropDownStyle = ComboBoxStyle.DropDownList;
            cboRango.Location = new Point(0, 10);
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

            // Solo el Administrador y el Supervisor ven el botón: devuelven plata y stock.
            btnNota.Text = "Nota de crédito…";
            btnNota.Location = new Point(196, 12);
            btnNota.Size = new Size(160, 34);
            btnNota.FlatStyle = FlatStyle.Flat;
            btnNota.FlatAppearance.BorderSize = 0;
            btnNota.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnNota.UseVisualStyleBackColor = false;
            btnNota.Visible = NegocioHistorial.PuedeEmitirNotaCredito;
            btnNota.EnabledChanged += (s, e) => PintarBoton();
            btnNota.Click += (s, e) => EmitirNotaCredito();
            barra.Controls.Add(btnNota);

            lblAyuda.ForeColor = Gris;
            lblAyuda.Location = new Point(btnNota.Visible ? 368 : 198, 20);
            lblAyuda.Size = new Size(btnNota.Visible ? 350 : 520, 20);
            barra.Controls.Add(lblAyuda);

            btnCerrar.Text = "Cerrar";
            btnCerrar.Size = new Size(100, 34);
            btnCerrar.Location = new Point(0, 12);
            btnCerrar.DialogResult = DialogResult.Cancel;
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

            Load += (s, e) => { Reubicar(); CargarVentas(); };
            Resize += (s, e) => Reubicar();
        }

        /// <summary>Pone lo que va pegado al borde derecho según el ancho real de la ventana.</summary>
        private void Reubicar()
        {
            int ancho = ClientSize.Width;
            cboRango.Left = ancho - 16 - cboRango.Width;
            lblMostrar.Left = cboRango.Left - 8 - lblMostrar.Width;
            btnCerrar.Left = ancho - 16 - btnCerrar.Width;
            lblAyuda.Width = Math.Max(100, btnCerrar.Left - lblAyuda.Left - 12);
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
                Columna("Signo", null, 0, false);
                Columna("Fecha", "Fecha", 90, true, "dd/MM HH:mm");
                Columna("Comprobante", "Comprobante", 130, true);
                Columna("Numero", "Número", 110, true);
                Columna("Cliente", "Cliente", 160, true);
                Columna("Total", "Total", 90, true, "N2", DataGridViewContentAlignment.MiddleRight);
                Columna("Estado", "Estado", 85, true);

                int ventas = 0, notas = 0, anuladas = 0;
                decimal vendido = 0, devuelto = 0;
                foreach (DataRow fila in dt.Rows)
                {
                    if (Convert.ToString(fila["Estado"]) == NegocioHistorial.EstadoAnulada) { anuladas++; continue; }

                    decimal importe = (decimal)fila["Total"];   // las notas de crédito ya vienen en negativo
                    if ((int)fila["Signo"] < 0) { notas++; devuelto += importe; }
                    else { ventas++; vendido += importe; }
                }

                string texto = string.Format("{0} venta{1} por $ {2}",
                    ventas, ventas == 1 ? string.Empty : "s", vendido.ToString(NegocioVenta.FormatoImporte));
                if (notas > 0)
                    texto += string.Format(" · {0} nota{1} de crédito por $ {2} · Neto $ {3}",
                        notas, notas == 1 ? string.Empty : "s",
                        Math.Abs(devuelto).ToString(NegocioVenta.FormatoImporte),
                        (vendido + devuelto).ToString(NegocioVenta.FormatoImporte));
                if (anuladas > 0)
                    texto += " · " + anuladas + " anulada" + (anuladas == 1 ? string.Empty : "s");
                lblResumen.Text = texto;

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
            if (fila == null) return;

            if (Convert.ToString(fila["Estado"]) == NegocioHistorial.EstadoAnulada)
                e.CellStyle.ForeColor = Gris;
            else if ((int)fila["Signo"] < 0)
                e.CellStyle.ForeColor = Color.FromArgb(180, 35, 24);   // las notas de crédito, en rojo
        }

        private DataRowView VentaElegida()
        {
            return dgv.CurrentRow == null ? null : dgv.CurrentRow.DataBoundItem as DataRowView;
        }

        private void ActualizarBoton()
        {
            DataRowView elegida = VentaElegida();
            bool hay = elegida != null;
            bool esVenta = hay && (int)elegida["Signo"] > 0
                           && Convert.ToString(elegida["Estado"]) != NegocioHistorial.EstadoAnulada;

            btnReimprimir.Enabled = hay;
            btnNota.Enabled = esVenta;

            if (!hay)
                lblAyuda.Text = "Elegí una venta de la lista para reimprimir su ticket.";
            else if (btnNota.Visible)
                lblAyuda.Text = esVenta
                    ? "Reimprimir guarda el PDF; la nota de crédito devuelve productos."
                    : "Es una nota de crédito: se reimprime, no se corrige.";
            else
                lblAyuda.Text = "Se abre el comprobante para guardarlo de nuevo en PDF. También con doble clic.";
            PintarBoton();
        }

        // Un botón plano deshabilitado no se ve "apagado" por sí solo: se le cambia el color.
        private void PintarBoton()
        {
            Pintar(btnReimprimir);
            Pintar(btnNota);
        }

        private static void Pintar(Button b)
        {
            b.BackColor = b.Enabled ? Acento : Color.FromArgb(214, 216, 220);
            b.ForeColor = b.Enabled ? Color.White : Color.FromArgb(120, 125, 135);
        }

        private void EmitirNotaCredito()
        {
            DataRowView v = VentaElegida();
            if (v == null) return;

            VentaADevolver venta;
            try
            {
                Cursor = Cursors.WaitCursor;
                venta = NegocioNotaCredito.Preparar((int)v["IdVenta"]);
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Nota de crédito", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo abrir la venta.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            using (var ventana = new frmNotaCredito(venta))
            {
                if (ventana.ShowDialog(this) == DialogResult.OK)
                    CargarVentas();
            }
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
