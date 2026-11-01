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

        private void ConstruirCliente()
        {
            panelCliente = new Panel { Dock = DockStyle.Top, Height = 96, BackColor = Color.White };

            panelCliente.Controls.Add(new Label
            {
                Text = "CLIENTE",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 125, 135),
                AutoSize = true,
                Location = new Point(14, 6)
            });

            lblCliente = new Label
            {
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                AutoEllipsis = true,
                UseMnemonic = false,
                Location = new Point(14, 22),
                Size = new Size(200, 20)
            };
            panelCliente.Controls.Add(lblCliente);

            lblClienteDetalle = new Label
            {
                ForeColor = Color.FromArgb(120, 125, 135),
                Font = new Font("Segoe UI", 8.5F),
                AutoEllipsis = true,
                UseMnemonic = false,
                Location = new Point(14, 40),
                Size = new Size(200, 16)
            };
            panelCliente.Controls.Add(lblClienteDetalle);

            btnCliente = new Button
            {
                Text = "Elegir (F3)",
                Size = new Size(104, 30),
                Location = new Point(panelTicket.Width - 104 - 14, 14),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                UseVisualStyleBackColor = true
            };
            btnCliente.Click += (s, e) => ElegirCliente();
            panelCliente.Controls.Add(btnCliente);

            panelCliente.Controls.Add(new Label
            {
                Text = "COMPROBANTE",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 125, 135),
                AutoSize = true,
                Location = new Point(14, 70)
            });

            cboComprobante = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(104, 66),
                Size = new Size(panelTicket.Width - 104 - 14, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            cboComprobante.Items.AddRange(NegocioComprobante.Tipos);
            cboComprobante.SelectedIndex = 0;
            cboComprobante.SelectedIndexChanged += cboComprobante_SelectedIndexChanged;
            panelCliente.Controls.Add(cboComprobante);

            panelCliente.Resize += (s, e) =>
            {
                int ancho = Math.Max(60, btnCliente.Left - 14 - 8);
                lblCliente.Width = ancho;
                lblClienteDetalle.Width = ancho;
            };

            // Queda debajo del título "Ticket en curso" y arriba de la tabla.
            panelTicket.Controls.Add(panelCliente);
            panelTicket.Controls.SetChildIndex(panelCliente, panelTicket.Controls.GetChildIndex(lblTituloTicket));

            ActualizarCliente();
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
