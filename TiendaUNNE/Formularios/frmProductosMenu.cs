using System;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>Submenú de Productos: tarjetas para el CRUD de productos y el de categorías.</summary>
    public partial class frmProductosMenu : Form
    {
        public frmProductosMenu()
        {
            InitializeComponent();
        }

        private void frmProductosMenu_Load(object sender, EventArgs e)
        {
            panelTarjetas.SuspendLayout();
            panelTarjetas.Controls.Clear();

            panelTarjetas.Controls.Add(CrearTarjeta(
                IconoTarjeta.Producto, "Cargar producto", "Alta, edición y baja", AbrirProductos));
            panelTarjetas.Controls.Add(CrearTarjeta(
                IconoTarjeta.Etiqueta, "Categorías", "Organizá los rubros", AbrirCategorias));

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
            tarjeta.Click += (s, e) => { if (tarjeta.Habilitada) onClick(); };
            return tarjeta;
        }

        private void AbrirProductos()
        {
            using (var f = new frmProductos())
                f.ShowDialog(this);
        }

        private void AbrirCategorias()
        {
            using (var f = new frmCategorias())
                f.ShowDialog(this);
        }

        private void btnAtras_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
