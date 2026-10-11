using System;
using System.Drawing;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Confirmación antes de restaurar una copia: se muestra qué se va a pisar y hay que
    /// escribir la palabra de confirmación. La palabra y su comprobación son de
    /// NegocioBackup; acá solo se pide y se habilita o no el botón.
    /// </summary>
    public partial class frmConfirmarRestauracion : Form
    {
        public frmConfirmarRestauracion(string archivo, string fechaHora, string tamanio)
        {
            InitializeComponent();

            lblArchivo.Text = archivo;
            lblCreada.Text = fechaHora;
            lblTamanio.Text = tamanio;
            lblConfirmar.Text = "Para continuar, escribí " + NegocioBackup.PalabraConfirmacion;

            // El Designer deja el botón deshabilitado antes de conectar el evento: se aplica el color a mano.
            btnRestaurar_EnabledChanged(btnRestaurar, EventArgs.Empty);
        }

        /// <summary>Lo que se escribió en el campo de confirmación (Negocio lo vuelve a verificar).</summary>
        public string Confirmacion => txtConfirmacion.Text;

        private void txtConfirmacion_TextChanged(object sender, EventArgs e)
        {
            btnRestaurar.Enabled = NegocioBackup.ConfirmacionValida(txtConfirmacion.Text);
        }

        // Un botón plano deshabilitado no se ve "apagado" por sí solo: se le cambia el color.
        private void btnRestaurar_EnabledChanged(object sender, EventArgs e)
        {
            btnRestaurar.BackColor = btnRestaurar.Enabled
                ? Color.FromArgb(185, 28, 28)
                : Color.FromArgb(214, 216, 220);
            btnRestaurar.ForeColor = btnRestaurar.Enabled ? Color.White : Color.FromArgb(120, 125, 135);
        }

        private void btnRestaurar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
