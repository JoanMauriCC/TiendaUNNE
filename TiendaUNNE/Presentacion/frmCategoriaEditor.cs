using System;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Alta / edición de una categoría. Solo muestra datos y recoge lo que escribe el
    /// usuario: validar, recortar espacios y decidir si es alta o edición lo hace NegocioCategoria.
    /// </summary>
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
                    _original = NegocioCategoria.ObtenerParaEdicion(_idCategoria.Value);
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
            catch (Exception)
            {
                MessageBox.Show("No se pudieron cargar los datos de la categoría.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            var modelo = new CategoriaEditModel
            {
                IdCategoria = EsAlta ? 0 : _original.IdCategoria,
                Nombre = txtNombre.Text,
                Descripcion = txtDescripcion.Text
            };

            try
            {
                Cursor = Cursors.WaitCursor;
                NegocioCategoria.Guardar(modelo, SesionActual.Usuario.IdUsuario);

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
                MessageBox.Show("No se pudo guardar la categoría.",
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
