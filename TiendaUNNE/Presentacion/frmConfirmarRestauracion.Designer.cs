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
            this.lblAlerta.TabIndex = 0;
            this.lblAlerta.Text = "Se van a reemplazar todos los datos actuales por los de esta copia. Lo que se cargó después de esa fecha se pierde.";
            this.lblAlerta.UseMnemonic = false;
            //
            // lblArchivoTitulo
            //
            this.lblArchivoTitulo.AutoSize = true;
            this.lblArchivoTitulo.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblArchivoTitulo.Location = new System.Drawing.Point(16, 88);
            this.lblArchivoTitulo.Name = "lblArchivoTitulo";
            this.lblArchivoTitulo.TabIndex = 1;
            this.lblArchivoTitulo.Text = "Archivo";
            //
            // lblArchivo
            //
            this.lblArchivo.AutoEllipsis = true;
            this.lblArchivo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblArchivo.Location = new System.Drawing.Point(86, 88);
            this.lblArchivo.Name = "lblArchivo";
            this.lblArchivo.Size = new System.Drawing.Size(368, 18);
            this.lblArchivo.TabIndex = 2;
            this.lblArchivo.UseMnemonic = false;
            //
            // lblCreadaTitulo
            //
            this.lblCreadaTitulo.AutoSize = true;
            this.lblCreadaTitulo.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblCreadaTitulo.Location = new System.Drawing.Point(16, 110);
            this.lblCreadaTitulo.Name = "lblCreadaTitulo";
            this.lblCreadaTitulo.TabIndex = 3;
            this.lblCreadaTitulo.Text = "Creada";
            //
            // lblCreada
            //
            this.lblCreada.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblCreada.Location = new System.Drawing.Point(86, 110);
            this.lblCreada.Name = "lblCreada";
            this.lblCreada.Size = new System.Drawing.Size(368, 18);
            this.lblCreada.TabIndex = 4;
            this.lblCreada.UseMnemonic = false;
            //
            // lblTamanioTitulo
            //
            this.lblTamanioTitulo.AutoSize = true;
            this.lblTamanioTitulo.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblTamanioTitulo.Location = new System.Drawing.Point(16, 132);
            this.lblTamanioTitulo.Name = "lblTamanioTitulo";
            this.lblTamanioTitulo.TabIndex = 5;
            this.lblTamanioTitulo.Text = "Tamaño";
            //
            // lblTamanio
            //
            this.lblTamanio.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTamanio.Location = new System.Drawing.Point(86, 132);
            this.lblTamanio.Name = "lblTamanio";
            this.lblTamanio.Size = new System.Drawing.Size(368, 18);
            this.lblTamanio.TabIndex = 6;
            this.lblTamanio.UseMnemonic = false;
            //
            // lblInfo
            //
            this.lblInfo.Location = new System.Drawing.Point(16, 164);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(438, 40);
            this.lblInfo.TabIndex = 7;
            this.lblInfo.Text = "Antes de restaurar, el sistema guarda una copia automática del estado actual, por si te equivocás. Al terminar, la aplicación se reinicia.";
            this.lblInfo.UseMnemonic = false;
            //
            // lblConfirmar
            //
            this.lblConfirmar.AutoSize = true;
            this.lblConfirmar.Location = new System.Drawing.Point(16, 214);
            this.lblConfirmar.Name = "lblConfirmar";
            this.lblConfirmar.TabIndex = 8;
            this.lblConfirmar.UseMnemonic = false;
            //
            // txtConfirmacion
            //
            this.txtConfirmacion.Location = new System.Drawing.Point(16, 234);
            this.txtConfirmacion.MaxLength = 20;
            this.txtConfirmacion.Name = "txtConfirmacion";
            this.txtConfirmacion.Size = new System.Drawing.Size(438, 23);
            this.txtConfirmacion.TabIndex = 0;
            this.txtConfirmacion.TextChanged += new System.EventHandler(this.txtConfirmacion_TextChanged);
            //
            // btnCancelar
            //
            this.btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelar.Location = new System.Drawing.Point(274, 280);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(86, 32);
            this.btnCancelar.TabIndex = 1;
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
            this.btnRestaurar.Location = new System.Drawing.Point(368, 280);
            this.btnRestaurar.Name = "btnRestaurar";
            this.btnRestaurar.Size = new System.Drawing.Size(86, 32);
            this.btnRestaurar.TabIndex = 2;
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
            this.ClientSize = new System.Drawing.Size(470, 328);
            this.Controls.Add(this.btnRestaurar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.txtConfirmacion);
            this.Controls.Add(this.lblConfirmar);
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
        private System.Windows.Forms.Label lblConfirmar;
        private System.Windows.Forms.TextBox txtConfirmacion;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Button btnRestaurar;
    }
}
