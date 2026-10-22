using System;
using System.Drawing;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Confirmación antes de restaurar una copia: se muestra qué se va a pisar y se piden las
    /// dos llaves (la contraseña de quien tiene la sesión y el DNI y la contraseña de otro
    /// Administrador o Supervisor) más la palabra de confirmación. Si las llaves alcanzan lo
    /// decide NegocioBackup; acá solo se piden, se cuentan los intentos y se habilita el botón.
    /// </summary>
    public partial class frmConfirmarRestauracion : Form
    {
        private int _intentosFallidos;

        public frmConfirmarRestauracion(string archivo, string fechaHora, string tamanio)
        {
            InitializeComponent();

            lblArchivo.Text = archivo;
            lblCreada.Text = fechaHora;
            lblTamanio.Text = tamanio;
            lblConfirmar.Text = "Para continuar, escribí " + NegocioBackup.PalabraConfirmacion;
            if (SesionActual.HaySesion)
                lblLlave1.Text = "Llave 1: " + SesionActual.Usuario.NombreCompleto + " (sesión actual)";

            // El Designer deja el botón deshabilitado antes de conectar el evento: se aplica el color a mano.
            btnRestaurar_EnabledChanged(btnRestaurar, EventArgs.Empty);
        }

        /// <summary>Lo que se escribió en el campo de confirmación (Negocio lo vuelve a verificar).</summary>
        public string Confirmacion => txtConfirmacion.Text;

        /// <summary>Las dos llaves tal como se escribieron (Negocio las vuelve a verificar al restaurar).</summary>
        public LlavesRestauracion Llaves => new LlavesRestauracion
        {
            PasswordPropia = txtPasswordPropia.Text,
            DniSegundo = txtDniSegundo.Text,
            PasswordSegundo = txtPasswordSegundo.Text
        };

        /// <summary>El botón se habilita cuando están las dos llaves y la palabra de confirmación.</summary>
        private void Campo_TextChanged(object sender, EventArgs e)
        {
            btnRestaurar.Enabled = txtPasswordPropia.TextLength > 0
                                   && txtDniSegundo.TextLength > 0
                                   && txtPasswordSegundo.TextLength > 0
                                   && NegocioBackup.ConfirmacionValida(txtConfirmacion.Text);
        }

        private void txtDniSegundo_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !Validaciones.EsCaracterDniValido(e.KeyChar);
        }

        // Un botón plano deshabilitado no se ve "apagado" por sí solo: se le cambia el color.
        private void btnRestaurar_EnabledChanged(object sender, EventArgs e)
        {
            btnRestaurar.BackColor = btnRestaurar.Enabled
                ? Color.FromArgb(185, 28, 28)
                : Color.FromArgb(214, 216, 220);
            btnRestaurar.ForeColor = btnRestaurar.Enabled ? Color.White : Color.FromArgb(120, 125, 135);
        }

        /// <summary>
        /// Se verifican las llaves antes de cerrar, así un error se corrige acá mismo. Después
        /// de los intentos permitidos la ventana se cierra y la restauración se cancela.
        /// </summary>
        private void btnRestaurar_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                NegocioBackup.VerificarLlaves(Llaves);
            }
            catch (ReglaNegocioException ex)
            {
                Cursor = Cursors.Default;
                _intentosFallidos++;
                txtPasswordPropia.Clear();
                txtPasswordSegundo.Clear();

                int restantes = NegocioBackup.IntentosLlaves - _intentosFallidos;
                if (restantes <= 0)
                {
                    MessageBox.Show(ex.Message + "\n\nSe agotaron los intentos: la restauración se cancela.",
                        "Restaurar copia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.Cancel;
                    Close();
                    return;
                }

                MessageBox.Show(ex.Message + "\n\nIntentos restantes: " + restantes + ".",
                    "Restaurar copia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPasswordPropia.Focus();
                return;
            }
            catch (Exception)
            {
                Cursor = Cursors.Default;
                MessageBox.Show("No se pudieron verificar las llaves.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Cursor = Cursors.Default;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
