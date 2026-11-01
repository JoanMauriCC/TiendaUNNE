using System;
using System.Drawing;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Pide el motivo antes de anular una venta. Es un paso de confirmación a propósito: anular
    /// mueve plata y stock. Solo recoge el texto; si alcanza lo decide NegocioAnulacion.
    /// </summary>
    public class frmMotivoAnulacion : Form
    {
        private readonly TextBox txtMotivo = new TextBox();
        private readonly Button btnAnular = new Button();

        /// <summary>Lo que se escribió como motivo.</summary>
        public string Motivo => txtMotivo.Text;

        public frmMotivoAnulacion(string descripcionVenta)
        {
            Text = "Anular venta";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(480, 270);
            Font = new Font("Segoe UI", 9F);

            Controls.Add(new Label
            {
                Text = "Vas a anular:",
                ForeColor = Color.FromArgb(120, 125, 135),
                AutoSize = true,
                Location = new Point(16, 14)
            });

            Controls.Add(new Label
            {
                Text = descripcionVenta,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                AutoEllipsis = true,
                UseMnemonic = false,
                Location = new Point(16, 34),
                Size = new Size(448, 22)
            });

            Controls.Add(new Label
            {
                Text = "El stock vuelve al depósito y la devolución queda como egreso de caja. " +
                       "Esto no se puede deshacer.",
                ForeColor = Color.FromArgb(154, 91, 0),
                Location = new Point(16, 64),
                Size = new Size(448, 34)
            });

            Controls.Add(new Label
            {
                Text = "Motivo de la anulación *",
                AutoSize = true,
                Location = new Point(16, 106)
            });

            txtMotivo.Multiline = true;
            txtMotivo.ScrollBars = ScrollBars.Vertical;
            txtMotivo.MaxLength = NegocioAnulacion.LargoMaximoMotivo;
            txtMotivo.Location = new Point(16, 126);
            txtMotivo.Size = new Size(448, 82);
            txtMotivo.TextChanged += (s, e) => ActualizarBoton();
            Controls.Add(txtMotivo);

            btnAnular.Text = "Anular venta";
            btnAnular.Location = new Point(16, 222);
            btnAnular.Size = new Size(160, 34);
            btnAnular.FlatStyle = FlatStyle.Flat;
            btnAnular.FlatAppearance.BorderSize = 0;
            btnAnular.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnAnular.UseVisualStyleBackColor = false;
            btnAnular.EnabledChanged += (s, e) => PintarBoton();
            btnAnular.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };
            Controls.Add(btnAnular);

            var btnVolver = new Button
            {
                Text = "Volver",
                Location = new Point(364, 222),
                Size = new Size(100, 34),
                DialogResult = DialogResult.Cancel
            };
            Controls.Add(btnVolver);
            CancelButton = btnVolver;

            ActualizarBoton();
        }

        private void ActualizarBoton()
        {
            btnAnular.Enabled = txtMotivo.Text.Trim().Length >= NegocioAnulacion.LargoMinimoMotivo;
            PintarBoton();
        }

        // Un botón plano deshabilitado no se ve "apagado" por sí solo: se le cambia el color.
        private void PintarBoton()
        {
            btnAnular.BackColor = btnAnular.Enabled ? Color.FromArgb(185, 28, 28) : Color.FromArgb(214, 216, 220);
            btnAnular.ForeColor = btnAnular.Enabled ? Color.White : Color.FromArgb(120, 125, 135);
        }
    }
}
