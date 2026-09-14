namespace TiendaUNNE
{
    partial class frmCierreCaja
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
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblMontoInicialTitulo = new System.Windows.Forms.Label();
            this.lblMontoInicial = new System.Windows.Forms.Label();
            this.lblVentasTitulo = new System.Windows.Forms.Label();
            this.lblVentas = new System.Windows.Forms.Label();
            this.separador = new System.Windows.Forms.Panel();
            this.lblEsperadoTitulo = new System.Windows.Forms.Label();
            this.lblEsperado = new System.Windows.Forms.Label();
            this.lblDeclaradoTitulo = new System.Windows.Forms.Label();
            this.numDeclarado = new System.Windows.Forms.NumericUpDown();
            this.lblDiferencia = new System.Windows.Forms.Label();
            this.lblObservaciones = new System.Windows.Forms.Label();
            this.txtObservaciones = new System.Windows.Forms.TextBox();
            this.btnCerrar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numDeclarado)).BeginInit();
            this.SuspendLayout();
            //
            // lblTitulo
            //
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.Location = new System.Drawing.Point(22, 18);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(104, 21);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Cerrar caja";
            //
            // lblMontoInicialTitulo
            //
            this.lblMontoInicialTitulo.AutoSize = true;
            this.lblMontoInicialTitulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(125)))), ((int)(((byte)(135)))));
            this.lblMontoInicialTitulo.Location = new System.Drawing.Point(24, 56);
            this.lblMontoInicialTitulo.Name = "lblMontoInicialTitulo";
            this.lblMontoInicialTitulo.Size = new System.Drawing.Size(79, 15);
            this.lblMontoInicialTitulo.TabIndex = 1;
            this.lblMontoInicialTitulo.Text = "Monto inicial";
            //
            // lblMontoInicial
            //
            this.lblMontoInicial.Location = new System.Drawing.Point(184, 56);
            this.lblMontoInicial.Name = "lblMontoInicial";
            this.lblMontoInicial.Size = new System.Drawing.Size(160, 15);
            this.lblMontoInicial.TabIndex = 2;
            this.lblMontoInicial.Text = "0,00";
            this.lblMontoInicial.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblVentasTitulo
            //
            this.lblVentasTitulo.AutoSize = true;
            this.lblVentasTitulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(125)))), ((int)(((byte)(135)))));
            this.lblVentasTitulo.Location = new System.Drawing.Point(24, 80);
            this.lblVentasTitulo.Name = "lblVentasTitulo";
            this.lblVentasTitulo.Size = new System.Drawing.Size(117, 15);
            this.lblVentasTitulo.TabIndex = 3;
            this.lblVentasTitulo.Text = "Ventas en efectivo";
            //
            // lblVentas
            //
            this.lblVentas.Location = new System.Drawing.Point(184, 80);
            this.lblVentas.Name = "lblVentas";
            this.lblVentas.Size = new System.Drawing.Size(160, 15);
            this.lblVentas.TabIndex = 4;
            this.lblVentas.Text = "0,00";
            this.lblVentas.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // separador
            //
            this.separador.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(230)))), ((int)(((byte)(234)))));
            this.separador.Location = new System.Drawing.Point(24, 104);
            this.separador.Name = "separador";
            this.separador.Size = new System.Drawing.Size(320, 1);
            this.separador.TabIndex = 5;
            //
            // lblEsperadoTitulo
            //
            this.lblEsperadoTitulo.AutoSize = true;
            this.lblEsperadoTitulo.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblEsperadoTitulo.Location = new System.Drawing.Point(24, 114);
            this.lblEsperadoTitulo.Name = "lblEsperadoTitulo";
            this.lblEsperadoTitulo.Size = new System.Drawing.Size(122, 17);
            this.lblEsperadoTitulo.TabIndex = 6;
            this.lblEsperadoTitulo.Text = "Efectivo esperado";
            //
            // lblEsperado
            //
            this.lblEsperado.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblEsperado.Location = new System.Drawing.Point(184, 114);
            this.lblEsperado.Name = "lblEsperado";
            this.lblEsperado.Size = new System.Drawing.Size(160, 17);
            this.lblEsperado.TabIndex = 7;
            this.lblEsperado.Text = "0,00";
            this.lblEsperado.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblDeclaradoTitulo
            //
            this.lblDeclaradoTitulo.AutoSize = true;
            this.lblDeclaradoTitulo.Location = new System.Drawing.Point(24, 150);
            this.lblDeclaradoTitulo.Name = "lblDeclaradoTitulo";
            this.lblDeclaradoTitulo.Size = new System.Drawing.Size(163, 15);
            this.lblDeclaradoTitulo.TabIndex = 8;
            this.lblDeclaradoTitulo.Text = "Efectivo contado en la caja";
            //
            // numDeclarado
            //
            this.numDeclarado.DecimalPlaces = 2;
            this.numDeclarado.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.numDeclarado.Location = new System.Drawing.Point(24, 170);
            this.numDeclarado.Maximum = new decimal(new int[] { 10000000, 0, 0, 0});
            this.numDeclarado.Name = "numDeclarado";
            this.numDeclarado.Size = new System.Drawing.Size(320, 27);
            this.numDeclarado.TabIndex = 9;
            this.numDeclarado.ThousandsSeparator = true;
            this.numDeclarado.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numDeclarado.ValueChanged += new System.EventHandler(this.numDeclarado_ValueChanged);
            //
            // lblDiferencia
            //
            this.lblDiferencia.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblDiferencia.Location = new System.Drawing.Point(24, 204);
            this.lblDiferencia.Name = "lblDiferencia";
            this.lblDiferencia.Size = new System.Drawing.Size(320, 20);
            this.lblDiferencia.TabIndex = 10;
            this.lblDiferencia.Text = "Diferencia: 0,00";
            this.lblDiferencia.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblObservaciones
            //
            this.lblObservaciones.AutoSize = true;
            this.lblObservaciones.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(125)))), ((int)(((byte)(135)))));
            this.lblObservaciones.Location = new System.Drawing.Point(24, 234);
            this.lblObservaciones.Name = "lblObservaciones";
            this.lblObservaciones.Size = new System.Drawing.Size(146, 15);
            this.lblObservaciones.TabIndex = 11;
            this.lblObservaciones.Text = "Observaciones (opcional)";
            //
            // txtObservaciones
            //
            this.txtObservaciones.Location = new System.Drawing.Point(24, 252);
            this.txtObservaciones.MaxLength = 300;
            this.txtObservaciones.Multiline = true;
            this.txtObservaciones.Name = "txtObservaciones";
            this.txtObservaciones.Size = new System.Drawing.Size(320, 54);
            this.txtObservaciones.TabIndex = 12;
            //
            // btnCerrar
            //
            this.btnCerrar.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnCerrar.Location = new System.Drawing.Point(190, 320);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(154, 34);
            this.btnCerrar.TabIndex = 13;
            this.btnCerrar.Text = "Cerrar caja";
            this.btnCerrar.UseVisualStyleBackColor = true;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            //
            // btnCancelar
            //
            this.btnCancelar.Location = new System.Drawing.Point(24, 322);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(110, 30);
            this.btnCancelar.TabIndex = 14;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            //
            // frmCierreCaja
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancelar;
            this.ClientSize = new System.Drawing.Size(368, 372);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblMontoInicialTitulo);
            this.Controls.Add(this.lblMontoInicial);
            this.Controls.Add(this.lblVentasTitulo);
            this.Controls.Add(this.lblVentas);
            this.Controls.Add(this.separador);
            this.Controls.Add(this.lblEsperadoTitulo);
            this.Controls.Add(this.lblEsperado);
            this.Controls.Add(this.lblDeclaradoTitulo);
            this.Controls.Add(this.numDeclarado);
            this.Controls.Add(this.lblDiferencia);
            this.Controls.Add(this.lblObservaciones);
            this.Controls.Add(this.txtObservaciones);
            this.Controls.Add(this.btnCerrar);
            this.Controls.Add(this.btnCancelar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmCierreCaja";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cerrar caja";
            this.Load += new System.EventHandler(this.frmCierreCaja_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numDeclarado)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblMontoInicialTitulo;
        private System.Windows.Forms.Label lblMontoInicial;
        private System.Windows.Forms.Label lblVentasTitulo;
        private System.Windows.Forms.Label lblVentas;
        private System.Windows.Forms.Panel separador;
        private System.Windows.Forms.Label lblEsperadoTitulo;
        private System.Windows.Forms.Label lblEsperado;
        private System.Windows.Forms.Label lblDeclaradoTitulo;
        private System.Windows.Forms.NumericUpDown numDeclarado;
        private System.Windows.Forms.Label lblDiferencia;
        private System.Windows.Forms.Label lblObservaciones;
        private System.Windows.Forms.TextBox txtObservaciones;
        private System.Windows.Forms.Button btnCerrar;
        private System.Windows.Forms.Button btnCancelar;
    }
}
