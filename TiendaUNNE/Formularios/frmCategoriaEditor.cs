using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>Alta / edición de una categoría.</summary>
    public partial class frmCategoriaEditor : Form
    {
        private readonly int? _idCategoria;   // null = alta
        private CategoriaEditModel _original;

        public frmCategoriaEditor(int? idCategoria)
        {
            InitializeComponent();
            _idCategoria = idCategoria;
        }

        private bool EsAlta => !_idCategoria.HasValue;

        private void frmCategoriaEditor_Load(object sender, EventArgs e)
        {
            try
            {
                if (EsAlta)
                {
                    Text = "Nueva categoría";
                }
                else
                {
                    Text = "Editar categoría";
                    _original = ServicioCategoria.ObtenerParaEdicion(_idCategoria.Value);
                    txtNombre.Text = _original.Nombre;
                    txtDescripcion.Text = _original.Descripcion;
                }
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Categorías", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            string error = Validar();
            if (error != null)
            {
                MessageBox.Show(error, "Datos incompletos",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var modelo = new CategoriaEditModel
            {
                IdCategoria = EsAlta ? 0 : _original.IdCategoria,
                Nombre = txtNombre.Text.Trim(),
                Descripcion = txtDescripcion.Text.Trim()
            };

            try
            {
                Cursor = Cursors.WaitCursor;
                int idSesion = SesionActual.Usuario.IdUsuario;

                if (EsAlta)
                    ServicioCategoria.Crear(modelo, idSesion);
                else
                    ServicioCategoria.Actualizar(modelo, idSesion);

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
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
                return "Ingresá el nombre de la categoría.";
            return null;
        }
    }
}
