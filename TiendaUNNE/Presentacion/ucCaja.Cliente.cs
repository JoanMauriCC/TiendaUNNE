using System;
using System.Drawing;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Franja "Cliente" del ticket: muestra a quién se le vende y abre la ventana para elegirlo
    /// o cargarlo. Sin cliente elegido la venta es a consumidor final.
    /// </summary>
    public partial class ucCaja
    {
        private Panel panelCliente;
        private Label lblCliente;
        private Label lblClienteDetalle;
        private Button btnCliente;
        private ComboBox cboComprobante;
        private bool _sincronizandoComprobante;

        private Label lblRotuloCliente;
        private Label lblRotuloComprobante;

        private void ConstruirCliente()
        {
            // El ancho se toma del ticket: un Panel nuevo mide 200 y, con los controles anclados a la
            // derecha, al estirarse se corrían fuera de la pantalla. Por eso la distribución se calcula
            // a mano cada vez que cambia el tamaño (AcomodarCliente), sin depender de anclajes.
            panelCliente = new Panel
            {
                Dock = DockStyle.Top,
                Height = 138,
                Width = panelTicket.ClientSize.Width,
                BackColor = Color.White
            };

            Color gris = Color.FromArgb(120, 125, 135);

            lblRotuloCliente = new Label
            {
                Text = "CLIENTE",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = gris,
                AutoSize = true
            };

            lblCliente = new Label
            {
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                AutoEllipsis = true,
                UseMnemonic = false
            };

            lblClienteDetalle = new Label
            {
                ForeColor = gris,
                Font = new Font("Segoe UI", 8.5F),
                AutoEllipsis = true,
                UseMnemonic = false
            };

            btnCliente = new Button
            {
                Text = "Buscar o cargar cliente  (F3)",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(231, 239, 255),
                ForeColor = ColorAcento,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            btnCliente.FlatAppearance.BorderColor = ColorAcento;
            btnCliente.Click += (s, e) => ElegirCliente();

            lblRotuloComprobante = new Label
            {
                Text = "COMPROBANTE",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = gris,
                AutoSize = true
            };

            cboComprobante = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboComprobante.Items.AddRange(NegocioComprobante.Tipos);
            cboComprobante.SelectedIndex = 0;
            cboComprobante.SelectedIndexChanged += cboComprobante_SelectedIndexChanged;

            panelCliente.Controls.AddRange(new Control[]
            {
                lblRotuloCliente, lblCliente, lblClienteDetalle, btnCliente, lblRotuloComprobante, cboComprobante
            });

            panelCliente.Resize += (s, e) => AcomodarCliente();

            // Queda debajo del título "Ticket en curso" y arriba de la tabla.
            panelTicket.Controls.Add(panelCliente);
            panelTicket.Controls.SetChildIndex(panelCliente, panelTicket.Controls.GetChildIndex(lblTituloTicket));

            AcomodarCliente();
            ActualizarCliente();
        }

        /// <summary>Coloca cada control según el ancho real del panel, con 14 px de margen a cada lado.</summary>
        private void AcomodarCliente()
        {
            const int margen = 14;
            int ancho = Math.Max(120, panelCliente.ClientSize.Width - 2 * margen);

            lblRotuloCliente.Location = new Point(margen, 8);

            lblCliente.SetBounds(margen, 24, ancho, 22);
            lblClienteDetalle.SetBounds(margen, 46, ancho, 18);
            btnCliente.SetBounds(margen, 70, ancho, 32);

            lblRotuloComprobante.Location = new Point(margen, 114);
            int xCombo = margen + 98;
            cboComprobante.SetBounds(xCombo, 110, Math.Max(80, panelCliente.ClientSize.Width - xCombo - margen), 24);
        }
        /// <summary>Abre la ventana de clientes y deja en la venta lo que se haya elegido.</summary>
        private void ElegirCliente()
        {
            if (_venta == null) return;

            using (var ventana = new frmCliente())
            {
                if (ventana.ShowDialog(this) != DialogResult.OK)
                    return;

                NegocioVenta.AsignarCliente(_venta, ventana.ClienteElegido);
                ActualizarCliente();
                RevalidarComprobante();
            }
        }

        private void cboComprobante_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_sincronizandoComprobante || _venta == null) return;

            var tipo = cboComprobante.SelectedItem as TipoComprobanteItem;
            if (tipo == null) return;

            try
            {
                NegocioComprobante.Validar(_venta, tipo.Codigo);
                _venta.CodigoComprobante = tipo.Codigo;
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Comprobante", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                SincronizarComprobante();   // vuelve al que estaba
            }
        }

        /// <summary>Si al cambiar de cliente el comprobante elegido ya no corresponde, vuelve a Ticket y avisa.</summary>
        private void RevalidarComprobante()
        {
            try
            {
                NegocioComprobante.Validar(_venta, _venta.CodigoComprobante);
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + Environment.NewLine + "El comprobante vuelve a Ticket.",
                    "Comprobante", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _venta.CodigoComprobante = NegocioComprobante.Ticket;
            }

            SincronizarComprobante();
        }

        /// <summary>Deja el selector mostrando el comprobante que tiene la venta.</summary>
        private void SincronizarComprobante()
        {
            if (cboComprobante == null) return;

            _sincronizandoComprobante = true;
            try
            {
                string codigo = _venta == null ? NegocioComprobante.Ticket : _venta.CodigoComprobante;
                for (int i = 0; i < cboComprobante.Items.Count; i++)
                    if (((TipoComprobanteItem)cboComprobante.Items[i]).Codigo == codigo)
                        cboComprobante.SelectedIndex = i;
            }
            finally
            {
                _sincronizandoComprobante = false;
            }
        }

        private void ActualizarCliente()
        {
            ClienteItem cliente = _venta == null ? null : _venta.Cliente;

            if (cliente == null)
            {
                lblCliente.Text = "Consumidor final";
                lblClienteDetalle.Text = "Sin datos del cliente";
                SincronizarComprobante();
                return;
            }

            lblCliente.Text = cliente.NombreCompleto;

            string cuit = string.IsNullOrEmpty(cliente.Cuit) ? string.Empty : "  ·  CUIT " + cliente.Cuit;
            lblClienteDetalle.Text = "DNI " + cliente.Dni + cuit + "  ·  " + cliente.CondicionIva;
            SincronizarComprobante();
        }
    }
}
