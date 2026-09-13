using System;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Pantalla de acceso. Solo toma lo que se tipea y muestra el resultado:
    /// validar y verificar las credenciales es trabajo de NegocioAutenticacion.
    /// </summary>
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            lblError.Visible = false;
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;

            try
            {
                Cursor = Cursors.WaitCursor;

                NegocioAutenticacion.IniciarSesion(txtUsuario.Text, txtPassword.Text);

                DialogResult = DialogResult.OK;   // Program.cs abre frmPrincipal
                Close();
            }
            catch (ReglaNegocioException ex)
            {
                MostrarError(ex.Message);
                txtPassword.Clear();
                txtPassword.Focus();
            }
            catch (Exception)
            {
                MostrarError("No se pudo conectar con la base de datos.");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void MostrarError(string mensaje)
        {
            lblError.Text = mensaje;
            lblError.Visible = true;
        }
    }
}
