namespace TiendaUNNE
{
    partial class ucProductos
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
            this.lblNombre = new System.Windows.Forms.Label();
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.lblCategoria = new System.Windows.Forms.Label();
            this.cboCategoria = new System.Windows.Forms.ComboBox();
            this.lblPrecioVenta = new System.Windows.Forms.Label();
            this.numPrecioVenta = new System.Windows.Forms.NumericUpDown();
            this.lblStock = new System.Windows.Forms.Label();
            this.numStock = new System.Windows.Forms.NumericUpDown();
            this.lblStockMinimo = new System.Windows.Forms.Label();
            this.numStockMinimo = new System.Windows.Forms.NumericUpDown();
            this.lblDescripcion = new System.Windows.Forms.Label();
            this.txtDescripcion = new System.Windows.Forms.TextBox();
            this.lblObligatorios = new System.Windows.Forms.Label();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnLimpiar = new System.Windows.Forms.Button();
            this.lblBuscar = new System.Windows.Forms.Label();
            this.txtBuscar = new System.Windows.Forms.TextBox();
            this.tarjetaVerInactivos = new TiendaUNNE.TarjetaAccion();
            this.tarjetaBaja = new TiendaUNNE.TarjetaAccion();
            this.tarjetaActualizar = new TiendaUNNE.TarjetaAccion();
            this.dgvProductos = new System.Windows.Forms.DataGridView();
            this.panelFormulario.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPrecioVenta)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStock)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStockMinimo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProductos)).BeginInit();
            this.SuspendLayout();
            //
            // panelFormulario
            //
            this.panelFormulario.BackColor = System.Drawing.Color.White;
            this.panelFormulario.Controls.Add(this.lblTituloForm);
            this.panelFormulario.Controls.Add(this.lblNombre);
            this.panelFormulario.Controls.Add(this.txtNombre);
            this.panelFormulario.Controls.Add(this.lblCategoria);
            this.panelFormulario.Controls.Add(this.cboCategoria);
            this.panelFormulario.Controls.Add(this.lblPrecioVenta);
            this.panelFormulario.Controls.Add(this.numPrecioVenta);
            this.panelFormulario.Controls.Add(this.lblStock);
            this.panelFormulario.Controls.Add(this.numStock);
            this.panelFormulario.Controls.Add(this.lblStockMinimo);
            this.panelFormulario.Controls.Add(this.numStockMinimo);
            this.panelFormulario.Controls.Add(this.lblDescripcion);
            this.panelFormulario.Controls.Add(this.txtDescripcion);
            this.panelFormulario.Controls.Add(this.lblObligatorios);
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
            this.panelFormulario.Size = new System.Drawing.Size(844, 266);
            this.panelFormulario.TabIndex = 0;
            //
            // lblTituloForm
            //
            this.lblTituloForm.AutoEllipsis = true;
            this.lblTituloForm.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTituloForm.Location = new System.Drawing.Point(12, 6);
            this.lblTituloForm.Name = "lblTituloForm";
            this.lblTituloForm.Size = new System.Drawing.Size(500, 24);
            this.lblTituloForm.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblTituloForm.TabIndex = 0;
            this.lblTituloForm.Text = "Nuevo producto";
            //
            // lblNombre
            //
            this.lblNombre.AutoSize = true;
            this.lblNombre.Location = new System.Drawing.Point(12, 36);
            this.lblNombre.Name = "lblNombre";
            this.lblNombre.Size = new System.Drawing.Size(56, 15);
            this.lblNombre.TabIndex = 1;
            this.lblNombre.Text = "&Nombre *";
            //
            // txtNombre
            //
            this.txtNombre.Location = new System.Drawing.Point(12, 52);
            this.txtNombre.MaxLength = 150;
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.Size = new System.Drawing.Size(334, 23);
            this.txtNombre.TabIndex = 2;
            //
            // lblCategoria
            //
            this.lblCategoria.AutoSize = true;
            this.lblCategoria.Location = new System.Drawing.Point(356, 36);
            this.lblCategoria.Name = "lblCategoria";
            this.lblCategoria.Size = new System.Drawing.Size(66, 15);
            this.lblCategoria.TabIndex = 3;
            this.lblCategoria.Text = "&Categoría *";
            //
            // cboCategoria
            //
            this.cboCategoria.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategoria.FormattingEnabled = true;
            this.cboCategoria.Location = new System.Drawing.Point(356, 52);
            this.cboCategoria.Name = "cboCategoria";
            this.cboCategoria.Size = new System.Drawing.Size(160, 23);
            this.cboCategoria.TabIndex = 4;
            //
            // lblPrecioVenta
            //
            this.lblPrecioVenta.AutoSize = true;
            this.lblPrecioVenta.Location = new System.Drawing.Point(12, 86);
            this.lblPrecioVenta.Name = "lblPrecioVenta";
            this.lblPrecioVenta.Size = new System.Drawing.Size(88, 15);
            this.lblPrecioVenta.TabIndex = 5;
            this.lblPrecioVenta.Text = "&Precio de venta";
            //
            // numPrecioVenta
            //
            this.numPrecioVenta.DecimalPlaces = 2;
            this.numPrecioVenta.Location = new System.Drawing.Point(12, 102);
            this.numPrecioVenta.Maximum = new decimal(new int[] { 1000000000, 0, 0, 0 });
            this.numPrecioVenta.Name = "numPrecioVenta";
            this.numPrecioVenta.Size = new System.Drawing.Size(160, 23);
            this.numPrecioVenta.TabIndex = 6;
            this.numPrecioVenta.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numPrecioVenta.ThousandsSeparator = true;
            //
            // lblStock
            //
            this.lblStock.AutoSize = true;
            this.lblStock.Location = new System.Drawing.Point(184, 86);
            this.lblStock.Name = "lblStock";
            this.lblStock.Size = new System.Drawing.Size(35, 15);
            this.lblStock.TabIndex = 7;
            this.lblStock.Text = "&Stock";
            //
            // numStock
            //
            this.numStock.DecimalPlaces = 3;
            this.numStock.Location = new System.Drawing.Point(184, 102);
            this.numStock.Maximum = new decimal(new int[] { 1000000000, 0, 0, 0 });
            this.numStock.Name = "numStock";
            this.numStock.Size = new System.Drawing.Size(160, 23);
            this.numStock.TabIndex = 8;
            this.numStock.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numStock.ThousandsSeparator = true;
            //
            // lblStockMinimo
            //
            this.lblStockMinimo.AutoSize = true;
            this.lblStockMinimo.Location = new System.Drawing.Point(356, 86);
            this.lblStockMinimo.Name = "lblStockMinimo";
            this.lblStockMinimo.Size = new System.Drawing.Size(78, 15);
            this.lblStockMinimo.TabIndex = 9;
            this.lblStockMinimo.Text = "Stock &mínimo";
            //
            // numStockMinimo
            //
            this.numStockMinimo.DecimalPlaces = 3;
            this.numStockMinimo.Location = new System.Drawing.Point(356, 102);
            this.numStockMinimo.Maximum = new decimal(new int[] { 1000000000, 0, 0, 0 });
            this.numStockMinimo.Name = "numStockMinimo";
            this.numStockMinimo.Size = new System.Drawing.Size(160, 23);
            this.numStockMinimo.TabIndex = 10;
            this.numStockMinimo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numStockMinimo.ThousandsSeparator = true;
            //
            // lblDescripcion
            //
            this.lblDescripcion.AutoSize = true;
            this.lblDescripcion.Location = new System.Drawing.Point(12, 136);
            this.lblDescripcion.Name = "lblDescripcion";
            this.lblDescripcion.Size = new System.Drawing.Size(69, 15);
            this.lblDescripcion.TabIndex = 11;
            this.lblDescripcion.Text = "&Descripción";
            //
            // txtDescripcion
            //
            this.txtDescripcion.Location = new System.Drawing.Point(12, 152);
            this.txtDescripcion.MaxLength = 500;
            this.txtDescripcion.Multiline = true;
            this.txtDescripcion.Name = "txtDescripcion";
            this.txtDescripcion.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDescripcion.Size = new System.Drawing.Size(504, 46);
            this.txtDescripcion.TabIndex = 12;
            //
            // lblObligatorios
            //
            this.lblObligatorios.AutoSize = true;
            this.lblObligatorios.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblObligatorios.Location = new System.Drawing.Point(12, 224);
            this.lblObligatorios.Name = "lblObligatorios";
            this.lblObligatorios.Size = new System.Drawing.Size(116, 15);
            this.lblObligatorios.TabIndex = 13;
            this.lblObligatorios.Text = "* Campos obligatorios";
            //
            // btnGuardar
            //
            this.btnGuardar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.btnGuardar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGuardar.FlatAppearance.BorderSize = 0;
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.btnGuardar.ForeColor = System.Drawing.Color.White;
            this.btnGuardar.Location = new System.Drawing.Point(184, 212);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(160, 40);
            this.btnGuardar.TabIndex = 14;
            this.btnGuardar.Text = "&Guardar producto";
            this.btnGuardar.UseVisualStyleBackColor = false;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            //
            // btnLimpiar
            //
            this.btnLimpiar.BackColor = System.Drawing.Color.White;
            this.btnLimpiar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLimpiar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(204)))), ((int)(((byte)(210)))));
            this.btnLimpiar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLimpiar.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.btnLimpiar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(48)))), ((int)(((byte)(54)))));
            this.btnLimpiar.Location = new System.Drawing.Point(352, 212);
            this.btnLimpiar.Name = "btnLimpiar";
            this.btnLimpiar.Size = new System.Drawing.Size(160, 40);
            this.btnLimpiar.TabIndex = 15;
            this.btnLimpiar.Text = "&Limpiar campos";
            this.btnLimpiar.UseVisualStyleBackColor = false;
            this.btnLimpiar.Click += new System.EventHandler(this.btnLimpiar_Click);
            //
            // lblBuscar
            //
            this.lblBuscar.AutoSize = true;
            this.lblBuscar.Location = new System.Drawing.Point(560, 8);
            this.lblBuscar.Name = "lblBuscar";
            this.lblBuscar.Size = new System.Drawing.Size(45, 15);
            this.lblBuscar.TabIndex = 16;
            this.lblBuscar.Text = "&Buscar";
            //
            // txtBuscar
            //
            this.txtBuscar.Location = new System.Drawing.Point(560, 26);
            this.txtBuscar.Name = "txtBuscar";
            this.txtBuscar.Size = new System.Drawing.Size(260, 23);
            this.txtBuscar.TabIndex = 17;
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
            this.tarjetaVerInactivos.TabIndex = 18;
            this.tarjetaVerInactivos.Click += new System.EventHandler(this.tarjetaVerInactivos_Click);
            //
            // tarjetaBaja
            //
            this.tarjetaBaja.Icono = TiendaUNNE.IconoAccion.BajaProducto;
            this.tarjetaBaja.Titulo = "Dar de baja";
            this.tarjetaBaja.Descripcion = "Al producto seleccionado";
            this.tarjetaBaja.Location = new System.Drawing.Point(560, 110);
            this.tarjetaBaja.Name = "tarjetaBaja";
            this.tarjetaBaja.Size = new System.Drawing.Size(260, 44);
            this.tarjetaBaja.TabIndex = 19;
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
            this.tarjetaActualizar.TabIndex = 20;
            this.tarjetaActualizar.Click += new System.EventHandler(this.tarjetaActualizar_Click);
            //
            // dgvProductos
            //
            this.dgvProductos.AllowUserToAddRows = false;
            this.dgvProductos.AllowUserToDeleteRows = false;
            this.dgvProductos.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvProductos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProductos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvProductos.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgvProductos.Location = new System.Drawing.Point(0, 266);
            this.dgvProductos.MultiSelect = false;
            this.dgvProductos.Name = "dgvProductos";
            this.dgvProductos.ReadOnly = true;
            this.dgvProductos.RowHeadersVisible = false;
            this.dgvProductos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvProductos.Size = new System.Drawing.Size(844, 245);
            this.dgvProductos.TabIndex = 1;
            this.dgvProductos.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvProductos_CellFormatting);
            this.dgvProductos.SelectionChanged += new System.EventHandler(this.dgvProductos_SelectionChanged);
            //
            // ucProductos
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dgvProductos);
            this.Controls.Add(this.panelFormulario);
            this.Name = "ucProductos";
            this.Size = new System.Drawing.Size(844, 501);
            this.Load += new System.EventHandler(this.ucProductos_Load);
            this.panelFormulario.ResumeLayout(false);
            this.panelFormulario.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPrecioVenta)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStock)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStockMinimo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProductos)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelFormulario;
        private System.Windows.Forms.Label lblTituloForm;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.Label lblCategoria;
        private System.Windows.Forms.ComboBox cboCategoria;
        private System.Windows.Forms.Label lblPrecioVenta;
        private System.Windows.Forms.NumericUpDown numPrecioVenta;
        private System.Windows.Forms.Label lblStock;
        private System.Windows.Forms.NumericUpDown numStock;
        private System.Windows.Forms.Label lblStockMinimo;
        private System.Windows.Forms.NumericUpDown numStockMinimo;
        private System.Windows.Forms.Label lblDescripcion;
        private System.Windows.Forms.TextBox txtDescripcion;
        private System.Windows.Forms.Label lblObligatorios;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnLimpiar;
        private System.Windows.Forms.Label lblBuscar;
        private System.Windows.Forms.TextBox txtBuscar;
        private TarjetaAccion tarjetaVerInactivos;
        private TarjetaAccion tarjetaBaja;
        private TarjetaAccion tarjetaActualizar;
        private System.Windows.Forms.DataGridView dgvProductos;
    }
}
