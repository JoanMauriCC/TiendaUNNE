using System;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Alta / edición de un producto. Solo muestra datos y recoge lo que carga el usuario:
    /// validar, recortar espacios, acotar rangos y decidir alta/edición lo hace NegocioProducto.
    /// </summary>
    public partial class frmProductoEditor : Form
    {
        private readonly int? _idProducto;   // null = alta
        private ProductoEditModel _original;

        public frmProductoEditor(int? idProducto)
        {
            InitializeComponent();
            _idProducto = idProducto;
        }

        private bool EsAlta => !_idProducto.HasValue;

        private void frmProductoEditor_Load(object sender, EventArgs e)
        {
            try
            {
                CargarCategorias();

                if (EsAlta)
                {
                    Text = "Nuevo producto";
                }
                else
                {
                    Text = "Editar producto";
                    CargarProducto(_idProducto.Value);
                }
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Productos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.Cancel;
                Close();
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudieron cargar los datos del producto.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void CargarCategorias()
        {
            cboCategoria.DataSource = NegocioCategoria.ListarActivas();
            cboCategoria.DisplayMember = "Nombre";
            cboCategoria.ValueMember = "Id";
            cboCategoria.SelectedIndex = -1;
        }

        private void CargarProducto(int idProducto)
        {
            _original = NegocioProducto.ObtenerParaEdicion(idProducto);

            txtNombre.Text = _original.Nombre;
            txtDescripcion.Text = _original.Descripcion;
            numPrecioVenta.Value = NegocioProducto.AcotarPrecio(_original.PrecioVenta);
            numStock.Value = NegocioProducto.AcotarStock(_original.Stock);
            cboCategoria.SelectedValue = _original.IdCategoria;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            var categoria = cboCategoria.SelectedItem as CategoriaItem;

            var modelo = new ProductoEditModel
            {
                IdProducto = EsAlta ? 0 : _original.IdProducto,
                Nombre = txtNombre.Text,
                Descripcion = txtDescripcion.Text,
                PrecioVenta = numPrecioVenta.Value,
                Stock = numStock.Value,
                IdCategoria = categoria == null ? 0 : categoria.Id,
                NombreCategoria = categoria == null ? null : categoria.Nombre
            };

            try
            {
                Cursor = Cursors.WaitCursor;
                NegocioProducto.Guardar(modelo, SesionActual.Usuario.IdUsuario);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo guardar",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo guardar el producto.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
}
