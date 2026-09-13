namespace TiendaUNNE
{
    partial class frmUsuarioEditor
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
            this.grpPersona = new System.Windows.Forms.GroupBox();
            this.dtpFechaNac = new System.Windows.Forms.DateTimePicker();
            this.lblFechaNac = new System.Windows.Forms.Label();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.lblEmail = new System.Windows.Forms.Label();
            this.txtTelefono = new System.Windows.Forms.TextBox();
            this.lblTelefono = new System.Windows.Forms.Label();
            this.txtDireccion = new System.Windows.Forms.TextBox();
            this.lblDireccion = new System.Windows.Forms.Label();
            this.txtApellido = new System.Windows.Forms.TextBox();
            this.lblApellido = new System.Windows.Forms.Label();
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.lblNombre = new System.Windows.Forms.Label();
            this.txtDniCuit = new System.Windows.Forms.TextBox();
            this.lblDniCuit = new System.Windows.Forms.Label();
            this.grpUsuario = new System.Windows.Forms.GroupBox();
            this.cboPerfil = new System.Windows.Forms.ComboBox();
            this.lblPerfil = new System.Windows.Forms.Label();
            this.lblPasswordAyuda = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtNombreUsuario = new System.Windows.Forms.TextBox();
            this.lblNombreUsuario = new System.Windows.Forms.Label();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.grpPersona.SuspendLayout();
            this.grpUsuario.SuspendLayout();
            this.SuspendLayout();
            //
            // grpPersona
            //
            this.grpPersona.Controls.Add(this.dtpFechaNac);
            this.grpPersona.Controls.Add(this.lblFechaNac);
            this.grpPersona.Controls.Add(this.txtEmail);
            this.grpPersona.Controls.Add(this.lblEmail);
            this.grpPersona.Controls.Add(this.txtTelefono);
            this.grpPersona.Controls.Add(this.lblTelefono);
            this.grpPersona.Controls.Add(this.txtDireccion);
            this.grpPersona.Controls.Add(this.lblDireccion);
            this.grpPersona.Controls.Add(this.txtApellido);
            this.grpPersona.Controls.Add(this.lblApellido);
            this.grpPersona.Controls.Add(this.txtNombre);
            this.grpPersona.Controls.Add(this.lblNombre);
            this.grpPersona.Controls.Add(this.txtDniCuit);
            this.grpPersona.Controls.Add(this.lblDniCuit);
            this.grpPersona.Location = new System.Drawing.Point(12, 12);
            this.grpPersona.Name = "grpPersona";
            this.grpPersona.Size = new System.Drawing.Size(410, 262);
            this.grpPersona.TabIndex = 0;
            this.grpPersona.TabStop = false;
            this.grpPersona.Text = "Persona";
            //
            // dtpFechaNac
            //
            this.dtpFechaNac.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFechaNac.Location = new System.Drawing.Point(118, 220);
            this.dtpFechaNac.Name = "dtpFechaNac";
            this.dtpFechaNac.ShowCheckBox = true;
            this.dtpFechaNac.Size = new System.Drawing.Size(170, 23);
            this.dtpFechaNac.TabIndex = 13;
            //
            // lblFechaNac
            //
            this.lblFechaNac.AutoSize = true;
            this.lblFechaNac.Location = new System.Drawing.Point(12, 224);
            this.lblFechaNac.Name = "lblFechaNac";
            this.lblFechaNac.Size = new System.Drawing.Size(102, 15);
            this.lblFechaNac.TabIndex = 12;
            this.lblFechaNac.Text = "Fecha nacimiento";
            //
            // txtEmail
            //
            this.txtEmail.Location = new System.Drawing.Point(118, 188);
            this.txtEmail.MaxLength = 150;
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(278, 23);
            this.txtEmail.TabIndex = 11;
            //
            // lblEmail
            //
            this.lblEmail.AutoSize = true;
            this.lblEmail.Location = new System.Drawing.Point(12, 192);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new System.Drawing.Size(39, 15);
            this.lblEmail.TabIndex = 10;
            this.lblEmail.Text = "Email";
            //
            // txtTelefono
            //
            this.txtTelefono.Location = new System.Drawing.Point(118, 156);
            this.txtTelefono.MaxLength = 30;
            this.txtTelefono.Name = "txtTelefono";
            this.txtTelefono.Size = new System.Drawing.Size(278, 23);
            this.txtTelefono.TabIndex = 9;
            this.txtTelefono.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtTelefono_KeyPress);
            //
            // lblTelefono
            //
            this.lblTelefono.AutoSize = true;
            this.lblTelefono.Location = new System.Drawing.Point(12, 160);
            this.lblTelefono.Name = "lblTelefono";
            this.lblTelefono.Size = new System.Drawing.Size(52, 15);
            this.lblTelefono.TabIndex = 8;
            this.lblTelefono.Text = "Teléfono";
            //
            // txtDireccion
            //
            this.txtDireccion.Location = new System.Drawing.Point(118, 124);
            this.txtDireccion.MaxLength = 200;
            this.txtDireccion.Name = "txtDireccion";
            this.txtDireccion.Size = new System.Drawing.Size(278, 23);
            this.txtDireccion.TabIndex = 7;
            //
            // lblDireccion
            //
            this.lblDireccion.AutoSize = true;
            this.lblDireccion.Location = new System.Drawing.Point(12, 128);
            this.lblDireccion.Name = "lblDireccion";
            this.lblDireccion.Size = new System.Drawing.Size(58, 15);
            this.lblDireccion.TabIndex = 6;
            this.lblDireccion.Text = "Dirección";
            //
            // txtApellido
            //
            this.txtApellido.Location = new System.Drawing.Point(118, 92);
            this.txtApellido.MaxLength = 100;
            this.txtApellido.Name = "txtApellido";
            this.txtApellido.Size = new System.Drawing.Size(278, 23);
            this.txtApellido.TabIndex = 5;
            //
            // lblApellido
            //
            this.lblApellido.AutoSize = true;
            this.lblApellido.Location = new System.Drawing.Point(12, 96);
            this.lblApellido.Name = "lblApellido";
            this.lblApellido.Size = new System.Drawing.Size(52, 15);
            this.lblApellido.TabIndex = 4;
            this.lblApellido.Text = "Apellido *";
            //
            // txtNombre
            //
            this.txtNombre.Location = new System.Drawing.Point(118, 60);
            this.txtNombre.MaxLength = 100;
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.Size = new System.Drawing.Size(278, 23);
            this.txtNombre.TabIndex = 3;
            //
            // lblNombre
            //
            this.lblNombre.AutoSize = true;
            this.lblNombre.Location = new System.Drawing.Point(12, 64);
            this.lblNombre.Name = "lblNombre";
            this.lblNombre.Size = new System.Drawing.Size(52, 15);
            this.lblNombre.TabIndex = 2;
            this.lblNombre.Text = "Nombre *";
            //
            // txtDniCuit
            //
            this.txtDniCuit.Location = new System.Drawing.Point(118, 28);
            this.txtDniCuit.MaxLength = 20;
            this.txtDniCuit.Name = "txtDniCuit";
            this.txtDniCuit.Size = new System.Drawing.Size(278, 23);
            this.txtDniCuit.TabIndex = 1;
            this.txtDniCuit.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtDniCuit_KeyPress);
            //
            // lblDniCuit
            //
            this.lblDniCuit.AutoSize = true;
            this.lblDniCuit.Location = new System.Drawing.Point(12, 32);
            this.lblDniCuit.Name = "lblDniCuit";
            this.lblDniCuit.Size = new System.Drawing.Size(66, 15);
            this.lblDniCuit.TabIndex = 0;
            this.lblDniCuit.Text = "DNI/CUIT *";
            //
            // grpUsuario
            //
            this.grpUsuario.Controls.Add(this.cboPerfil);
            this.grpUsuario.Controls.Add(this.lblPerfil);
            this.grpUsuario.Controls.Add(this.lblPasswordAyuda);
            this.grpUsuario.Controls.Add(this.txtPassword);
            this.grpUsuario.Controls.Add(this.lblPassword);
            this.grpUsuario.Controls.Add(this.txtNombreUsuario);
            this.grpUsuario.Controls.Add(this.lblNombreUsuario);
            this.grpUsuario.Location = new System.Drawing.Point(12, 280);
            this.grpUsuario.Name = "grpUsuario";
            this.grpUsuario.Size = new System.Drawing.Size(410, 176);
            this.grpUsuario.TabIndex = 1;
            this.grpUsuario.TabStop = false;
            this.grpUsuario.Text = "Usuario";
            //
            // cboPerfil
            //
            this.cboPerfil.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPerfil.FormattingEnabled = true;
            this.cboPerfil.Location = new System.Drawing.Point(118, 128);
            this.cboPerfil.Name = "cboPerfil";
            this.cboPerfil.Size = new System.Drawing.Size(220, 23);
            this.cboPerfil.TabIndex = 6;
            //
            // lblPerfil
            //
            this.lblPerfil.AutoSize = true;
            this.lblPerfil.Location = new System.Drawing.Point(12, 132);
            this.lblPerfil.Name = "lblPerfil";
            this.lblPerfil.Size = new System.Drawing.Size(45, 15);
            this.lblPerfil.TabIndex = 5;
            this.lblPerfil.Text = "Perfil *";
            //
            // lblPasswordAyuda
            //
            this.lblPasswordAyuda.AutoSize = true;
            this.lblPasswordAyuda.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblPasswordAyuda.Location = new System.Drawing.Point(118, 92);
            this.lblPasswordAyuda.Name = "lblPasswordAyuda";
            this.lblPasswordAyuda.Size = new System.Drawing.Size(62, 15);
            this.lblPasswordAyuda.TabIndex = 4;
            this.lblPasswordAyuda.Text = "Obligatoria.";
            //
            // txtPassword
            //
            this.txtPassword.Location = new System.Drawing.Point(118, 64);
            this.txtPassword.MaxLength = 100;
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '●';
            this.txtPassword.Size = new System.Drawing.Size(220, 23);
            this.txtPassword.TabIndex = 3;
            //
            // lblPassword
            //
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(12, 68);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(70, 15);
            this.lblPassword.TabIndex = 2;
            this.lblPassword.Text = "Contraseña";
            //
            // txtNombreUsuario
            //
            this.txtNombreUsuario.Location = new System.Drawing.Point(118, 28);
            this.txtNombreUsuario.MaxLength = 50;
            this.txtNombreUsuario.Name = "txtNombreUsuario";
            this.txtNombreUsuario.Size = new System.Drawing.Size(220, 23);
            this.txtNombreUsuario.TabIndex = 1;
            //
            // lblNombreUsuario
            //
            this.lblNombreUsuario.AutoSize = true;
            this.lblNombreUsuario.Location = new System.Drawing.Point(12, 32);
            this.lblNombreUsuario.Name = "lblNombreUsuario";
            this.lblNombreUsuario.Size = new System.Drawing.Size(97, 15);
            this.lblNombreUsuario.TabIndex = 0;
            this.lblNombreUsuario.Text = "Nombre usuario *";
            //
            // btnGuardar
            //
            this.btnGuardar.Location = new System.Drawing.Point(242, 466);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(85, 32);
            this.btnGuardar.TabIndex = 2;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            //
            // btnCancelar
            //
            this.btnCancelar.Location = new System.Drawing.Point(337, 466);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(85, 32);
            this.btnCancelar.TabIndex = 3;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            //
            // frmUsuarioEditor
            //
            this.AcceptButton = this.btnGuardar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancelar;
            this.ClientSize = new System.Drawing.Size(434, 510);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.grpUsuario);
            this.Controls.Add(this.grpPersona);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmUsuarioEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Usuario";
            this.Load += new System.EventHandler(this.frmUsuarioEditor_Load);
            this.grpPersona.ResumeLayout(false);
            this.grpPersona.PerformLayout();
            this.grpUsuario.ResumeLayout(false);
            this.grpUsuario.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox grpPersona;
        private System.Windows.Forms.DateTimePicker dtpFechaNac;
        private System.Windows.Forms.Label lblFechaNac;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.TextBox txtTelefono;
        private System.Windows.Forms.Label lblTelefono;
        private System.Windows.Forms.TextBox txtDireccion;
        private System.Windows.Forms.Label lblDireccion;
        private System.Windows.Forms.TextBox txtApellido;
        private System.Windows.Forms.Label lblApellido;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.TextBox txtDniCuit;
        private System.Windows.Forms.Label lblDniCuit;
        private System.Windows.Forms.GroupBox grpUsuario;
        private System.Windows.Forms.ComboBox cboPerfil;
        private System.Windows.Forms.Label lblPerfil;
        private System.Windows.Forms.Label lblPasswordAyuda;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtNombreUsuario;
        private System.Windows.Forms.Label lblNombreUsuario;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnCancelar;
    }
}
