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
            lblMarca.Text = "TiendaUNNE";

            if (SesionActual.HaySesion)
                lblUsuario.Text = "Bienvenido, " + SesionActual.Usuario.NombreCompleto;

            ConstruirTarjetas();
        }

        private void ConstruirTarjetas()
        {
            panelTarjetas.SuspendLayout();
            panelTarjetas.Controls.Clear();

            if (SesionActual.EsAdministrador)
            {
                panelTarjetas.Controls.Add(
                    CrearTarjeta(IconoTarjeta.Usuarios, "Usuarios", "Altas, bajas y roles", AbrirUsuarios));
                panelTarjetas.Controls.Add(
                    CrearTarjeta(IconoTarjeta.Producto, "Productos", "Productos y categorías", AbrirProductosMenu));
                panelTarjetas.Controls.Add(
                    CrearTarjeta(IconoTarjeta.Auditoria, "Auditoría", "Historial de cambios", AbrirAuditoria));
            }

            var caja = CrearTarjeta(IconoTarjeta.Caja, "Caja", "Próximamente", null);
            caja.Habilitada = false;
            panelTarjetas.Controls.Add(caja);

            panelTarjetas.ResumeLayout();
        }

        private TarjetaMenu CrearTarjeta(IconoTarjeta icono, string titulo, string descripcion, Action onClick)
        {
            var tarjeta = new TarjetaMenu
            {
                Icono = icono,
                Titulo = titulo,
                Descripcion = descripcion
            };

            if (onClick != null)
                tarjeta.Click += (s, e) => { if (tarjeta.Habilitada) onClick(); };

            return tarjeta;
        }

        // -----------------------------------------------------------------
        // Acciones de las tarjetas (mantienen la doble barrera de rol)
        // -----------------------------------------------------------------

        private void AbrirUsuarios()
        {
            if (!ValidarAccesoAdministrador()) return;
            using (var f = new frmUsuarios())
                f.ShowDialog(this);
        }

        private void AbrirProductosMenu()
        {
            if (!ValidarAccesoAdministrador()) return;
            using (var f = new frmProductosMenu())
                f.ShowDialog(this);
        }

        private void AbrirAuditoria()
        {
            if (!ValidarAccesoAdministrador()) return;
            using (var f = new frmAuditoria())
                f.ShowDialog(this);
        }

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            SesionActual.Cerrar();
            Application.Restart();
        }

        private bool ValidarAccesoAdministrador()
        {
            if (SesionActual.EsAdministrador) return true;

            MessageBox.Show("No tiene permisos para acceder a esta opción.",
                "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
    }
}
