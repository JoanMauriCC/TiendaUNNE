using System;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Alta / edición de un usuario (datos de Persona + Usuario en una sola pantalla).
    /// El formulario solo muestra datos y recoge lo tipeado: validar, recortar espacios,
    /// hashear la contraseña y decidir alta/edición es trabajo de NegocioUsuario.
    /// </summary>
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
            dtpFechaNac.MinDate = NegocioUsuario.FechaNacimientoMinima;
            dtpFechaNac.MaxDate = NegocioUsuario.FechaNacimientoMaxima;

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
            catch (Exception)
            {
                MessageBox.Show("No se pudieron cargar los datos del usuario.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        /// <summary>
        /// Evento propio del formulario: filtra lo que se puede tipear en el DNI/CUIT.
        /// Qué carácter es aceptable lo decide Negocio, acá solo se aplica.
        /// </summary>
        private void txtDniCuit_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !Validaciones.EsCaracterDniCuitValido(e.KeyChar);
        }

        private void txtTelefono_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !Validaciones.EsCaracterTelefonoValido(e.KeyChar);
        }

        private void txtNombre_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !Validaciones.EsCaracterNombrePersonaValido(e.KeyChar);
        }

        private void txtApellido_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !Validaciones.EsCaracterNombrePersonaValido(e.KeyChar);
        }

        private void txtEmail_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !Validaciones.EsCaracterEmailValido(e.KeyChar);
        }

        private void txtNombreUsuario_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !Validaciones.EsCaracterNombreUsuarioValido(e.KeyChar);
        }

        private void CargarPerfiles()
        {
            cboPerfil.DataSource = NegocioPerfil.ListarActivos();
            cboPerfil.DisplayMember = "Nombre";
            cboPerfil.ValueMember = "Id";
            cboPerfil.SelectedIndex = -1;
        }

        private void CargarUsuario(int idUsuario)
        {
            _original = NegocioUsuario.ObtenerParaEdicion(idUsuario);

            txtDniCuit.Text = _original.DniCuit;
            txtNombre.Text = _original.Nombre;
            txtApellido.Text = _original.Apellido;
            txtDireccion.Text = _original.Direccion;
            txtTelefono.Text = _original.Telefono;
            txtEmail.Text = _original.Email;

            if (_original.FechaNacimiento.HasValue)
            {
                dtpFechaNac.Value = NegocioUsuario.AcotarFechaNacimiento(_original.FechaNacimiento.Value);
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
            try
            {
                Cursor = Cursors.WaitCursor;
                NegocioUsuario.Guardar(ArmarModelo(), SesionActual.Usuario.IdUsuario);

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
                MessageBox.Show("No se pudo guardar el usuario.",
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

        /// <summary>
        /// Copia lo que hay en pantalla al modelo, sin transformarlo: de eso se encarga Negocio.
        /// </summary>
        private UsuarioEditModel ArmarModelo()
        {
            var perfil = cboPerfil.SelectedItem as PerfilItem;

            return new UsuarioEditModel
            {
                IdUsuario = EsAlta ? 0 : _original.IdUsuario,
                IdPersona = EsAlta ? 0 : _original.IdPersona,

                DniCuit = txtDniCuit.Text,
                Nombre = txtNombre.Text,
                Apellido = txtApellido.Text,
                Direccion = txtDireccion.Text,
                Telefono = txtTelefono.Text,
                Email = txtEmail.Text,
                FechaNacimiento = dtpFechaNac.Checked ? dtpFechaNac.Value.Date : (DateTime?)null,

                NombreUsuario = txtNombreUsuario.Text,
                PasswordPlano = txtPassword.Text,   // vacío en edición = no cambiar
                IdPerfil = perfil == null ? 0 : perfil.Id,
                NombrePerfil = perfil == null ? null : perfil.Nombre
            };
        }
    }
}
