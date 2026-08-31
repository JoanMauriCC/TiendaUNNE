namespace TiendaUNNE
{
    partial class frmProductoEditor
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
            this.lblNombre = new System.Windows.Forms.Label();
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.lblDescripcion = new System.Windows.Forms.Label();
            this.txtDescripcion = new System.Windows.Forms.TextBox();
            this.lblCategoria = new System.Windows.Forms.Label();
            this.cboCategoria = new System.Windows.Forms.ComboBox();
            this.lblPrecioVenta = new System.Windows.Forms.Label();
            this.numPrecioVenta = new System.Windows.Forms.NumericUpDown();
            this.lblStock = new System.Windows.Forms.Label();
            this.numStock = new System.Windows.Forms.NumericUpDown();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numPrecioVenta)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStock)).BeginInit();
            this.SuspendLayout();
            //
            // lblNombre
            //
            this.lblNombre.AutoSize = true;
            this.lblNombre.Location = new System.Drawing.Point(16, 18);
            this.lblNombre.Name = "lblNombre";
            this.lblNombre.Size = new System.Drawing.Size(52, 15);
            this.lblNombre.TabIndex = 0;
            this.lblNombre.Text = "Nombre *";
            //
            // txtNombre
            //
            this.txtNombre.Location = new System.Drawing.Point(16, 36);
            this.txtNombre.MaxLength = 150;
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.Size = new System.Drawing.Size(250, 23);
            this.txtNombre.TabIndex = 1;
            //
            // lblDescripcion
            //
            this.lblDescripcion.AutoSize = true;
            this.lblDescripcion.Location = new System.Drawing.Point(16, 66);
            this.lblDescripcion.Name = "lblDescripcion";
            this.lblDescripcion.Size = new System.Drawing.Size(69, 15);
            this.lblDescripcion.TabIndex = 2;
            this.lblDescripcion.Text = "Descripción";
            //
            // txtDescripcion
            //
            this.txtDescripcion.Location = new System.Drawing.Point(16, 84);
            this.txtDescripcion.MaxLength = 500;
            this.txtDescripcion.Multiline = true;
            this.txtDescripcion.Name = "txtDescripcion";
            this.txtDescripcion.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDescripcion.Size = new System.Drawing.Size(250, 64);
            this.txtDescripcion.TabIndex = 3;
            //
            // lblCategoria
            //
            this.lblCategoria.AutoSize = true;
            this.lblCategoria.Location = new System.Drawing.Point(16, 156);
            this.lblCategoria.Name = "lblCategoria";
            this.lblCategoria.Size = new System.Drawing.Size(66, 15);
            this.lblCategoria.TabIndex = 4;
            this.lblCategoria.Text = "Categoría *";
            //
            // cboCategoria
            //
            this.cboCategoria.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategoria.FormattingEnabled = true;
            this.cboCategoria.Location = new System.Drawing.Point(16, 174);
            this.cboCategoria.Name = "cboCategoria";
            this.cboCategoria.Size = new System.Drawing.Size(250, 23);
            this.cboCategoria.TabIndex = 5;
            //
            // lblPrecioVenta
            //
            this.lblPrecioVenta.AutoSize = true;
            this.lblPrecioVenta.Location = new System.Drawing.Point(296, 18);
            this.lblPrecioVenta.Name = "lblPrecioVenta";
            this.lblPrecioVenta.Size = new System.Drawing.Size(75, 15);
            this.lblPrecioVenta.TabIndex = 6;
            this.lblPrecioVenta.Text = "Precio venta";
            //
            // numPrecioVenta
            //
            this.numPrecioVenta.DecimalPlaces = 2;
            this.numPrecioVenta.Location = new System.Drawing.Point(296, 36);
            this.numPrecioVenta.Maximum = new decimal(new int[] { 1000000000, 0, 0, 0});
            this.numPrecioVenta.Name = "numPrecioVenta";
            this.numPrecioVenta.Size = new System.Drawing.Size(160, 23);
            this.numPrecioVenta.TabIndex = 7;
            this.numPrecioVenta.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numPrecioVenta.ThousandsSeparator = true;
            //
            // lblStock
            //
            this.lblStock.AutoSize = true;
            this.lblStock.Location = new System.Drawing.Point(296, 66);
            this.lblStock.Name = "lblStock";
            this.lblStock.Size = new System.Drawing.Size(37, 15);
            this.lblStock.TabIndex = 8;
            this.lblStock.Text = "Stock";
            //
            // numStock
            //
            this.numStock.DecimalPlaces = 3;
            this.numStock.Location = new System.Drawing.Point(296, 84);
            this.numStock.Maximum = new decimal(new int[] { 1000000000, 0, 0, 0});
            this.numStock.Name = "numStock";
            this.numStock.Size = new System.Drawing.Size(160, 23);
            this.numStock.TabIndex = 9;
            this.numStock.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numStock.ThousandsSeparator = true;
            //
            // btnGuardar
            //
            this.btnGuardar.Location = new System.Drawing.Point(300, 216);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(85, 32);
            this.btnGuardar.TabIndex = 10;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            //
            // btnCancelar
            //
            this.btnCancelar.Location = new System.Drawing.Point(391, 216);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(85, 32);
            this.btnCancelar.TabIndex = 11;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            //
            // frmProductoEditor
            //
            this.AcceptButton = this.btnGuardar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancelar;
            this.ClientSize = new System.Drawing.Size(492, 266);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.numStock);
            this.Controls.Add(this.lblStock);
            this.Controls.Add(this.numPrecioVenta);
            this.Controls.Add(this.lblPrecioVenta);
            this.Controls.Add(this.cboCategoria);
            this.Controls.Add(this.lblCategoria);
            this.Controls.Add(this.txtDescripcion);
            this.Controls.Add(this.lblDescripcion);
            this.Controls.Add(this.txtNombre);
            this.Controls.Add(this.lblNombre);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmProductoEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Producto";
            this.Load += new System.EventHandler(this.frmProductoEditor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numPrecioVenta)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStock)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.Label lblDescripcion;
        private System.Windows.Forms.TextBox txtDescripcion;
        private System.Windows.Forms.Label lblCategoria;
        private System.Windows.Forms.ComboBox cboCategoria;
        private System.Windows.Forms.Label lblPrecioVenta;
        private System.Windows.Forms.NumericUpDown numPrecioVenta;
        private System.Windows.Forms.Label lblStock;
        private System.Windows.Forms.NumericUpDown numStock;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnCancelar;
    }
}
