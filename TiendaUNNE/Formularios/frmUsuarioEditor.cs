using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>Alta / edición de un usuario (datos de Persona + Usuario en una sola pantalla).</summary>
    public partial class frmUsuarioEditor : Form
    {
        private readonly int? _idUsuario;   // null = alta
        private UsuarioEditModel _original;  // solo en edición

        public frmUsuarioEditor(int? idUsuario)
        {
            InitializeComponent();
            _idUsuario = idUsuario;
        }

        private bool EsAlta => !_idUsuario.HasValue;

        private void frmUsuarioEditor_Load(object sender, EventArgs e)
        {
            dtpFechaNac.MaxDate = DateTime.Today;

            try
            {
                CargarPerfiles();

                if (EsAlta)
                {
                    Text = "Nuevo usuario";
                    lblPasswordAyuda.Text = "Obligatoria.";
                    dtpFechaNac.Checked = false;
                }
                else
                {
                    Text = "Editar usuario";
                    lblPasswordAyuda.Text = "Dejar en blanco para no cambiarla.";
                    CargarUsuario(_idUsuario.Value);
                }
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void CargarPerfiles()
        {
            cboPerfil.DataSource = ServicioPerfil.ListarActivos();
            cboPerfil.DisplayMember = "Nombre";
            cboPerfil.ValueMember = "Id";
            cboPerfil.SelectedIndex = -1;
        }

        private void CargarUsuario(int idUsuario)
        {
            _original = ServicioUsuario.ObtenerParaEdicion(idUsuario);

            txtDniCuit.Text = _original.DniCuit;
            txtNombre.Text = _original.Nombre;
            txtApellido.Text = _original.Apellido;
            txtDireccion.Text = _original.Direccion;
            txtTelefono.Text = _original.Telefono;
            txtEmail.Text = _original.Email;

            if (_original.FechaNacimiento.HasValue)
            {
                dtpFechaNac.Value = _original.FechaNacimiento.Value;
                dtpFechaNac.Checked = true;
            }
            else
            {
                dtpFechaNac.Checked = false;
            }

            txtNombreUsuario.Text = _original.NombreUsuario;
            txtPassword.Text = string.Empty;
            cboPerfil.SelectedValue = _original.IdPerfil;
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

            UsuarioEditModel modelo = ArmarModelo();

            try
            {
                Cursor = Cursors.WaitCursor;
                int idSesion = SesionActual.Usuario.IdUsuario;

                if (EsAlta)
                    ServicioUsuario.Crear(modelo, idSesion);
                else
                    ServicioUsuario.Actualizar(modelo, idSesion);

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
            if (string.IsNullOrWhiteSpace(txtDniCuit.Text)) return "Ingresá el DNI/CUIT.";
            if (string.IsNullOrWhiteSpace(txtNombre.Text)) return "Ingresá el nombre.";
            if (string.IsNullOrWhiteSpace(txtApellido.Text)) return "Ingresá el apellido.";
            if (string.IsNullOrWhiteSpace(txtNombreUsuario.Text)) return "Ingresá el nombre de usuario.";
            if (cboPerfil.SelectedIndex < 0) return "Elegí un perfil.";

            if (EsAlta && string.IsNullOrWhiteSpace(txtPassword.Text))
                return "La contraseña es obligatoria en un alta.";

            string email = txtEmail.Text.Trim();
            if (email.Length > 0 && (!email.Contains("@") || email.EndsWith("@") || email.StartsWith("@")))
                return "El email no tiene un formato válido.";

            return null;
        }

        private UsuarioEditModel ArmarModelo()
        {
            var perfil = (PerfilItem)cboPerfil.SelectedItem;

            return new UsuarioEditModel
            {
                IdUsuario = EsAlta ? 0 : _original.IdUsuario,
                IdPersona = EsAlta ? 0 : _original.IdPersona,

                DniCuit = txtDniCuit.Text.Trim(),
                Nombre = txtNombre.Text.Trim(),
                Apellido = txtApellido.Text.Trim(),
                Direccion = txtDireccion.Text.Trim(),
                Telefono = txtTelefono.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                FechaNacimiento = dtpFechaNac.Checked ? dtpFechaNac.Value.Date : (DateTime?)null,

                NombreUsuario = txtNombreUsuario.Text.Trim(),
                PasswordPlano = txtPassword.Text,   // vacío en edición = no cambiar
                IdPerfil = perfil.Id,
                NombrePerfil = perfil.Nombre
            };
        }
    }
}
