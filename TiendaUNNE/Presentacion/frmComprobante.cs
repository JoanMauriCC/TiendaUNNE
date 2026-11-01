using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Muestra el comprobante recién emitido, con el aspecto de un ticket, y permite guardarlo en PDF.
    /// No calcula nada: todo viene armado en ComprobanteVista.
    /// </summary>
    public class frmComprobante : Form
    {
        private const int Ancho = 46;   // caracteres por línea del ticket

        private readonly ComprobanteVista _comprobante;

        public frmComprobante(ComprobanteVista comprobante)
        {
            _comprobante = comprobante ?? throw new ArgumentNullException(nameof(comprobante));

            Text = comprobante.Nombre + " " + comprobante.NumeroFormateado;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(500, 660);
            Font = new Font("Segoe UI", 9F);
            BackColor = Color.FromArgb(244, 246, 249);

            var barra = new Panel { Dock = DockStyle.Bottom, Height = 58, BackColor = Color.White };

            var btnPdf = new Button
            {
                Text = "Guardar PDF…",
                Location = new Point(16, 12),
                Size = new Size(150, 34)
            };
            btnPdf.Click += btnPdf_Click;
            barra.Controls.Add(btnPdf);

            var btnCerrar = new Button
            {
                Text = "Listo",
                Location = new Point(ClientSize.Width - 16 - 120, 12),
                Size = new Size(120, 34),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(47, 111, 237),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                DialogResult = DialogResult.OK,
                UseVisualStyleBackColor = false
            };
            btnCerrar.FlatAppearance.BorderSize = 0;
            barra.Controls.Add(btnCerrar);

            var papel = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                Font = new Font("Consolas", 9.5F),
                Text = ArmarTexto(comprobante),
                TabStop = false,
                WordWrap = false
            };

            var marco = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };
            marco.Controls.Add(papel);

            Controls.Add(marco);
            Controls.Add(barra);

            AcceptButton = btnCerrar;
            CancelButton = btnCerrar;
        }

        private void btnPdf_Click(object sender, EventArgs e)
        {
            string ruta;
            using (var dialogo = new SaveFileDialog
            {
                Title = "Guardar comprobante en PDF",
                Filter = "Documento PDF (*.pdf)|*.pdf",
                FileName = ExportadorPdfComprobante.NombreSugerido(_comprobante)
            })
            {
                if (dialogo.ShowDialog(this) != DialogResult.OK) return;
                ruta = dialogo.FileName;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                ExportadorPdfComprobante.Guardar(_comprobante, ruta);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                MessageBox.Show("No se pudo guardar el PDF. Si el archivo está abierto en otro programa, " +
                                "cerralo; si no, probá guardarlo en otra carpeta.",
                    "Guardar PDF", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo generar el PDF.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            if (MessageBox.Show("El comprobante se guardó en PDF.\n\n¿Querés abrirlo?", "Guardar PDF",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                try { Process.Start(ruta); }
                catch (Exception)
                {
                    MessageBox.Show("No hay ningún programa para abrir PDF. El archivo quedó en:\n" + ruta,
                        "Guardar PDF", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // -----------------------------------------------------------------
        // El ticket en texto de ancho fijo
        // -----------------------------------------------------------------

        private static string ArmarTexto(ComprobanteVista v)
        {
            var sb = new StringBuilder();
            string raya = new string('-', Ancho);

            sb.AppendLine(Centrar("TiendaUNNE"));
            sb.AppendLine(Centrar(v.Nombre.ToUpperInvariant()));
            sb.AppendLine(Centrar("Nº " + v.NumeroFormateado));
            sb.AppendLine();
            sb.AppendLine("Fecha:   " + v.FechaHora.ToString("dd/MM/yyyy HH:mm"));
            if (!string.IsNullOrWhiteSpace(v.Cajero))
                sb.AppendLine("Atendió: " + Recortar(v.Cajero, Ancho - 9));
            sb.AppendLine("Cliente: " + Recortar(v.ClienteNombre, Ancho - 9));
            sb.AppendLine("         " + Recortar(v.ClienteDocumento + " · " + v.ClienteCondicionIva, Ancho - 9));
            sb.AppendLine(raya);

            foreach (ComprobanteRenglon r in v.Renglones)
            {
                sb.AppendLine(Recortar(r.Descripcion, Ancho));
                string cuenta = "  " + r.Cantidad.ToString(NegocioVenta.FormatoCantidadTicket) +
                                " x " + r.PrecioUnitario.ToString(NegocioVenta.FormatoImporte);
                sb.AppendLine(Dos(cuenta, r.Subtotal.ToString(NegocioVenta.FormatoImporte)));
            }

            sb.AppendLine(raya);

            if (v.DiscriminaIva)
            {
                sb.AppendLine(Dos("Neto gravado", v.ImporteNeto.ToString(NegocioVenta.FormatoImporte)));
                sb.AppendLine(Dos("IVA " + NegocioComprobante.AlicuotaIva.ToString("0.##") + " %",
                    v.ImporteIva.ToString(NegocioVenta.FormatoImporte)));
            }

            sb.AppendLine(Dos("TOTAL  $", v.Total.ToString(NegocioVenta.FormatoImporte)));
            sb.AppendLine(raya);

            foreach (ComprobantePago p in v.Pagos)
                sb.AppendLine(Dos(Recortar(p.Medio, Ancho - 14), p.Importe.ToString(NegocioVenta.FormatoImporte)));

            if (v.Vuelto > 0)
                sb.AppendLine(Dos("VUELTO", v.Vuelto.ToString(NegocioVenta.FormatoImporte)));

            sb.AppendLine();
            sb.AppendLine(Centrar(v.Codigo == NegocioComprobante.Ticket
                ? "Ticket interno, no válido como factura"
                : "Comprobante interno, sin CAE de ARCA"));
            sb.AppendLine(Centrar("¡Gracias por tu compra!"));

            return sb.ToString();
        }

        private static string Centrar(string texto)
        {
            if (texto.Length >= Ancho) return texto;
            return new string(' ', (Ancho - texto.Length) / 2) + texto;
        }

        /// <summary>Texto a la izquierda y valor a la derecha, en la misma línea.</summary>
        private static string Dos(string izquierda, string derecha)
        {
            int espacio = Math.Max(1, Ancho - izquierda.Length - derecha.Length);
            return izquierda + new string(' ', espacio) + derecha;
        }

        private static string Recortar(string texto, int maximo)
        {
            if (texto == null) return string.Empty;
            return texto.Length <= maximo ? texto : texto.Substring(0, maximo - 1) + "…";
        }
    }
}
