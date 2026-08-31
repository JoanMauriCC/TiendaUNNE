using System;
using System.Windows.Forms;

namespace TiendaUNNE
{
    public partial class frmPrincipal : Form
    {
        public frmPrincipal()
        {
            InitializeComponent();
        }

        private void frmPrincipal_Load(object sender, EventArgs e)
        {
            if (SesionActual.HaySesion)
            {
                lblSesion.Text = string.Format("Usuario: {0}  |  Rol: {1}",
                    SesionActual.Usuario.NombreCompleto, SesionActual.Usuario.Rol);
            }

            // El CRUD de Usuarios solo es visible/accesible para el rol Administrador.
            menuUsuarios.Visible = SesionActual.EsAdministrador;
        }

        private void menuUsuarios_Click(object sender, EventArgs e)
        {
            // Segunda barrera: aunque el menú esté oculto, se vuelve a validar el rol.
            if (!SesionActual.EsAdministrador)
            {
                MessageBox.Show("No tiene permisos para acceder a esta opción.",
                    "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var f = new frmUsuarios())
                f.ShowDialog(this);
        }

        private void menuCerrarSesion_Click(object sender, EventArgs e)
        {
            SesionActual.Cerrar();
            Application.Restart();
        }

        private void menuSalir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
