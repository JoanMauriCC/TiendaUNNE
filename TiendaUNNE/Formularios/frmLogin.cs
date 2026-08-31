using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace TiendaUNNE
{
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
            string usuario = txtUsuario.Text.Trim();
            string password = txtPassword.Text;

            lblError.Visible = false;

            if (usuario.Length == 0 || password.Length == 0)
            {
                MostrarError("Ingrese usuario y contraseña.");
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;

                UsuarioLogueado login = ServicioAutenticacion.Autenticar(usuario, password);

                if (login == null)
                {
                    // Mensaje genérico: no se indica si falló el usuario o la contraseña.
                    MostrarError("Usuario o contraseña incorrectos.");
                    txtPassword.Clear();
                    txtPassword.Focus();
                    return;
                }

                SesionActual.Iniciar(login);
                DialogResult = DialogResult.OK;   // Program.cs abre frmPrincipal
                Close();
            }
            catch (SqlException ex)
            {
                MostrarError("No se pudo conectar con la base de datos.");
                MessageBox.Show(ex.Message, "Detalle del error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
