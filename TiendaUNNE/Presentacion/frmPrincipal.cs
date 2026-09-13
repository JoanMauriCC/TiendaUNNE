using System;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Menú principal. Solo dibuja las tarjetas y abre pantallas: qué opciones ve cada rol
    /// y si tiene permiso para entrar lo decide NegocioSeguridad.
    /// </summary>
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
                lblUsuario.Text = "Bienvenido al sistema, " + SesionActual.Usuario.NombreCompleto;

            ConstruirTarjetas();
        }

        private void ConstruirTarjetas()
        {
            panelTarjetas.SuspendLayout();
            panelTarjetas.Controls.Clear();

            if (NegocioSeguridad.PuedeAcceder(OpcionMenu.Usuarios))
                panelTarjetas.Controls.Add(
                    CrearTarjeta(IconoTarjeta.Usuarios, "Usuarios", "Altas, bajas y roles", AbrirUsuarios));

            if (NegocioSeguridad.PuedeAcceder(OpcionMenu.Productos))
                panelTarjetas.Controls.Add(
                    CrearTarjeta(IconoTarjeta.Producto, "Productos", "Productos y categorías", AbrirProductosMenu));

            if (NegocioSeguridad.PuedeAcceder(OpcionMenu.Auditoria))
                panelTarjetas.Controls.Add(
                    CrearTarjeta(IconoTarjeta.Auditoria, "Auditoría", "Historial de cambios", AbrirAuditoria));

            var caja = CrearTarjeta(IconoTarjeta.Caja, "Caja", "Próximamente", null);
            caja.Habilitada = NegocioSeguridad.ModuloDisponible(OpcionMenu.Caja);
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
            if (!TienePermiso(OpcionMenu.Usuarios)) return;
            using (var f = new frmUsuarios())
                f.ShowDialog(this);
        }

        private void AbrirProductosMenu()
        {
            if (!TienePermiso(OpcionMenu.Productos)) return;
            using (var f = new frmProductosMenu())
                f.ShowDialog(this);
        }

        private void AbrirAuditoria()
        {
            if (!TienePermiso(OpcionMenu.Auditoria)) return;
            using (var f = new frmAuditoria())
                f.ShowDialog(this);
        }

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            NegocioAutenticacion.CerrarSesion();
            Application.Restart();
        }

        /// <summary>Le pregunta a Negocio y, si dice que no, muestra el motivo.</summary>
        private bool TienePermiso(OpcionMenu opcion)
        {
            try
            {
                NegocioSeguridad.ValidarAcceso(opcion);
                return true;
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Acceso denegado",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }
    }
}
