using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>Alta / edición de un producto.</summary>
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
            catch (SqlException ex)
            {
                MessageBox.Show("No se pudieron cargar los datos.\n\n" + ex.Message,
                    "Base de datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void CargarCategorias()
        {
            cboCategoria.DataSource = ServicioCategoria.ListarActivas();
            cboCategoria.DisplayMember = "Nombre";
            cboCategoria.ValueMember = "Id";
            cboCategoria.SelectedIndex = -1;
        }

        private void CargarProducto(int idProducto)
        {
            _original = ServicioProducto.ObtenerParaEdicion(idProducto);

            txtNombre.Text = _original.Nombre;
            txtDescripcion.Text = _original.Descripcion;
            numPrecioVenta.Value = Acotar(numPrecioVenta, _original.PrecioVenta);
            numStock.Value = Acotar(numStock, _original.Stock);
            cboCategoria.SelectedValue = _original.IdCategoria;
        }

        private static decimal Acotar(NumericUpDown ctrl, decimal valor)
        {
            if (valor < ctrl.Minimum) return ctrl.Minimum;
            if (valor > ctrl.Maximum) return ctrl.Maximum;
            return valor;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            string error = Validar();
            if (error != null)
            {
                MessageBox.Show(error, "Datos incompletos",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var categoria = (CategoriaItem)cboCategoria.SelectedItem;
            var modelo = new ProductoEditModel
            {
                IdProducto = EsAlta ? 0 : _original.IdProducto,
                Nombre = txtNombre.Text.Trim(),
                Descripcion = txtDescripcion.Text.Trim(),
                PrecioVenta = numPrecioVenta.Value,
                Stock = numStock.Value,
                IdCategoria = categoria.Id,
                NombreCategoria = categoria.Nombre
            };

            try
            {
                Cursor = Cursors.WaitCursor;
                int idSesion = SesionActual.Usuario.IdUsuario;

                if (EsAlta)
                    ServicioProducto.Crear(modelo, idSesion);
                else
                    ServicioProducto.Actualizar(modelo, idSesion);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo guardar",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error de base de datos.\n\n" + ex.Message,
                    "Base de datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private string Validar()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text)) return "Ingresá el nombre del producto.";
            if (cboCategoria.SelectedIndex < 0) return "Elegí una categoría.";

            // Los NumericUpDown ya tienen Minimum = 0; se revalida por las dudas.
            if (numPrecioVenta.Value < 0 || numStock.Value < 0)
                return "El precio de venta y el stock no pueden ser negativos.";

            return null;
        }
    }
}
