using System;
using System.Data;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Sección de usuarios activos (Persona + Usuario + Perfil): formulario de alta/edición
    /// arriba, siempre visible, y listado abajo. Es un UserControl: se muestra dentro del
    /// panel de contenido de frmPrincipal, no en una ventana aparte.
    /// El formulario solo muestra datos y recoge lo tipeado: validar, recortar espacios,
    /// hashear la contraseña y decidir alta/edición es trabajo de NegocioUsuario.
    /// </summary>
    public partial class ucUsuarios : UserControl
    {
        private UsuarioEditModel _original;   // distinto de null = editando ese usuario

        public ucUsuarios()
        {
            InitializeComponent();
        }

        private bool EsAlta => _original == null;

        private void ucUsuarios_Load(object sender, EventArgs e)
        {
            dtpFechaNac.MinDate = NegocioUsuario.FechaNacimientoMinima;
            dtpFechaNac.MaxDate = NegocioUsuario.FechaNacimientoMaxima;

            CargarPerfiles();
            LimpiarFormulario();
            CargarGrilla();
        }

        // -----------------------------------------------------------------
        // Formulario: alta / edición
        // -----------------------------------------------------------------

        private void CargarPerfiles()
        {
            cboPerfil.DataSource = NegocioPerfil.ListarActivos();
            cboPerfil.DisplayMember = "Nombre";
            cboPerfil.ValueMember = "Id";
            cboPerfil.SelectedIndex = -1;
        }

        /// <summary>Deja el formulario listo para cargar un usuario nuevo.</summary>
        private void LimpiarFormulario()
        {
            _original = null;

            lblTituloForm.Text = "Nuevo usuario";
            lblPasswordAyuda.Text = "Obligatoria.";

            txtDniCuit.Clear();
            txtNombre.Clear();
            txtApellido.Clear();
            txtDireccion.Clear();
            txtTelefono.Clear();
            txtEmail.Clear();
            dtpFechaNac.Checked = false;
            txtNombreUsuario.Clear();
            txtPassword.Clear();
            cboPerfil.SelectedIndex = -1;

            txtDniCuit.Focus();
        }

        /// <summary>Carga en el formulario los datos del usuario seleccionado para editarlo.</summary>
        private void CargarEnFormulario(UsuarioEditModel m)
        {
            _original = m;

            lblTituloForm.Text = "Editar usuario";
            lblPasswordAyuda.Text = "Dejar en blanco para no cambiarla.";

            txtDniCuit.Text = m.DniCuit;
            txtNombre.Text = m.Nombre;
            txtApellido.Text = m.Apellido;
            txtDireccion.Text = m.Direccion;
            txtTelefono.Text = m.Telefono;
            txtEmail.Text = m.Email;

            if (m.FechaNacimiento.HasValue)
            {
                dtpFechaNac.Value = NegocioUsuario.AcotarFechaNacimiento(m.FechaNacimiento.Value);
                dtpFechaNac.Checked = true;
            }
            else
            {
                dtpFechaNac.Checked = false;
            }

            txtNombreUsuario.Text = m.NombreUsuario;
            txtPassword.Clear();
            cboPerfil.SelectedValue = m.IdPerfil;

            txtDniCuit.Focus();
        }

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

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            int? id = UsuarioSeleccionadoId();
            if (!id.HasValue) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                CargarEnFormulario(NegocioUsuario.ObtenerParaEdicion(id.Value));
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudieron cargar los datos del usuario.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                NegocioUsuario.Guardar(ArmarModelo(), SesionActual.Usuario.IdUsuario);

                LimpiarFormulario();
                CargarGrilla();
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
            LimpiarFormulario();
        }

        /// <summary>
        /// Eventos propios del formulario: filtran lo que se puede tipear en cada campo.
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

        // -----------------------------------------------------------------
        // Listado
        // -----------------------------------------------------------------

        private void CargarGrilla()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                int? idSeleccionado = UsuarioSeleccionadoId();

                DataTable dt = NegocioUsuario.Listar(activos: !chkVerInactivos.Checked);
                dgvUsuarios.DataSource = dt;

                if (dgvUsuarios.Columns.Contains("IdUsuario"))
                    dgvUsuarios.Columns["IdUsuario"].Visible = false;

                AplicarEncabezados();

                // Reposiciona la selección en una celda VISIBLE (nunca sobre la columna oculta).
                if (!(idSeleccionado.HasValue && SeleccionarFilaPorId(idSeleccionado.Value)))
                    SeleccionarPrimeraFila();

                ActualizarBotones();
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo leer el listado de usuarios.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void AplicarEncabezados()
        {
            SetHeader("DniCuit", "DNI/CUIT");
            SetHeader("Apellido", "Apellido");
            SetHeader("Nombre", "Nombre");
            SetHeader("Telefono", "Teléfono");
            SetHeader("Email", "Email");
            SetHeader("Usuario", "Usuario");
            SetHeader("Perfil", "Perfil");
            SetHeader("Activo", "Activo");
        }

        private void SetHeader(string columna, string texto)
        {
            if (dgvUsuarios.Columns.Contains(columna))
                dgvUsuarios.Columns[columna].HeaderText = texto;
        }

        private DataGridViewColumn PrimeraColumnaVisible()
        {
            foreach (DataGridViewColumn col in dgvUsuarios.Columns)
                if (col.Visible)
                    return col;
            return null;
        }

        private void SeleccionarPrimeraFila()
        {
            if (dgvUsuarios.Rows.Count == 0)
            {
                dgvUsuarios.ClearSelection();
                return;
            }

            var col = PrimeraColumnaVisible();
            dgvUsuarios.ClearSelection();
            dgvUsuarios.Rows[0].Selected = true;
            if (col != null)
                dgvUsuarios.CurrentCell = dgvUsuarios.Rows[0].Cells[col.Index];
        }

        private bool SeleccionarFilaPorId(int idUsuario)
        {
            var col = PrimeraColumnaVisible();
            foreach (DataGridViewRow fila in dgvUsuarios.Rows)
            {
                if (IdDeFila(fila) == idUsuario)
                {
                    dgvUsuarios.ClearSelection();
                    fila.Selected = true;
                    if (col != null)
                        dgvUsuarios.CurrentCell = fila.Cells[col.Index];
                    return true;
                }
            }
            return false;
        }

        // -----------------------------------------------------------------
        // Selección actual
        // -----------------------------------------------------------------

        private DataGridViewRow FilaSeleccionada()
        {
            if (dgvUsuarios.CurrentRow != null)
                return dgvUsuarios.CurrentRow;
            if (dgvUsuarios.SelectedRows.Count > 0)
                return dgvUsuarios.SelectedRows[0];
            return null;
        }

        /// <summary>Lee el id desde el DataRowView enlazado, sin depender de la columna oculta.</summary>
        private static int? IdDeFila(DataGridViewRow fila)
        {
            var drv = fila == null ? null : fila.DataBoundItem as DataRowView;
            if (drv == null || drv["IdUsuario"] == DBNull.Value)
                return null;
            return Convert.ToInt32(drv["IdUsuario"]);
        }

        private int? UsuarioSeleccionadoId()
        {
            return IdDeFila(FilaSeleccionada());
        }

        private string UsuarioSeleccionadoDescripcion()
        {
            var drv = FilaSeleccionada() == null ? null : FilaSeleccionada().DataBoundItem as DataRowView;
            if (drv == null) return string.Empty;
            return string.Format("{0}, {1} ({2})", drv["Apellido"], drv["Nombre"], drv["Usuario"]);
        }

        private void ActualizarBotones()
        {
            bool haySeleccion = UsuarioSeleccionadoId().HasValue;
            btnEditar.Enabled = haySeleccion;
            // Dar de baja solo tiene sentido mirando la lista de activos: un usuario
            // que ya está inactivo no se puede volver a dar de baja.
            btnBaja.Enabled = haySeleccion && !chkVerInactivos.Checked;
        }

        // -----------------------------------------------------------------
        // Acciones
        // -----------------------------------------------------------------

        private void btnBaja_Click(object sender, EventArgs e)
        {
            int? id = UsuarioSeleccionadoId();
            if (!id.HasValue) return;

            if (id.Value == SesionActual.Usuario.IdUsuario)
            {
                MessageBox.Show("No podés dar de baja tu propio usuario.",
                    "Acción no permitida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var r = MessageBox.Show(
                "¿Seguro que querés dar de baja a " + UsuarioSeleccionadoDescripcion() + "?",
                "Confirmar baja", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (r != DialogResult.Yes) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                NegocioUsuario.DarDeBaja(id.Value, SesionActual.Usuario.IdUsuario);

                // Si estaba en el formulario el usuario que acabamos de dar de baja,
                // no lo dejamos ahí a mitad de edición.
                if (!EsAlta && _original.IdUsuario == id.Value)
                    LimpiarFormulario();

                CargarGrilla();
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo dar de baja",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo dar de baja el usuario.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            CargarGrilla();
        }

        /// <summary>Alterna entre ver los usuarios activos o los dados de baja.</summary>
        private void chkVerInactivos_CheckedChanged(object sender, EventArgs e)
        {
            chkVerInactivos.Text = chkVerInactivos.Checked ? "Mostrando: Inactivos" : "Mostrando: Activos";
            CargarGrilla();
        }

        // Ambos eventos mantienen el estado de los botones: SelectionChanged cubre el
        // cambio de fila; CellClick cubre el clic sobre la fila que ya estaba seleccionada.
        private void dgvUsuarios_SelectionChanged(object sender, EventArgs e)
        {
            ActualizarBotones();
        }

        private void dgvUsuarios_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ActualizarBotones();
        }

        private void dgvUsuarios_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && btnEditar.Enabled)
                btnEditar_Click(sender, e);
        }
    }
}
