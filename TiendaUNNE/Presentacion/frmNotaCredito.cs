using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Emisión de una nota de crédito sobre una venta: se eligen los productos y las cantidades a
    /// devolver, el medio de pago con el que sale el dinero y el motivo. Solo muestra y pide; las
    /// reglas (permisos, cantidades, caja abierta, IVA, stock) las resuelve NegocioNotaCredito.
    /// </summary>
    public class frmNotaCredito : Form
    {
        private static readonly Color Gris = Color.FromArgb(120, 125, 135);
        private static readonly Color Rojo = Color.FromArgb(180, 35, 24);

        private readonly VentaADevolver _venta;
        private readonly DataGridView dgv = new DataGridView();
        private readonly ComboBox cboMedio = new ComboBox();
        private readonly TextBox txtMotivo = new TextBox();
        private readonly Label lblTotal = new Label();
        private readonly Button btnEmitir = new Button();
        private readonly Button btnCancelar = new Button();

        private const int ColADevolver = 4;

        /// <summary>La nota emitida, para que quien abrió el formulario sepa que hay que refrescar.</summary>
        public ComprobanteVista Emitida { get; private set; }

        public frmNotaCredito(VentaADevolver venta)
        {
            _venta = venta ?? throw new ArgumentNullException(nameof(venta));

            string nombreNota = NegocioNotaCredito.NombreDeLaNota(venta);

            Text = nombreNota;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(820, 560);
            MinimumSize = new Size(760, 520);
            Font = new Font("Segoe UI", 9F);

            // ---- Encabezado
            var encabezado = new Panel { Dock = DockStyle.Top, Height = 78 };
            encabezado.Controls.Add(new Label
            {
                Text = nombreNota,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(16, 8)
            });
            encabezado.Controls.Add(new Label
            {
                Text = "Corrige: " + venta.NombreTipo + " " + NegocioComprobante.FormatearNumero(venta.PuntoVenta, venta.Numero) +
                       "  ·  " + venta.Fecha.ToString("dd/MM/yyyy HH:mm") +
                       "  ·  Cliente: " + (venta.ClienteNombre ?? NegocioCliente.ConsumidorFinal),
                ForeColor = Gris,
                AutoSize = true,
                Location = new Point(18, 40)
            });
            encabezado.Controls.Add(new Label
            {
                Text = "Poné en «A devolver» lo que vuelve. La venta original no se toca: esta nota la compensa.",
                ForeColor = Gris,
                AutoSize = true,
                Location = new Point(18, 58)
            });

            // ---- Parte de abajo: medio de pago, motivo, total y botones
            var pie = new Panel { Dock = DockStyle.Bottom, Height = 150 };

            pie.Controls.Add(new Label { Text = "Devolver el dinero por:", AutoSize = true, Location = new Point(16, 17) });
            cboMedio.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMedio.Location = new Point(190, 13);
            cboMedio.Size = new Size(220, 24);
            pie.Controls.Add(cboMedio);

            lblTotal.AutoSize = false;
            lblTotal.TextAlign = ContentAlignment.MiddleRight;
            lblTotal.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTotal.Size = new Size(300, 28);
            // Lo de la derecha se acomoda en Reubicar(): el anclaje a la derecha lo dejaba fuera de pantalla.
            lblTotal.Location = new Point(0, 11);
            pie.Controls.Add(lblTotal);

            pie.Controls.Add(new Label { Text = "Motivo:", AutoSize = true, Location = new Point(16, 55) });
            txtMotivo.Location = new Point(190, 51);
            txtMotivo.Size = new Size(400, 24);
            txtMotivo.MaxLength = NegocioNotaCredito.LargoMaximoMotivo;
            pie.Controls.Add(txtMotivo);

            pie.Controls.Add(new Label
            {
                Text = "Obligatorio (mínimo " + NegocioNotaCredito.LargoMinimoMotivo + " letras). Queda en el comprobante y en la Auditoría.",
                ForeColor = Gris,
                AutoSize = true,
                Location = new Point(190, 79)
            });

            var btnTodo = new Button { Text = "Devolver todo", Location = new Point(16, 104), Size = new Size(130, 34) };
            btnTodo.Click += (s, e) => DevolverTodo();
            pie.Controls.Add(btnTodo);

            btnCancelar.Text = "Cancelar";
            btnCancelar.Size = new Size(100, 34);
            btnCancelar.Location = new Point(0, 104);
            btnCancelar.DialogResult = DialogResult.Cancel;
            pie.Controls.Add(btnCancelar);
            CancelButton = btnCancelar;

            btnEmitir.Text = "Emitir nota de crédito";
            btnEmitir.Size = new Size(200, 34);
            btnEmitir.Location = new Point(0, 104);
            btnEmitir.FlatStyle = FlatStyle.Flat;
            btnEmitir.FlatAppearance.BorderSize = 0;
            btnEmitir.BackColor = Color.FromArgb(47, 111, 237);
            btnEmitir.ForeColor = Color.White;
            btnEmitir.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnEmitir.UseVisualStyleBackColor = false;
            btnEmitir.Click += (s, e) => Emitir();
            pie.Controls.Add(btnEmitir);

            // ---- Renglones
            dgv.Dock = DockStyle.Fill;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.RowHeadersVisible = false;
            dgv.MultiSelect = false;
            dgv.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgv.EditMode = DataGridViewEditMode.EditOnEnter;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;

            AgregarColumna("Producto", "Producto", 220, false, DataGridViewContentAlignment.MiddleLeft);
            AgregarColumna("Vendido", "Vendido", 70, false, DataGridViewContentAlignment.MiddleRight);
            AgregarColumna("Devuelto", "Ya devuelto", 80, false, DataGridViewContentAlignment.MiddleRight);
            AgregarColumna("Disponible", "Se puede devolver", 110, false, DataGridViewContentAlignment.MiddleRight);
            AgregarColumna("ADevolver", "A devolver", 90, true, DataGridViewContentAlignment.MiddleRight);
            AgregarColumna("Importe", "Importe", 100, false, DataGridViewContentAlignment.MiddleRight);
            dgv.Columns[ColADevolver].DefaultCellStyle.BackColor = Color.FromArgb(234, 242, 251);
            dgv.Columns[ColADevolver].HeaderCell.Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            dgv.CellEndEdit += dgv_CellEndEdit;
            dgv.CellFormatting += dgv_CellFormatting;

            Controls.Add(dgv);
            Controls.Add(pie);
            Controls.Add(encabezado);

            CargarMedios();
            CargarRenglones();
            ActualizarTotal();

            Load += (s, e) => Reubicar();
            Resize += (s, e) => Reubicar();
        }

        /// <summary>Pone lo que va pegado al borde derecho según el ancho real de la ventana.</summary>
        private void Reubicar()
        {
            int ancho = ClientSize.Width;
            lblTotal.Left = ancho - 16 - lblTotal.Width;
            btnEmitir.Left = ancho - 16 - btnEmitir.Width;
            btnCancelar.Left = btnEmitir.Left - 12 - btnCancelar.Width;
            txtMotivo.Width = Math.Max(200, ancho - txtMotivo.Left - 16);
        }

        private void AgregarColumna(string nombre, string titulo, float peso, bool editable, DataGridViewContentAlignment alineacion)
        {
            int i = dgv.Columns.Add(nombre, titulo);
            DataGridViewColumn c = dgv.Columns[i];
            c.FillWeight = peso;
            c.ReadOnly = !editable;
            c.SortMode = DataGridViewColumnSortMode.NotSortable;
            c.DefaultCellStyle.Alignment = alineacion;
        }

        private void CargarMedios()
        {
            foreach (MedioPagoItem m in NegocioVenta.ListarMediosDePago())
                cboMedio.Items.Add(m);

            MedioPagoItem sugerido = cboMedio.Items.Cast<MedioPagoItem>()
                .FirstOrDefault(m => m.Id == _venta.IdMedioPagoSugerido);

            if (sugerido != null) cboMedio.SelectedItem = sugerido;
            else if (cboMedio.Items.Count > 0) cboMedio.SelectedIndex = 0;
        }

        private void CargarRenglones()
        {
            foreach (LineaDevolvible r in _venta.Renglones)
            {
                int i = dgv.Rows.Add(
                    r.Descripcion,
                    Cantidad(r.Vendida),
                    Cantidad(r.Devuelta),
                    Cantidad(r.Disponible),
                    Cantidad(0),
                    Importe(0));

                dgv.Rows[i].Tag = r;
                // Lo que ya se devolvió por completo no se puede tocar.
                dgv.Rows[i].Cells[ColADevolver].ReadOnly = r.Disponible <= 0;
            }

            if (dgv.Rows.Count > 0)
            {
                int primera = _venta.Renglones.FindIndex(r => r.Disponible > 0);
                if (primera >= 0) dgv.CurrentCell = dgv.Rows[primera].Cells[ColADevolver];
            }
        }

        private static string Cantidad(decimal valor) => valor.ToString(NegocioVenta.FormatoCantidadTicket);
        private static string Importe(decimal valor) => valor.ToString(NegocioVenta.FormatoImporte);

        private void dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var r = dgv.Rows[e.RowIndex].Tag as LineaDevolvible;
            if (r != null && r.Disponible <= 0)
            {
                e.CellStyle.ForeColor = Gris;
                e.CellStyle.BackColor = Color.FromArgb(244, 245, 247);
            }
        }

        // Lo escrito en «A devolver» se interpreta y se corrige al salir de la celda:
        // vacío o inválido = 0; más de lo disponible = lo disponible, con aviso.
        private void dgv_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != ColADevolver) return;

            var linea = dgv.Rows[e.RowIndex].Tag as LineaDevolvible;
            if (linea == null) return;

            string texto = Convert.ToString(dgv.Rows[e.RowIndex].Cells[ColADevolver].Value) ?? string.Empty;
            decimal valor;
            if (!decimal.TryParse(texto.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out valor) || valor < 0)
                valor = 0;

            if (valor > linea.Disponible)
            {
                MessageBox.Show(string.Format(CultureInfo.CurrentCulture,
                        "De «{0}» solo se puede devolver {1}.", linea.Descripcion, Cantidad(linea.Disponible)),
                    "Nota de crédito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                valor = linea.Disponible;
            }

            Poner(e.RowIndex, valor);
            ActualizarTotal();
        }

        private void Poner(int fila, decimal valor)
        {
            var linea = (LineaDevolvible)dgv.Rows[fila].Tag;
            linea.ADevolver = valor;
            dgv.Rows[fila].Cells[ColADevolver].Value = Cantidad(valor);
            dgv.Rows[fila].Cells["Importe"].Value = Importe(decimal.Round(valor * linea.PrecioUnitario, 2));
        }

        private void DevolverTodo()
        {
            dgv.EndEdit();
            for (int i = 0; i < dgv.Rows.Count; i++)
                Poner(i, ((LineaDevolvible)dgv.Rows[i].Tag).Disponible);
            ActualizarTotal();
        }

        private void ActualizarTotal()
        {
            decimal total = NegocioNotaCredito.TotalADevolver(_venta);
            lblTotal.Text = "Total a devolver:  $ " + Importe(total);
            lblTotal.ForeColor = total > 0 ? Rojo : Gris;
        }

        private void Emitir()
        {
            dgv.EndEdit();

            var medio = cboMedio.SelectedItem as MedioPagoItem;
            decimal total = NegocioNotaCredito.TotalADevolver(_venta);

            if (total > 0 && medio != null)
            {
                string pregunta = string.Format(CultureInfo.CurrentCulture,
                    "Se va a emitir una {0} por $ {1}, devueltos por {2}, y el stock vuelve al depósito.\n\nEsto no se puede deshacer. ¿Confirmás?",
                    NegocioNotaCredito.NombreDeLaNota(_venta), Importe(total), medio.Nombre);

                if (MessageBox.Show(pregunta, "Emitir nota de crédito",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                    return;
            }

            ComprobanteVista nota;
            try
            {
                Cursor = Cursors.WaitCursor;
                nota = NegocioNotaCredito.Emitir(_venta, medio == null ? 0 : medio.Id, txtMotivo.Text);
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Nota de crédito", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo emitir la nota de crédito. No se registró nada.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            Emitida = nota;

            using (var ventana = new frmComprobante(nota))
                ventana.ShowDialog(this);

            DialogResult = DialogResult.OK;
        }
    }
}
