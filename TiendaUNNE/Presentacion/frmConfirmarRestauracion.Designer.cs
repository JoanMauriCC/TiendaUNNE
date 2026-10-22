namespace TiendaUNNE
{
    partial class frmConfirmarRestauracion
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
            this.lblAlerta = new System.Windows.Forms.Label();
            this.lblArchivoTitulo = new System.Windows.Forms.Label();
            this.lblArchivo = new System.Windows.Forms.Label();
            this.lblCreadaTitulo = new System.Windows.Forms.Label();
            this.lblCreada = new System.Windows.Forms.Label();
            this.lblTamanioTitulo = new System.Windows.Forms.Label();
            this.lblTamanio = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.lblLlave1 = new System.Windows.Forms.Label();
            this.lblPasswordPropia = new System.Windows.Forms.Label();
            this.txtPasswordPropia = new System.Windows.Forms.TextBox();
            this.lblLlave2 = new System.Windows.Forms.Label();
            this.lblDniSegundo = new System.Windows.Forms.Label();
            this.txtDniSegundo = new System.Windows.Forms.TextBox();
            this.lblPasswordSegundo = new System.Windows.Forms.Label();
            this.txtPasswordSegundo = new System.Windows.Forms.TextBox();
            this.lblConfirmar = new System.Windows.Forms.Label();
            this.txtConfirmacion = new System.Windows.Forms.TextBox();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnRestaurar = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // lblAlerta
            //
            this.lblAlerta.BackColor = System.Drawing.Color.FromArgb(253, 228, 228);
            this.lblAlerta.ForeColor = System.Drawing.Color.FromArgb(163, 45, 45);
            this.lblAlerta.Location = new System.Drawing.Point(16, 16);
            this.lblAlerta.Name = "lblAlerta";
            this.lblAlerta.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.lblAlerta.Size = new System.Drawing.Size(438, 56);
            this.lblAlerta.TabIndex = 20;
            this.lblAlerta.Text = "Se van a reemplazar todos los datos actuales por los de esta copia. Lo que se cargó después de esa fecha se pierde.";
            this.lblAlerta.UseMnemonic = false;
            //
            // lblArchivoTitulo
            //
            this.lblArchivoTitulo.AutoSize = true;
            this.lblArchivoTitulo.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblArchivoTitulo.Location = new System.Drawing.Point(16, 88);
            this.lblArchivoTitulo.Name = "lblArchivoTitulo";
            this.lblArchivoTitulo.TabIndex = 21;
            this.lblArchivoTitulo.Text = "Archivo";
            //
            // lblArchivo
            //
            this.lblArchivo.AutoEllipsis = true;
            this.lblArchivo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblArchivo.Location = new System.Drawing.Point(86, 88);
            this.lblArchivo.Name = "lblArchivo";
            this.lblArchivo.Size = new System.Drawing.Size(368, 18);
            this.lblArchivo.TabIndex = 22;
            this.lblArchivo.UseMnemonic = false;
            //
            // lblCreadaTitulo
            //
            this.lblCreadaTitulo.AutoSize = true;
            this.lblCreadaTitulo.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblCreadaTitulo.Location = new System.Drawing.Point(16, 110);
            this.lblCreadaTitulo.Name = "lblCreadaTitulo";
            this.lblCreadaTitulo.TabIndex = 23;
            this.lblCreadaTitulo.Text = "Creada";
            //
            // lblCreada
            //
            this.lblCreada.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblCreada.Location = new System.Drawing.Point(86, 110);
            this.lblCreada.Name = "lblCreada";
            this.lblCreada.Size = new System.Drawing.Size(368, 18);
            this.lblCreada.TabIndex = 24;
            this.lblCreada.UseMnemonic = false;
            //
            // lblTamanioTitulo
            //
            this.lblTamanioTitulo.AutoSize = true;
            this.lblTamanioTitulo.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblTamanioTitulo.Location = new System.Drawing.Point(16, 132);
            this.lblTamanioTitulo.Name = "lblTamanioTitulo";
            this.lblTamanioTitulo.TabIndex = 25;
            this.lblTamanioTitulo.Text = "Tamaño";
            //
            // lblTamanio
            //
            this.lblTamanio.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTamanio.Location = new System.Drawing.Point(86, 132);
            this.lblTamanio.Name = "lblTamanio";
            this.lblTamanio.Size = new System.Drawing.Size(368, 18);
            this.lblTamanio.TabIndex = 26;
            this.lblTamanio.UseMnemonic = false;
            //
            // lblInfo
            //
            this.lblInfo.Location = new System.Drawing.Point(16, 164);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(438, 40);
            this.lblInfo.TabIndex = 27;
            this.lblInfo.Text = "Antes de restaurar, el sistema guarda una copia automática del estado actual, por si te equivocás. Al terminar, la aplicación se reinicia.";
            this.lblInfo.UseMnemonic = false;
            //
            // lblLlave1
            //
            this.lblLlave1.AutoEllipsis = true;
            this.lblLlave1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblLlave1.Location = new System.Drawing.Point(16, 214);
            this.lblLlave1.Name = "lblLlave1";
            this.lblLlave1.Size = new System.Drawing.Size(438, 18);
            this.lblLlave1.TabIndex = 28;
            this.lblLlave1.Text = "Llave 1: quien tiene la sesión abierta";
            this.lblLlave1.UseMnemonic = false;
            //
            // lblPasswordPropia
            //
            this.lblPasswordPropia.AutoSize = true;
            this.lblPasswordPropia.Location = new System.Drawing.Point(16, 241);
            this.lblPasswordPropia.Name = "lblPasswordPropia";
            this.lblPasswordPropia.TabIndex = 0;
            this.lblPasswordPropia.Text = "&Tu contraseña:";
            //
            // txtPasswordPropia
            //
            this.txtPasswordPropia.Location = new System.Drawing.Point(150, 238);
            this.txtPasswordPropia.MaxLength = 50;
            this.txtPasswordPropia.Name = "txtPasswordPropia";
            this.txtPasswordPropia.Size = new System.Drawing.Size(304, 23);
            this.txtPasswordPropia.TabIndex = 1;
            this.txtPasswordPropia.UseSystemPasswordChar = true;
            this.txtPasswordPropia.TextChanged += new System.EventHandler(this.Campo_TextChanged);
            //
            // lblLlave2
            //
            this.lblLlave2.AutoSize = true;
            this.lblLlave2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblLlave2.Location = new System.Drawing.Point(16, 276);
            this.lblLlave2.Name = "lblLlave2";
            this.lblLlave2.TabIndex = 29;
            this.lblLlave2.Text = "Llave 2: otro Administrador o Supervisor";
            this.lblLlave2.UseMnemonic = false;
            //
            // lblDniSegundo
            //
            this.lblDniSegundo.AutoSize = true;
            this.lblDniSegundo.Location = new System.Drawing.Point(16, 303);
            this.lblDniSegundo.Name = "lblDniSegundo";
            this.lblDniSegundo.TabIndex = 2;
            this.lblDniSegundo.Text = "&DNI:";
            //
            // txtDniSegundo
            //
            this.txtDniSegundo.Location = new System.Drawing.Point(150, 300);
            this.txtDniSegundo.MaxLength = 8;
            this.txtDniSegundo.Name = "txtDniSegundo";
            this.txtDniSegundo.Size = new System.Drawing.Size(304, 23);
            this.txtDniSegundo.TabIndex = 3;
            this.txtDniSegundo.TextChanged += new System.EventHandler(this.Campo_TextChanged);
            this.txtDniSegundo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtDniSegundo_KeyPress);
            //
            // lblPasswordSegundo
            //
            this.lblPasswordSegundo.AutoSize = true;
            this.lblPasswordSegundo.Location = new System.Drawing.Point(16, 335);
            this.lblPasswordSegundo.Name = "lblPasswordSegundo";
            this.lblPasswordSegundo.TabIndex = 4;
            this.lblPasswordSegundo.Text = "C&ontraseña:";
            //
            // txtPasswordSegundo
            //
            this.txtPasswordSegundo.Location = new System.Drawing.Point(150, 332);
            this.txtPasswordSegundo.MaxLength = 50;
            this.txtPasswordSegundo.Name = "txtPasswordSegundo";
            this.txtPasswordSegundo.Size = new System.Drawing.Size(304, 23);
            this.txtPasswordSegundo.TabIndex = 5;
            this.txtPasswordSegundo.UseSystemPasswordChar = true;
            this.txtPasswordSegundo.TextChanged += new System.EventHandler(this.Campo_TextChanged);
            //
            // lblConfirmar
            //
            this.lblConfirmar.AutoSize = true;
            this.lblConfirmar.Location = new System.Drawing.Point(16, 374);
            this.lblConfirmar.Name = "lblConfirmar";
            this.lblConfirmar.TabIndex = 30;
            this.lblConfirmar.UseMnemonic = false;
            //
            // txtConfirmacion
            //
            this.txtConfirmacion.Location = new System.Drawing.Point(16, 394);
            this.txtConfirmacion.MaxLength = 20;
            this.txtConfirmacion.Name = "txtConfirmacion";
            this.txtConfirmacion.Size = new System.Drawing.Size(438, 23);
            this.txtConfirmacion.TabIndex = 6;
            this.txtConfirmacion.TextChanged += new System.EventHandler(this.Campo_TextChanged);
            //
            // btnCancelar
            //
            this.btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelar.Location = new System.Drawing.Point(274, 440);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(86, 32);
            this.btnCancelar.TabIndex = 8;
            this.btnCancelar.Text = "&Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            //
            // btnRestaurar
            //
            this.btnRestaurar.BackColor = System.Drawing.Color.FromArgb(185, 28, 28);
            this.btnRestaurar.Enabled = false;
            this.btnRestaurar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRestaurar.FlatAppearance.BorderSize = 0;
            this.btnRestaurar.ForeColor = System.Drawing.Color.White;
            this.btnRestaurar.Location = new System.Drawing.Point(368, 440);
            this.btnRestaurar.Name = "btnRestaurar";
            this.btnRestaurar.Size = new System.Drawing.Size(86, 32);
            this.btnRestaurar.TabIndex = 7;
            this.btnRestaurar.Text = "&Restaurar";
            this.btnRestaurar.UseVisualStyleBackColor = false;
            this.btnRestaurar.EnabledChanged += new System.EventHandler(this.btnRestaurar_EnabledChanged);
            this.btnRestaurar.Click += new System.EventHandler(this.btnRestaurar_Click);
            //
            // frmConfirmarRestauracion
            //
            this.AcceptButton = this.btnRestaurar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancelar;
            this.ClientSize = new System.Drawing.Size(470, 488);
            this.Controls.Add(this.btnRestaurar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.txtConfirmacion);
            this.Controls.Add(this.lblConfirmar);
            this.Controls.Add(this.txtPasswordSegundo);
            this.Controls.Add(this.lblPasswordSegundo);
            this.Controls.Add(this.txtDniSegundo);
            this.Controls.Add(this.lblDniSegundo);
            this.Controls.Add(this.lblLlave2);
            this.Controls.Add(this.txtPasswordPropia);
            this.Controls.Add(this.lblPasswordPropia);
            this.Controls.Add(this.lblLlave1);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.lblTamanio);
            this.Controls.Add(this.lblTamanioTitulo);
            this.Controls.Add(this.lblCreada);
            this.Controls.Add(this.lblCreadaTitulo);
            this.Controls.Add(this.lblArchivo);
            this.Controls.Add(this.lblArchivoTitulo);
            this.Controls.Add(this.lblAlerta);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmConfirmarRestauracion";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Restaurar copia de seguridad";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblAlerta;
        private System.Windows.Forms.Label lblArchivoTitulo;
        private System.Windows.Forms.Label lblArchivo;
        private System.Windows.Forms.Label lblCreadaTitulo;
        private System.Windows.Forms.Label lblCreada;
        private System.Windows.Forms.Label lblTamanioTitulo;
        private System.Windows.Forms.Label lblTamanio;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Label lblLlave1;
        private System.Windows.Forms.Label lblPasswordPropia;
        private System.Windows.Forms.TextBox txtPasswordPropia;
        private System.Windows.Forms.Label lblLlave2;
        private System.Windows.Forms.Label lblDniSegundo;
        private System.Windows.Forms.TextBox txtDniSegundo;
        private System.Windows.Forms.Label lblPasswordSegundo;
        private System.Windows.Forms.TextBox txtPasswordSegundo;
        private System.Windows.Forms.Label lblConfirmar;
        private System.Windows.Forms.TextBox txtConfirmacion;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Button btnRestaurar;
    }
}
