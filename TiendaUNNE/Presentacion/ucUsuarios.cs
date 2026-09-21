using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Sección de usuarios activos (Persona + Usuario + Perfil): formulario de alta/edición
    /// arriba, siempre visible, y listado abajo. Es un UserControl: se muestra dentro del
    /// panel de contenido de frmPrincipal, no en una ventana aparte.
    /// No hay botones "Nuevo"/"Editar": seleccionar una fila de la grilla carga ese usuario
    /// en el formulario; "Limpiar" lo deja listo para un alta.
    /// El formulario solo muestra datos y recoge lo tipeado: validar, recortar espacios,
    /// hashear la contraseña y decidir alta/edición es trabajo de NegocioUsuario.
    /// </summary>
    public partial class ucUsuarios : UserControl
    {
        private UsuarioEditModel _original;   // distinto de null = editando ese usuario
        private bool _actualizandoGrilla;      // true mientras CargarGrilla() reacomoda la selección
        private bool _verInactivos;            // false = activos, true = dados de baja

        // Ancho fijo del riel de acciones de la derecha (Buscar + tarjetas) y el margen
        // que se le deja antes; las 3 columnas del formulario se reparten todo lo demás.
        private const int RailAncho = 260;
        private const int RailMargen = 24;
        private const int FormMargen = 12;
        private const int FormGapColumna = 14;
        private const int BotonesSeparacion = 8;

        // Colores del título: neutro para un alta, ámbar para avisar que se está
        // modificando algo que ya existe.
        private static readonly System.Drawing.Color ColorTituloAlta =
            System.Drawing.Color.FromArgb(45, 48, 54);
        private static readonly System.Drawing.Color ColorTituloEdicion =
            System.Drawing.Color.FromArgb(217, 119, 6);

        // Campos que Negocio dijo que faltaban: se les dibuja un contorno rojo hasta que
        // la persona empieza a completarlos.
        private readonly HashSet<Control> _camposIncompletos = new HashSet<Control>();
        private static readonly Color ColorCampoIncompleto = Color.FromArgb(220, 38, 38);

        public ucUsuarios()
        {
            InitializeComponent();
            panelFormulario.Resize += (s, e) => ReubicarFormulario();
            panelFormulario.Paint += panelFormulario_Paint;

            txtDniCuit.TextChanged += CampoModificado;
            txtNombre.TextChanged += CampoModificado;
            txtApellido.TextChanged += CampoModificado;
            txtPassword.TextChanged += CampoModificado;
            cboPerfil.SelectedIndexChanged += CampoModificado;
        }

        // -----------------------------------------------------------------
        // Campos incompletos: contorno rojo
        // -----------------------------------------------------------------

        /// <summary>A qué control del formulario corresponde cada campo que señala Negocio.</summary>
        private Control ControlDeCampo(CampoUsuario campo)
        {
            switch (campo)
            {
                case CampoUsuario.Dni: return txtDniCuit;
                case CampoUsuario.Nombre: return txtNombre;
                case CampoUsuario.Apellido: return txtApellido;
                case CampoUsuario.Perfil: return cboPerfil;
                default: return txtPassword;
            }
        }

        private void MarcarCamposIncompletos(IReadOnlyList<CampoUsuario> campos)
        {
            _camposIncompletos.Clear();
            foreach (CampoUsuario campo in campos)
                _camposIncompletos.Add(ControlDeCampo(campo));

            panelFormulario.Invalidate();
            panelFormulario.Update();   // que el rojo se vea ya, detrás del cartel
        }

        private void QuitarMarcasDeCampos()
        {
            if (_camposIncompletos.Count == 0) return;

            _camposIncompletos.Clear();
            panelFormulario.Invalidate();
        }

        /// <summary>Apenas se toca un campo marcado, deja de estar en rojo.</summary>
        private void CampoModificado(object sender, EventArgs e)
        {
            if (_camposIncompletos.Remove((Control)sender))
                panelFormulario.Invalidate();
        }

        private void panelFormulario_Paint(object sender, PaintEventArgs e)
        {
            if (_camposIncompletos.Count == 0) return;

            using (var lapiz = new Pen(ColorCampoIncompleto, 2f))
            {
                foreach (Control campo in _camposIncompletos)
                {
                    Rectangle borde = campo.Bounds;
                    borde.Inflate(1, 1);
                    e.Graphics.DrawRectangle(lapiz, borde);
                }
            }
        }

        private bool EsAlta => _original == null;

        // -----------------------------------------------------------------
        // Atajos de teclado (ver AtajosTeclado para la lista completa)
        // -----------------------------------------------------------------

        /// <summary>
        /// Se intercepta acá y no en cada control porque Windows procesa Enter y Esc como
        /// teclas de diálogo antes de que el control llegue a verlas. Un UserControl no
        /// tiene AcceptButton/CancelButton como una ventana, por eso se resuelve así.
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Esc en el buscador: primero borra lo que se escribió, sin tocar el formulario.
            if (keyData == Keys.Escape && ActiveControl == txtBuscar && txtBuscar.Text.Length > 0)
            {
                txtBuscar.Clear();
                return true;
            }

            switch (AtajosTeclado.Interpretar(keyData, ActiveControl, panelFormulario, txtBuscar))
            {
                case AccionAtajo.Guardar:
                    btnGuardar.PerformClick();
                    return true;
                case AccionAtajo.Limpiar:
                    btnLimpiar.PerformClick();
                    return true;
                case AccionAtajo.Buscar:
                    txtBuscar.Focus();
                    txtBuscar.SelectAll();
                    return true;
                case AccionAtajo.Actualizar:
                    CargarGrilla();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ucUsuarios_Load(object sender, EventArgs e)
        {
            dtpFechaNac.MinDate = NegocioUsuario.FechaNacimientoMinima;
            dtpFechaNac.MaxDate = NegocioUsuario.FechaNacimientoMaxima;

            CargarPerfiles();
            LimpiarFormulario();
            ActualizarTarjetaBaja();
            CargarGrilla();
            ReubicarFormulario();
        }

        /// <summary>
        /// Reparte el ancho disponible del panel: las 3 columnas del formulario crecen
        /// parejo y el riel de acciones de la derecha queda siempre pegado al borde
        /// derecho. Se llama al cargar y cada vez que cambia el tamaño de la ventana.
        /// </summary>
        private void ReubicarFormulario()
        {
            int anchoPanel = panelFormulario.ClientSize.Width;
            if (anchoPanel <= 0) return;

            int railX = anchoPanel - RailAncho - FormMargen;
            int anchoForm = railX - RailMargen - FormMargen;
            int colAncho = Math.Max(90, (anchoForm - 2 * FormGapColumna) / 3);

            int col1 = FormMargen;
            int col2 = col1 + colAncho + FormGapColumna;
            int col3 = col2 + colAncho + FormGapColumna;

            // Si la ventana es demasiado angosta, el riel no se mete debajo de la columna 3.
            railX = Math.Max(railX, col3 + colAncho + RailMargen);

            UbicarColumna(lblDniCuit, txtDniCuit, col1, colAncho);
            UbicarColumna(lblNombre, txtNombre, col2, colAncho);
            UbicarColumna(lblApellido, txtApellido, col3, colAncho);

            UbicarColumna(lblDireccion, txtDireccion, col1, colAncho);
            UbicarColumna(lblTelefono, txtTelefono, col2, colAncho);
            UbicarColumna(lblEmail, txtEmail, col3, colAncho);

            UbicarColumna(lblFechaNac, dtpFechaNac, col1, colAncho);
            UbicarColumna(lblPassword, txtPassword, col2, colAncho);
            UbicarColumna(lblPerfil, cboPerfil, col3, colAncho);

            lblPasswordAyuda.Left = col2;

            // El título ocupa todo el ancho del formulario (sin invadir el riel de la derecha)
            // y se recorta con "…" si el texto de edición es más largo.
            lblTituloForm.Left = col1;
            lblTituloForm.Width = Math.Max(120, anchoForm);

            // Los botones van centrados bajo la columna 2, pero sin salirse por la izquierda
            // cuando la ventana es angosta.
            int anchoBotones = btnGuardar.Width + BotonesSeparacion + btnLimpiar.Width;
            int xBotones = Math.Max(col1, col2 + (colAncho - anchoBotones) / 2);
            btnGuardar.Left = xBotones;
            btnLimpiar.Left = xBotones + btnGuardar.Width + BotonesSeparacion;

            lblBuscar.Left = railX;
            txtBuscar.Left = railX;
            tarjetaVerInactivos.Left = railX;
            tarjetaBaja.Left = railX;
            tarjetaActualizar.Left = railX;
        }

        private static void UbicarColumna(Label etiqueta, Control campo, int x, int ancho)
        {
            etiqueta.Left = x;
            campo.Left = x;
            campo.Width = ancho;
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

        /// <summary>
        /// Hace evidente en qué modo está el formulario. Antes solo cambiaba un título chico y
        /// el único modo de volver a cargar un usuario nuevo era un botón llamado "Limpiar",
        /// que no da ninguna pista de eso. Ahora el título, su color y los dos botones cuentan
        /// lo mismo: mientras se edita, el segundo botón se llama "Nuevo usuario".
        /// </summary>
        private void ActualizarModoFormulario()
        {
            if (EsAlta)
            {
                lblTituloForm.Text = "Nuevo usuario";
                lblTituloForm.ForeColor = ColorTituloAlta;
                lblPasswordAyuda.Text = "Obligatoria.";
                btnGuardar.Text = "&Guardar usuario";
                btnLimpiar.Text = "&Limpiar campos";
            }
            else
            {
                lblTituloForm.Text = string.Format(
                    "Editando a {0}, {1}   ·   Para cargar uno nuevo tocá «Nuevo usuario»",
                    _original.Apellido, _original.Nombre);
                lblTituloForm.ForeColor = ColorTituloEdicion;
                lblPasswordAyuda.Text = "Dejar en blanco para no cambiarla.";
                btnGuardar.Text = "&Guardar cambios";
                btnLimpiar.Text = "N&uevo usuario";
            }
        }

        /// <summary>Deja el formulario listo para cargar un usuario nuevo y sin nada seleccionado.</summary>
        private void LimpiarFormulario()
        {
            _original = null;

            ActualizarModoFormulario();

            txtDniCuit.Clear();
            txtNombre.Clear();
            txtApellido.Clear();
            txtDireccion.Clear();
            txtTelefono.Clear();
            txtEmail.Clear();
            dtpFechaNac.Checked = false;
            txtPassword.Clear();
            cboPerfil.SelectedIndex = -1;

            // ClearSelection() sola no borra CurrentRow: entre esta línea y la siguiente,
            // la grilla dispararía SelectionChanged todavía "viendo" la fila vieja, y como
            // _original ya es null en ese instante, CargarSeleccionEnFormulario la volvería
            // a cargar y pisaría el vaciado que acabamos de hacer. Se suprime ese evento
            // intermedio con la misma bandera que usa CargarGrilla().
            _actualizandoGrilla = true;
            dgvUsuarios.ClearSelection();
            dgvUsuarios.CurrentCell = null;
            _actualizandoGrilla = false;

            QuitarMarcasDeCampos();
            ActualizarBotones();
            txtDniCuit.Focus();
        }

        /// <summary>Carga en el formulario los datos del usuario seleccionado para editarlo.</summary>
        private void CargarEnFormulario(UsuarioEditModel m)
        {
            _original = m;

            ActualizarModoFormulario();

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

            txtPassword.Clear();
            cboPerfil.SelectedValue = m.IdPerfil;
        }

        /// <summary>Trae del servidor y carga el usuario que está seleccionado en la grilla.</summary>
        private void CargarSeleccionEnFormulario()
        {
            int? id = UsuarioSeleccionadoId();
            if (!id.HasValue)
                return;

            // Ya está cargado (p.ej. la grilla se refrescó pero la selección no cambió): no repetir el viaje.
            if (!EsAlta && _original.IdUsuario == id.Value)
                return;

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

                PasswordPlano = txtPassword.Text,   // vacío en edición = no cambiar
                IdPerfil = perfil == null ? 0 : perfil.Id,
                NombrePerfil = perfil == null ? null : perfil.Nombre
            };
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
            catch (CamposIncompletosException ex)
            {
                MarcarCamposIncompletos(ex.Campos);
                ControlDeCampo(ex.Campos[0]).Focus();

                MessageBox.Show(ex.Message, "Campos incompletos",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        /// <summary>
        /// Eventos propios del formulario: filtran lo que se puede tipear en cada campo.
        /// Qué carácter es aceptable lo decide Negocio, acá solo se aplica.
        /// </summary>
        private void txtDniCuit_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !Validaciones.EsCaracterDniValido(e.KeyChar);
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

        // -----------------------------------------------------------------
        // Listado
        // -----------------------------------------------------------------

        private void CargarGrilla()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                _actualizandoGrilla = true;

                int? idSeleccionado = UsuarioSeleccionadoId();

                DataTable dt = NegocioUsuario.Listar(activos: !_verInactivos);
                dgvUsuarios.DataSource = dt;

                if (dgvUsuarios.Columns.Contains("IdUsuario"))
                    dgvUsuarios.Columns["IdUsuario"].Visible = false;

                AplicarEncabezados();
                AplicarFiltroBusqueda();

                // Si había algo seleccionado (y sigue existiendo) se reselecciona; si no,
                // la grilla queda sin selección y el formulario tal como esté.
                if (!(idSeleccionado.HasValue && SeleccionarFilaPorId(idSeleccionado.Value)))
                {
                    dgvUsuarios.ClearSelection();
                    dgvUsuarios.CurrentCell = null;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo leer el listado de usuarios.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _actualizandoGrilla = false;
                Cursor = Cursors.Default;
            }

            ActualizarBotones();
            CargarSeleccionEnFormulario();
        }

        private void AplicarEncabezados()
        {
            SetHeader("DniCuit", "DNI");
            SetHeader("Apellido", "Apellido");
            SetHeader("Nombre", "Nombre");
            SetHeader("Telefono", "Teléfono");
            SetHeader("Email", "Email");
            SetHeader("Perfil", "Perfil");
            SetHeader("Activo", "Activo");
        }

        private void SetHeader(string columna, string texto)
        {
            if (dgvUsuarios.Columns.Contains(columna))
                dgvUsuarios.Columns[columna].HeaderText = texto;
        }

        /// <summary>Filtra la grilla por lo tipeado en Buscar (DNI, apellido, nombre o email).</summary>
        private void AplicarFiltroBusqueda()
        {
            var dt = dgvUsuarios.DataSource as DataTable;
            if (dt == null) return;

            dt.DefaultView.RowFilter = FiltroGrilla.Contiene(
                txtBuscar.Text, "DniCuit", "Apellido", "Nombre", "Email");
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltroBusqueda();
        }

        private DataGridViewColumn PrimeraColumnaVisible()
        {
            foreach (DataGridViewColumn col in dgvUsuarios.Columns)
                if (col.Visible)
                    return col;
            return null;
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
            return string.Format("{0}, {1} (DNI {2})", drv["Apellido"], drv["Nombre"], drv["DniCuit"]);
        }

        private void ActualizarBotones()
        {
            bool haySeleccion = UsuarioSeleccionadoId().HasValue;
            tarjetaBaja.Enabled = haySeleccion;
        }

        /// <summary>Pone la tarjeta en modo "Dar de baja" o "Dar de alta" según lo que se está viendo.</summary>
        private void ActualizarTarjetaBaja()
        {
            if (_verInactivos)
            {
                tarjetaBaja.Icono = IconoAccion.Alta;
                tarjetaBaja.Titulo = "Dar de alta";
                tarjetaBaja.Descripcion = "Reactiva al usuario seleccionado";
            }
            else
            {
                tarjetaBaja.Icono = IconoAccion.Baja;
                tarjetaBaja.Titulo = "Dar de baja";
                tarjetaBaja.Descripcion = "Al usuario seleccionado";
            }
            tarjetaBaja.Invalidate();
        }

        // -----------------------------------------------------------------
        // Acciones
        // -----------------------------------------------------------------

        private void tarjetaBaja_Click(object sender, EventArgs e)
        {
            if (_verInactivos)
                DarDeAlta();
            else
                DarDeBaja();
        }

        private void DarDeBaja()
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

        private void DarDeAlta()
        {
            int? id = UsuarioSeleccionadoId();
            if (!id.HasValue) return;

            var r = MessageBox.Show(
                "¿Reactivar a " + UsuarioSeleccionadoDescripcion() + "?",
                "Confirmar alta", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (r != DialogResult.Yes) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                NegocioUsuario.DarDeAlta(id.Value, SesionActual.Usuario.IdUsuario);
                CargarGrilla();
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo dar de alta",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo dar de alta el usuario.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void tarjetaActualizar_Click(object sender, EventArgs e)
        {
            CargarGrilla();
        }

        /// <summary>Alterna entre ver los usuarios activos o los dados de baja.</summary>
        private void tarjetaVerInactivos_Click(object sender, EventArgs e)
        {
            _verInactivos = !_verInactivos;
            tarjetaVerInactivos.Activa = _verInactivos;
            tarjetaVerInactivos.Titulo = _verInactivos ? "Mostrando: Inactivos" : "Mostrando: Activos";
            tarjetaVerInactivos.Descripcion = _verInactivos
                ? "Tocá para ver los activos"
                : "Tocá para ver los dados de baja";
            tarjetaVerInactivos.Invalidate();
            ActualizarTarjetaBaja();
            CargarGrilla();
        }

        private void dgvUsuarios_SelectionChanged(object sender, EventArgs e)
        {
            if (_actualizandoGrilla) return;   // evita recargar el formulario en medio de un refresco

            ActualizarBotones();
            CargarSeleccionEnFormulario();
        }
    }
}
