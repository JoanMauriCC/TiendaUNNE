namespace TiendaUNNE
{
    partial class frmCobro
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
            this.lblTotalTitulo = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            this.lblMedioPago = new System.Windows.Forms.Label();
            this.cboMedioPago = new System.Windows.Forms.ComboBox();
            this.lblImporte = new System.Windows.Forms.Label();
            this.numImporte = new System.Windows.Forms.NumericUpDown();
            this.btnAgregarPago = new System.Windows.Forms.Button();
            this.dgvPagos = new System.Windows.Forms.DataGridView();
            this.btnQuitarPago = new System.Windows.Forms.Button();
            this.lblRestante = new System.Windows.Forms.Label();
            this.lblVuelto = new System.Windows.Forms.Label();
            this.btnConfirmar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numImporte)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPagos)).BeginInit();
            this.SuspendLayout();
            //
            // lblTotalTitulo
            //
            this.lblTotalTitulo.AutoSize = true;
            this.lblTotalTitulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(125)))), ((int)(((byte)(135)))));
            this.lblTotalTitulo.Location = new System.Drawing.Point(24, 20);
            this.lblTotalTitulo.Name = "lblTotalTitulo";
            this.lblTotalTitulo.Size = new System.Drawing.Size(76, 15);
            this.lblTotalTitulo.TabIndex = 0;
            this.lblTotalTitulo.Text = "Total a pagar";
            //
            // lblTotal
            //
            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblTotal.Location = new System.Drawing.Point(21, 38);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(80, 37);
            this.lblTotal.TabIndex = 1;
            this.lblTotal.Text = "0,00";
            //
            // lblMedioPago
            //
            this.lblMedioPago.AutoSize = true;
            this.lblMedioPago.Location = new System.Drawing.Point(24, 92);
            this.lblMedioPago.Name = "lblMedioPago";
            this.lblMedioPago.Size = new System.Drawing.Size(84, 15);
            this.lblMedioPago.TabIndex = 2;
            this.lblMedioPago.Text = "Medio de pago";
            //
            // cboMedioPago
            //
            this.cboMedioPago.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMedioPago.Location = new System.Drawing.Point(24, 110);
            this.cboMedioPago.Name = "cboMedioPago";
            this.cboMedioPago.Size = new System.Drawing.Size(190, 23);
            this.cboMedioPago.TabIndex = 3;
            //
            // lblImporte
            //
            this.lblImporte.AutoSize = true;
            this.lblImporte.Location = new System.Drawing.Point(224, 92);
            this.lblImporte.Name = "lblImporte";
            this.lblImporte.Size = new System.Drawing.Size(48, 15);
            this.lblImporte.TabIndex = 4;
            this.lblImporte.Text = "Importe";
            //
            // numImporte
            //
            this.numImporte.DecimalPlaces = 2;
            this.numImporte.Location = new System.Drawing.Point(224, 110);
            this.numImporte.Maximum = new decimal(new int[] { 10000000, 0, 0, 0});
            this.numImporte.Name = "numImporte";
            this.numImporte.Size = new System.Drawing.Size(120, 23);
            this.numImporte.TabIndex = 5;
            this.numImporte.ThousandsSeparator = true;
            this.numImporte.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            //
            // btnAgregarPago
            //
            this.btnAgregarPago.Location = new System.Drawing.Point(354, 109);
            this.btnAgregarPago.Name = "btnAgregarPago";
            this.btnAgregarPago.Size = new System.Drawing.Size(90, 25);
            this.btnAgregarPago.TabIndex = 6;
            this.btnAgregarPago.Text = "Agregar";
            this.btnAgregarPago.UseVisualStyleBackColor = true;
            this.btnAgregarPago.Click += new System.EventHandler(this.btnAgregarPago_Click);
            //
            // dgvPagos
            //
            this.dgvPagos.AllowUserToAddRows = false;
            this.dgvPagos.AllowUserToDeleteRows = false;
            this.dgvPagos.AllowUserToResizeRows = false;
            this.dgvPagos.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvPagos.BackgroundColor = System.Drawing.Color.White;
            this.dgvPagos.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dgvPagos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPagos.Location = new System.Drawing.Point(24, 148);
            this.dgvPagos.MultiSelect = false;
            this.dgvPagos.Name = "dgvPagos";
            this.dgvPagos.ReadOnly = true;
            this.dgvPagos.RowHeadersVisible = false;
            this.dgvPagos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPagos.Size = new System.Drawing.Size(420, 126);
            this.dgvPagos.TabIndex = 7;
            //
            // btnQuitarPago
            //
            this.btnQuitarPago.Location = new System.Drawing.Point(24, 282);
            this.btnQuitarPago.Name = "btnQuitarPago";
            this.btnQuitarPago.Size = new System.Drawing.Size(110, 26);
            this.btnQuitarPago.TabIndex = 8;
            this.btnQuitarPago.Text = "Quitar pago";
            this.btnQuitarPago.UseVisualStyleBackColor = true;
            this.btnQuitarPago.Click += new System.EventHandler(this.btnQuitarPago_Click);
            //
            // lblRestante
            //
            this.lblRestante.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblRestante.Location = new System.Drawing.Point(224, 282);
            this.lblRestante.Name = "lblRestante";
            this.lblRestante.Size = new System.Drawing.Size(220, 22);
            this.lblRestante.TabIndex = 9;
            this.lblRestante.Text = "Restante: 0,00";
            this.lblRestante.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblVuelto
            //
            this.lblVuelto.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(148)))), ((int)(((byte)(136)))));
            this.lblVuelto.Location = new System.Drawing.Point(224, 306);
            this.lblVuelto.Name = "lblVuelto";
            this.lblVuelto.Size = new System.Drawing.Size(220, 20);
            this.lblVuelto.TabIndex = 10;
            this.lblVuelto.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // btnConfirmar
            //
            this.btnConfirmar.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnConfirmar.Location = new System.Drawing.Point(250, 338);
            this.btnConfirmar.Name = "btnConfirmar";
            this.btnConfirmar.Size = new System.Drawing.Size(194, 38);
            this.btnConfirmar.TabIndex = 11;
            this.btnConfirmar.Text = "Confirmar venta";
            this.btnConfirmar.UseVisualStyleBackColor = true;
            this.btnConfirmar.Click += new System.EventHandler(this.btnConfirmar_Click);
            //
            // btnCancelar
            //
            this.btnCancelar.Location = new System.Drawing.Point(24, 342);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(110, 30);
            this.btnCancelar.TabIndex = 12;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            //
            // frmCobro
            //
            this.AcceptButton = this.btnAgregarPago;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancelar;
            this.ClientSize = new System.Drawing.Size(468, 394);
            this.Controls.Add(this.lblTotalTitulo);
            this.Controls.Add(this.lblTotal);
            this.Controls.Add(this.lblMedioPago);
            this.Controls.Add(this.cboMedioPago);
            this.Controls.Add(this.lblImporte);
            this.Controls.Add(this.numImporte);
            this.Controls.Add(this.btnAgregarPago);
            this.Controls.Add(this.dgvPagos);
            this.Controls.Add(this.btnQuitarPago);
            this.Controls.Add(this.lblRestante);
            this.Controls.Add(this.lblVuelto);
            this.Controls.Add(this.btnConfirmar);
            this.Controls.Add(this.btnCancelar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmCobro";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cobrar";
            this.Load += new System.EventHandler(this.frmCobro_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numImporte)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPagos)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTotalTitulo;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label lblMedioPago;
        private System.Windows.Forms.ComboBox cboMedioPago;
        private System.Windows.Forms.Label lblImporte;
        private System.Windows.Forms.NumericUpDown numImporte;
        private System.Windows.Forms.Button btnAgregarPago;
        private System.Windows.Forms.DataGridView dgvPagos;
        private System.Windows.Forms.Button btnQuitarPago;
        private System.Windows.Forms.Label lblRestante;
        private System.Windows.Forms.Label lblVuelto;
        private System.Windows.Forms.Button btnConfirmar;
        private System.Windows.Forms.Button btnCancelar;
    }
}
