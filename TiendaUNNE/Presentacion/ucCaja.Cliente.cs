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

        private void ConstruirCliente()
        {
            panelCliente = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = Color.White };

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
            }
        }

        private void ActualizarCliente()
        {
            ClienteItem cliente = _venta == null ? null : _venta.Cliente;

            if (cliente == null)
            {
                lblCliente.Text = "Consumidor final";
                lblClienteDetalle.Text = "Sin datos del cliente";
                return;
            }

            lblCliente.Text = cliente.NombreCompleto;

            string cuit = string.IsNullOrEmpty(cliente.Cuit) ? string.Empty : "  ·  CUIT " + cliente.Cuit;
            lblClienteDetalle.Text = "DNI " + cliente.Dni + cuit + "  ·  " + cliente.CondicionIva;
        }
    }
}
