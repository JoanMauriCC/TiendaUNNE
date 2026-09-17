namespace TiendaUNNE
{
    partial class ucUsuarios
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.panelFormulario = new System.Windows.Forms.Panel();
            this.lblTituloForm = new System.Windows.Forms.Label();
            this.lblDniCuit = new System.Windows.Forms.Label();
            this.txtDniCuit = new System.Windows.Forms.TextBox();
            this.lblNombre = new System.Windows.Forms.Label();
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.lblApellido = new System.Windows.Forms.Label();
            this.txtApellido = new System.Windows.Forms.TextBox();
            this.lblDireccion = new System.Windows.Forms.Label();
            this.txtDireccion = new System.Windows.Forms.TextBox();
            this.lblTelefono = new System.Windows.Forms.Label();
            this.txtTelefono = new System.Windows.Forms.TextBox();
            this.lblEmail = new System.Windows.Forms.Label();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.lblFechaNac = new System.Windows.Forms.Label();
            this.dtpFechaNac = new System.Windows.Forms.DateTimePicker();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblPerfil = new System.Windows.Forms.Label();
            this.cboPerfil = new System.Windows.Forms.ComboBox();
            this.lblPasswordAyuda = new System.Windows.Forms.Label();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnLimpiar = new System.Windows.Forms.Button();
            this.lblBuscar = new System.Windows.Forms.Label();
            this.txtBuscar = new System.Windows.Forms.TextBox();
            this.tarjetaVerInactivos = new TiendaUNNE.TarjetaAccion();
            this.tarjetaBaja = new TiendaUNNE.TarjetaAccion();
            this.tarjetaActualizar = new TiendaUNNE.TarjetaAccion();
            this.dgvUsuarios = new System.Windows.Forms.DataGridView();
            this.panelFormulario.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsuarios)).BeginInit();
            this.SuspendLayout();
            //
            // panelFormulario
            //
            this.panelFormulario.BackColor = System.Drawing.Color.White;
            this.panelFormulario.Controls.Add(this.lblTituloForm);
            this.panelFormulario.Controls.Add(this.lblDniCuit);
            this.panelFormulario.Controls.Add(this.txtDniCuit);
            this.panelFormulario.Controls.Add(this.lblNombre);
            this.panelFormulario.Controls.Add(this.txtNombre);
            this.panelFormulario.Controls.Add(this.lblApellido);
            this.panelFormulario.Controls.Add(this.txtApellido);
            this.panelFormulario.Controls.Add(this.lblDireccion);
            this.panelFormulario.Controls.Add(this.txtDireccion);
            this.panelFormulario.Controls.Add(this.lblTelefono);
            this.panelFormulario.Controls.Add(this.txtTelefono);
            this.panelFormulario.Controls.Add(this.lblEmail);
            this.panelFormulario.Controls.Add(this.txtEmail);
            this.panelFormulario.Controls.Add(this.lblFechaNac);
            this.panelFormulario.Controls.Add(this.dtpFechaNac);
            this.panelFormulario.Controls.Add(this.lblPassword);
            this.panelFormulario.Controls.Add(this.txtPassword);
            this.panelFormulario.Controls.Add(this.lblPerfil);
            this.panelFormulario.Controls.Add(this.cboPerfil);
            this.panelFormulario.Controls.Add(this.lblPasswordAyuda);
            this.panelFormulario.Controls.Add(this.btnGuardar);
            this.panelFormulario.Controls.Add(this.btnLimpiar);
            this.panelFormulario.Controls.Add(this.lblBuscar);
            this.panelFormulario.Controls.Add(this.txtBuscar);
            this.panelFormulario.Controls.Add(this.tarjetaVerInactivos);
            this.panelFormulario.Controls.Add(this.tarjetaBaja);
            this.panelFormulario.Controls.Add(this.tarjetaActualizar);
            this.panelFormulario.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFormulario.Location = new System.Drawing.Point(0, 0);
            this.panelFormulario.Name = "panelFormulario";
            this.panelFormulario.Size = new System.Drawing.Size(844, 244);
            this.panelFormulario.TabIndex = 0;
            //
            // lblTituloForm
            //
            this.lblTituloForm.AutoSize = true;
            this.lblTituloForm.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblTituloForm.Location = new System.Drawing.Point(12, 8);
            this.lblTituloForm.Name = "lblTituloForm";
            this.lblTituloForm.Size = new System.Drawing.Size(107, 20);
            this.lblTituloForm.TabIndex = 0;
            this.lblTituloForm.Text = "Nuevo usuario";
            //
            // lblDniCuit
            //
            this.lblDniCuit.AutoSize = true;
            this.lblDniCuit.Location = new System.Drawing.Point(12, 36);
            this.lblDniCuit.Name = "lblDniCuit";
            this.lblDniCuit.Size = new System.Drawing.Size(37, 15);
            this.lblDniCuit.TabIndex = 1;
            this.lblDniCuit.Text = "DNI *";
            //
            // txtDniCuit
            //
            this.txtDniCuit.Location = new System.Drawing.Point(12, 52);
            this.txtDniCuit.MaxLength = 20;
            this.txtDniCuit.Name = "txtDniCuit";
            this.txtDniCuit.Size = new System.Drawing.Size(160, 23);
            this.txtDniCuit.TabIndex = 2;
            this.txtDniCuit.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtDniCuit_KeyPress);
            //
            // lblNombre
            //
            this.lblNombre.AutoSize = true;
            this.lblNombre.Location = new System.Drawing.Point(184, 36);
            this.lblNombre.Name = "lblNombre";
            this.lblNombre.Size = new System.Drawing.Size(56, 15);
            this.lblNombre.TabIndex = 3;
            this.lblNombre.Text = "Nombre *";
            //
            // txtNombre
            //
            this.txtNombre.Location = new System.Drawing.Point(184, 52);
            this.txtNombre.MaxLength = 100;
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.Size = new System.Drawing.Size(160, 23);
            this.txtNombre.TabIndex = 4;
            this.txtNombre.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtNombre_KeyPress);
            //
            // lblApellido
            //
            this.lblApellido.AutoSize = true;
            this.lblApellido.Location = new System.Drawing.Point(356, 36);
            this.lblApellido.Name = "lblApellido";
            this.lblApellido.Size = new System.Drawing.Size(58, 15);
            this.lblApellido.TabIndex = 5;
            this.lblApellido.Text = "Apellido *";
            //
            // txtApellido
            //
            this.txtApellido.Location = new System.Drawing.Point(356, 52);
            this.txtApellido.MaxLength = 100;
            this.txtApellido.Name = "txtApellido";
            this.txtApellido.Size = new System.Drawing.Size(160, 23);
            this.txtApellido.TabIndex = 6;
            this.txtApellido.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtApellido_KeyPress);
            //
            // lblDireccion
            //
            this.lblDireccion.AutoSize = true;
            this.lblDireccion.Location = new System.Drawing.Point(12, 86);
            this.lblDireccion.Name = "lblDireccion";
            this.lblDireccion.Size = new System.Drawing.Size(58, 15);
            this.lblDireccion.TabIndex = 7;
            this.lblDireccion.Text = "Dirección";
            //
            // txtDireccion
            //
            this.txtDireccion.Location = new System.Drawing.Point(12, 102);
            this.txtDireccion.MaxLength = 200;
            this.txtDireccion.Name = "txtDireccion";
            this.txtDireccion.Size = new System.Drawing.Size(160, 23);
            this.txtDireccion.TabIndex = 8;
            //
            // lblTelefono
            //
            this.lblTelefono.AutoSize = true;
            this.lblTelefono.Location = new System.Drawing.Point(184, 86);
            this.lblTelefono.Name = "lblTelefono";
            this.lblTelefono.Size = new System.Drawing.Size(52, 15);
            this.lblTelefono.TabIndex = 9;
            this.lblTelefono.Text = "Teléfono";
            //
            // txtTelefono
            //
            this.txtTelefono.Location = new System.Drawing.Point(184, 102);
            this.txtTelefono.MaxLength = 30;
            this.txtTelefono.Name = "txtTelefono";
            this.txtTelefono.Size = new System.Drawing.Size(160, 23);
            this.txtTelefono.TabIndex = 10;
            this.txtTelefono.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtTelefono_KeyPress);
            //
            // lblEmail
            //
            this.lblEmail.AutoSize = true;
            this.lblEmail.Location = new System.Drawing.Point(356, 86);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new System.Drawing.Size(39, 15);
            this.lblEmail.TabIndex = 11;
            this.lblEmail.Text = "Email";
            //
            // txtEmail
            //
            this.txtEmail.Location = new System.Drawing.Point(356, 102);
            this.txtEmail.MaxLength = 150;
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(160, 23);
            this.txtEmail.TabIndex = 12;
            this.txtEmail.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtEmail_KeyPress);
            //
            // lblFechaNac
            //
            this.lblFechaNac.AutoSize = true;
            this.lblFechaNac.Location = new System.Drawing.Point(12, 136);
            this.lblFechaNac.Name = "lblFechaNac";
            this.lblFechaNac.Size = new System.Drawing.Size(102, 15);
            this.lblFechaNac.TabIndex = 13;
            this.lblFechaNac.Text = "Fecha nacimiento";
            //
            // dtpFechaNac
            //
            this.dtpFechaNac.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFechaNac.Location = new System.Drawing.Point(12, 152);
            this.dtpFechaNac.Name = "dtpFechaNac";
            this.dtpFechaNac.ShowCheckBox = true;
            this.dtpFechaNac.Size = new System.Drawing.Size(160, 23);
            this.dtpFechaNac.TabIndex = 14;
            //
            // lblPassword
            //
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(184, 136);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(70, 15);
            this.lblPassword.TabIndex = 15;
            this.lblPassword.Text = "Contraseña";
            //
            // txtPassword
            //
            this.txtPassword.Location = new System.Drawing.Point(184, 152);
            this.txtPassword.MaxLength = 100;
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '●';
            this.txtPassword.Size = new System.Drawing.Size(160, 23);
            this.txtPassword.TabIndex = 16;
            //
            // lblPerfil
            //
            this.lblPerfil.AutoSize = true;
            this.lblPerfil.Location = new System.Drawing.Point(356, 136);
            this.lblPerfil.Name = "lblPerfil";
            this.lblPerfil.Size = new System.Drawing.Size(45, 15);
            this.lblPerfil.TabIndex = 17;
            this.lblPerfil.Text = "Perfil *";
            //
            // cboPerfil
            //
            this.cboPerfil.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPerfil.FormattingEnabled = true;
            this.cboPerfil.Location = new System.Drawing.Point(356, 152);
            this.cboPerfil.Name = "cboPerfil";
            this.cboPerfil.Size = new System.Drawing.Size(160, 23);
            this.cboPerfil.TabIndex = 18;
            //
            // lblPasswordAyuda
            //
            this.lblPasswordAyuda.AutoSize = true;
            this.lblPasswordAyuda.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblPasswordAyuda.Location = new System.Drawing.Point(184, 178);
            this.lblPasswordAyuda.Name = "lblPasswordAyuda";
            this.lblPasswordAyuda.Size = new System.Drawing.Size(62, 15);
            this.lblPasswordAyuda.TabIndex = 19;
            this.lblPasswordAyuda.Text = "Obligatoria.";
            //
            // btnGuardar
            //
            // Centrado bajo la columna 2 (Contraseña/Perfil), en su propia fila para que el
            // texto de ayuda de arriba nunca lo tape, sin importar cuánto mida ese texto.
            this.btnGuardar.Location = new System.Drawing.Point(184, 202);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(77, 30);
            this.btnGuardar.TabIndex = 20;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            //
            // btnLimpiar
            //
            this.btnLimpiar.Location = new System.Drawing.Point(267, 202);
            this.btnLimpiar.Name = "btnLimpiar";
            this.btnLimpiar.Size = new System.Drawing.Size(77, 30);
            this.btnLimpiar.TabIndex = 21;
            this.btnLimpiar.Text = "Limpiar";
            this.btnLimpiar.UseVisualStyleBackColor = true;
            this.btnLimpiar.Click += new System.EventHandler(this.btnLimpiar_Click);
            //
            // lblBuscar
            //
            this.lblBuscar.AutoSize = true;
            this.lblBuscar.Location = new System.Drawing.Point(560, 8);
            this.lblBuscar.Name = "lblBuscar";
            this.lblBuscar.Size = new System.Drawing.Size(45, 15);
            this.lblBuscar.TabIndex = 22;
            this.lblBuscar.Text = "Buscar";
            //
            // txtBuscar
            //
            this.txtBuscar.Location = new System.Drawing.Point(560, 26);
            this.txtBuscar.Name = "txtBuscar";
            this.txtBuscar.Size = new System.Drawing.Size(260, 23);
            this.txtBuscar.TabIndex = 23;
            this.txtBuscar.TextChanged += new System.EventHandler(this.txtBuscar_TextChanged);
            //
            // tarjetaVerInactivos
            //
            this.tarjetaVerInactivos.Icono = TiendaUNNE.IconoAccion.Ver;
            this.tarjetaVerInactivos.Titulo = "Mostrando: Activos";
            this.tarjetaVerInactivos.Descripcion = "Tocá para ver los dados de baja";
            this.tarjetaVerInactivos.Location = new System.Drawing.Point(560, 58);
            this.tarjetaVerInactivos.Name = "tarjetaVerInactivos";
            this.tarjetaVerInactivos.Size = new System.Drawing.Size(260, 44);
            this.tarjetaVerInactivos.TabIndex = 24;
            this.tarjetaVerInactivos.Click += new System.EventHandler(this.tarjetaVerInactivos_Click);
            //
            // tarjetaBaja
            //
            this.tarjetaBaja.Icono = TiendaUNNE.IconoAccion.Baja;
            this.tarjetaBaja.Titulo = "Dar de baja";
            this.tarjetaBaja.Descripcion = "Al usuario seleccionado";
            this.tarjetaBaja.Location = new System.Drawing.Point(560, 110);
            this.tarjetaBaja.Name = "tarjetaBaja";
            this.tarjetaBaja.Size = new System.Drawing.Size(260, 44);
            this.tarjetaBaja.TabIndex = 25;
            this.tarjetaBaja.Click += new System.EventHandler(this.tarjetaBaja_Click);
            //
            // tarjetaActualizar
            //
            this.tarjetaActualizar.Icono = TiendaUNNE.IconoAccion.Actualizar;
            this.tarjetaActualizar.Titulo = "Actualizar listado";
            this.tarjetaActualizar.Descripcion = "Vuelve a leer la grilla";
            this.tarjetaActualizar.Location = new System.Drawing.Point(560, 162);
            this.tarjetaActualizar.Name = "tarjetaActualizar";
            this.tarjetaActualizar.Size = new System.Drawing.Size(260, 44);
            this.tarjetaActualizar.TabIndex = 26;
            this.tarjetaActualizar.Click += new System.EventHandler(this.tarjetaActualizar_Click);
            //
            // dgvUsuarios
            //
            this.dgvUsuarios.AllowUserToAddRows = false;
            this.dgvUsuarios.AllowUserToDeleteRows = false;
            this.dgvUsuarios.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvUsuarios.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUsuarios.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvUsuarios.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgvUsuarios.Location = new System.Drawing.Point(0, 244);
            this.dgvUsuarios.MultiSelect = false;
            this.dgvUsuarios.Name = "dgvUsuarios";
            this.dgvUsuarios.ReadOnly = true;
            this.dgvUsuarios.RowHeadersVisible = false;
            this.dgvUsuarios.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvUsuarios.Size = new System.Drawing.Size(844, 257);
            this.dgvUsuarios.TabIndex = 1;
            this.dgvUsuarios.SelectionChanged += new System.EventHandler(this.dgvUsuarios_SelectionChanged);
            //
            // ucUsuarios
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dgvUsuarios);
            this.Controls.Add(this.panelFormulario);
            this.Name = "ucUsuarios";
            this.Size = new System.Drawing.Size(844, 501);
            this.Load += new System.EventHandler(this.ucUsuarios_Load);
            this.panelFormulario.ResumeLayout(false);
            this.panelFormulario.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsuarios)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelFormulario;
        private System.Windows.Forms.Label lblTituloForm;
        private System.Windows.Forms.Label lblDniCuit;
        private System.Windows.Forms.TextBox txtDniCuit;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.Label lblApellido;
        private System.Windows.Forms.TextBox txtApellido;
        private System.Windows.Forms.Label lblDireccion;
        private System.Windows.Forms.TextBox txtDireccion;
        private System.Windows.Forms.Label lblTelefono;
        private System.Windows.Forms.TextBox txtTelefono;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Label lblFechaNac;
        private System.Windows.Forms.DateTimePicker dtpFechaNac;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblPerfil;
        private System.Windows.Forms.ComboBox cboPerfil;
        private System.Windows.Forms.Label lblPasswordAyuda;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnLimpiar;
        private System.Windows.Forms.Label lblBuscar;
        private System.Windows.Forms.TextBox txtBuscar;
        private TarjetaAccion tarjetaVerInactivos;
        private TarjetaAccion tarjetaBaja;
        private TarjetaAccion tarjetaActualizar;
        private System.Windows.Forms.DataGridView dgvUsuarios;
    }
}
