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

            // Los CRUD de administración solo son visibles/accesibles para el rol Administrador.
            menuUsuarios.Visible = SesionActual.EsAdministrador;
            menuCategorias.Visible = SesionActual.EsAdministrador;
            menuProductos.Visible = SesionActual.EsAdministrador;
        }

        private void menuUsuarios_Click(object sender, EventArgs e)
        {
            if (!ValidarAccesoAdministrador()) return;

            using (var f = new frmUsuarios())
                f.ShowDialog(this);
        }

        private void menuCategorias_Click(object sender, EventArgs e)
        {
            if (!ValidarAccesoAdministrador()) return;

            using (var f = new frmCategorias())
                f.ShowDialog(this);
        }

        private void menuProductos_Click(object sender, EventArgs e)
        {
            if (!ValidarAccesoAdministrador()) return;

            using (var f = new frmProductos())
                f.ShowDialog(this);
        }

        // Segunda barrera: aunque el menú esté oculto, se vuelve a validar el rol.
        private bool ValidarAccesoAdministrador()
        {
            if (SesionActual.EsAdministrador) return true;

            MessageBox.Show("No tiene permisos para acceder a esta opción.",
                "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
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
