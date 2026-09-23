namespace TiendaUNNE
{
    partial class ucCaja
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
            this.panelCerrada = new System.Windows.Forms.Panel();
            this.lblTituloCerrada = new System.Windows.Forms.Label();
            this.lblAyudaCerrada = new System.Windows.Forms.Label();
            this.lblMontoInicial = new System.Windows.Forms.Label();
            this.numMontoInicial = new System.Windows.Forms.NumericUpDown();
            this.btnAbrirCaja = new System.Windows.Forms.Button();
            this.panelAbierta = new System.Windows.Forms.Panel();
            this.dgvTicket = new System.Windows.Forms.DataGridView();
            this.panelResumen = new System.Windows.Forms.Panel();
            this.lblTotalTitulo = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            this.btnCobrar = new System.Windows.Forms.Button();
            this.btnQuitar = new System.Windows.Forms.Button();
            this.btnCancelarVenta = new System.Windows.Forms.Button();
            this.panelBusqueda = new System.Windows.Forms.Panel();
            this.txtBuscar = new System.Windows.Forms.TextBox();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.cboProducto = new System.Windows.Forms.ComboBox();
            this.lblCantidad = new System.Windows.Forms.Label();
            this.numCantidad = new System.Windows.Forms.NumericUpDown();
            this.btnAgregar = new System.Windows.Forms.Button();
            this.lblInfoProducto = new System.Windows.Forms.Label();
            this.panelEncabezado = new System.Windows.Forms.Panel();
            this.lblSesion = new System.Windows.Forms.Label();
            this.btnCerrarCaja = new System.Windows.Forms.Button();
            this.panelCerrada.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMontoInicial)).BeginInit();
            this.panelAbierta.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTicket)).BeginInit();
            this.panelResumen.SuspendLayout();
            this.panelBusqueda.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCantidad)).BeginInit();
            this.panelEncabezado.SuspendLayout();
            this.SuspendLayout();
            //
            // panelCerrada
            //
            this.panelCerrada.Controls.Add(this.lblTituloCerrada);
            this.panelCerrada.Controls.Add(this.lblAyudaCerrada);
            this.panelCerrada.Controls.Add(this.lblMontoInicial);
            this.panelCerrada.Controls.Add(this.numMontoInicial);
            this.panelCerrada.Controls.Add(this.btnAbrirCaja);
            this.panelCerrada.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelCerrada.Location = new System.Drawing.Point(0, 0);
            this.panelCerrada.Name = "panelCerrada";
            this.panelCerrada.Padding = new System.Windows.Forms.Padding(24);
            this.panelCerrada.Size = new System.Drawing.Size(800, 560);
            this.panelCerrada.TabIndex = 0;
            //
            // lblTituloCerrada
            //
            this.lblTituloCerrada.AutoSize = true;
            this.lblTituloCerrada.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTituloCerrada.Location = new System.Drawing.Point(28, 28);
            this.lblTituloCerrada.Name = "lblTituloCerrada";
            this.lblTituloCerrada.Size = new System.Drawing.Size(180, 21);
            this.lblTituloCerrada.TabIndex = 0;
            this.lblTituloCerrada.Text = "La caja está cerrada";
            //
            // lblAyudaCerrada
            //
            this.lblAyudaCerrada.AutoSize = true;
            this.lblAyudaCerrada.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(125)))), ((int)(((byte)(135)))));
            this.lblAyudaCerrada.Location = new System.Drawing.Point(30, 56);
            this.lblAyudaCerrada.Name = "lblAyudaCerrada";
            this.lblAyudaCerrada.Size = new System.Drawing.Size(360, 15);
            this.lblAyudaCerrada.TabIndex = 1;
            this.lblAyudaCerrada.Text = "Para vender hay que abrirla declarando cuánto efectivo hay en el cajón.";
            //
            // lblMontoInicial
            //
            this.lblMontoInicial.AutoSize = true;
            this.lblMontoInicial.Location = new System.Drawing.Point(30, 96);
            this.lblMontoInicial.Name = "lblMontoInicial";
            this.lblMontoInicial.Size = new System.Drawing.Size(79, 15);
            this.lblMontoInicial.TabIndex = 2;
            this.lblMontoInicial.Text = "&Monto inicial";
            //
            // numMontoInicial
            //
            this.numMontoInicial.DecimalPlaces = 2;
            this.numMontoInicial.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.numMontoInicial.Location = new System.Drawing.Point(33, 116);
            this.numMontoInicial.Maximum = new decimal(new int[] { 10000000, 0, 0, 0});
            this.numMontoInicial.Name = "numMontoInicial";
            this.numMontoInicial.Size = new System.Drawing.Size(220, 27);
            this.numMontoInicial.TabIndex = 3;
            this.numMontoInicial.ThousandsSeparator = true;
            this.numMontoInicial.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            //
            // btnAbrirCaja
            //
            this.btnAbrirCaja.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnAbrirCaja.Location = new System.Drawing.Point(33, 158);
            this.btnAbrirCaja.Name = "btnAbrirCaja";
            this.btnAbrirCaja.Size = new System.Drawing.Size(220, 38);
            this.btnAbrirCaja.TabIndex = 4;
            this.btnAbrirCaja.Text = "&Abrir caja";
            this.btnAbrirCaja.UseVisualStyleBackColor = true;
            this.btnAbrirCaja.Click += new System.EventHandler(this.btnAbrirCaja_Click);
            //
            // panelAbierta
            //
            this.panelAbierta.Controls.Add(this.dgvTicket);
            this.panelAbierta.Controls.Add(this.panelResumen);
            this.panelAbierta.Controls.Add(this.panelBusqueda);
            this.panelAbierta.Controls.Add(this.panelEncabezado);
            this.panelAbierta.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAbierta.Location = new System.Drawing.Point(0, 0);
            this.panelAbierta.Name = "panelAbierta";
            this.panelAbierta.Size = new System.Drawing.Size(800, 560);
            this.panelAbierta.TabIndex = 1;
            //
            // dgvTicket
            //
            this.dgvTicket.AllowUserToAddRows = false;
            this.dgvTicket.AllowUserToDeleteRows = false;
            this.dgvTicket.AllowUserToResizeRows = false;
            this.dgvTicket.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvTicket.BackgroundColor = System.Drawing.Color.White;
            this.dgvTicket.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvTicket.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTicket.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTicket.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgvTicket.Location = new System.Drawing.Point(0, 128);
            this.dgvTicket.MultiSelect = false;
            this.dgvTicket.Name = "dgvTicket";
            this.dgvTicket.ReadOnly = true;
            this.dgvTicket.RowHeadersVisible = false;
            this.dgvTicket.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTicket.Size = new System.Drawing.Size(590, 432);
            this.dgvTicket.TabIndex = 3;
            //
            // panelResumen
            //
            this.panelResumen.BackColor = System.Drawing.Color.White;
            this.panelResumen.Controls.Add(this.lblTotalTitulo);
            this.panelResumen.Controls.Add(this.lblTotal);
            this.panelResumen.Controls.Add(this.btnCobrar);
            this.panelResumen.Controls.Add(this.btnQuitar);
            this.panelResumen.Controls.Add(this.btnCancelarVenta);
            this.panelResumen.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelResumen.Location = new System.Drawing.Point(590, 128);
            this.panelResumen.Name = "panelResumen";
            this.panelResumen.Padding = new System.Windows.Forms.Padding(16);
            this.panelResumen.Size = new System.Drawing.Size(210, 432);
            this.panelResumen.TabIndex = 2;
            //
            // lblTotalTitulo
            //
            this.lblTotalTitulo.AutoSize = true;
            this.lblTotalTitulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(125)))), ((int)(((byte)(135)))));
            this.lblTotalTitulo.Location = new System.Drawing.Point(18, 18);
            this.lblTotalTitulo.Name = "lblTotalTitulo";
            this.lblTotalTitulo.Size = new System.Drawing.Size(33, 15);
            this.lblTotalTitulo.TabIndex = 0;
            this.lblTotalTitulo.Text = "Total";
            //
            // lblTotal
            //
            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblTotal.Location = new System.Drawing.Point(14, 36);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(80, 37);
            this.lblTotal.TabIndex = 1;
            this.lblTotal.Text = "0,00";
            //
            // btnCobrar
            //
            this.btnCobrar.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.btnCobrar.Location = new System.Drawing.Point(18, 90);
            this.btnCobrar.Name = "btnCobrar";
            this.btnCobrar.Size = new System.Drawing.Size(174, 46);
            this.btnCobrar.TabIndex = 2;
            this.btnCobrar.Text = "C&obrar";
            this.btnCobrar.UseVisualStyleBackColor = true;
            this.btnCobrar.Click += new System.EventHandler(this.btnCobrar_Click);
            //
            // btnQuitar
            //
            this.btnQuitar.Location = new System.Drawing.Point(18, 144);
            this.btnQuitar.Name = "btnQuitar";
            this.btnQuitar.Size = new System.Drawing.Size(174, 32);
            this.btnQuitar.TabIndex = 3;
            this.btnQuitar.Text = "&Quitar renglón";
            this.btnQuitar.UseVisualStyleBackColor = true;
            this.btnQuitar.Click += new System.EventHandler(this.btnQuitar_Click);
            //
            // btnCancelarVenta
            //
            this.btnCancelarVenta.Location = new System.Drawing.Point(18, 182);
            this.btnCancelarVenta.Name = "btnCancelarVenta";
            this.btnCancelarVenta.Size = new System.Drawing.Size(174, 32);
            this.btnCancelarVenta.TabIndex = 4;
            this.btnCancelarVenta.Text = "Cancelar &venta";
            this.btnCancelarVenta.UseVisualStyleBackColor = true;
            this.btnCancelarVenta.Click += new System.EventHandler(this.btnCancelarVenta_Click);
            //
            // panelBusqueda
            //
            this.panelBusqueda.Controls.Add(this.txtBuscar);
            this.panelBusqueda.Controls.Add(this.btnBuscar);
            this.panelBusqueda.Controls.Add(this.cboProducto);
            this.panelBusqueda.Controls.Add(this.lblCantidad);
            this.panelBusqueda.Controls.Add(this.numCantidad);
            this.panelBusqueda.Controls.Add(this.btnAgregar);
            this.panelBusqueda.Controls.Add(this.lblInfoProducto);
            this.panelBusqueda.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelBusqueda.Location = new System.Drawing.Point(0, 44);
            this.panelBusqueda.Name = "panelBusqueda";
            this.panelBusqueda.Size = new System.Drawing.Size(800, 84);
            this.panelBusqueda.TabIndex = 1;
            //
            // txtBuscar
            //
            this.txtBuscar.Location = new System.Drawing.Point(0, 6);
            this.txtBuscar.MaxLength = 150;
            this.txtBuscar.Name = "txtBuscar";
            this.txtBuscar.Size = new System.Drawing.Size(240, 23);
            this.txtBuscar.TabIndex = 0;
            this.txtBuscar.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtBuscar_KeyDown);
            //
            // btnBuscar
            //
            this.btnBuscar.Location = new System.Drawing.Point(246, 5);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Size = new System.Drawing.Size(80, 25);
            this.btnBuscar.TabIndex = 1;
            this.btnBuscar.Text = "&Buscar";
            this.btnBuscar.UseVisualStyleBackColor = true;
            this.btnBuscar.Click += new System.EventHandler(this.btnBuscar_Click);
            //
            // cboProducto
            //
            this.cboProducto.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboProducto.Location = new System.Drawing.Point(0, 42);
            this.cboProducto.Name = "cboProducto";
            this.cboProducto.Size = new System.Drawing.Size(326, 23);
            this.cboProducto.TabIndex = 2;
            this.cboProducto.SelectedIndexChanged += new System.EventHandler(this.cboProducto_SelectedIndexChanged);
            //
            // lblCantidad
            //
            this.lblCantidad.AutoSize = true;
            this.lblCantidad.Location = new System.Drawing.Point(340, 24);
            this.lblCantidad.Name = "lblCantidad";
            this.lblCantidad.Size = new System.Drawing.Size(58, 15);
            this.lblCantidad.TabIndex = 3;
            this.lblCantidad.Text = "&Cantidad";
            //
            // numCantidad
            //
            this.numCantidad.DecimalPlaces = 3;
            this.numCantidad.Location = new System.Drawing.Point(340, 42);
            this.numCantidad.Maximum = new decimal(new int[] { 9999, 0, 0, 0});
            this.numCantidad.Minimum = new decimal(new int[] { 1, 0, 0, 196608});
            this.numCantidad.Name = "numCantidad";
            this.numCantidad.Size = new System.Drawing.Size(90, 23);
            this.numCantidad.TabIndex = 4;
            this.numCantidad.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numCantidad.Value = new decimal(new int[] { 1, 0, 0, 0});
            //
            // btnAgregar
            //
            this.btnAgregar.Location = new System.Drawing.Point(440, 41);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Size = new System.Drawing.Size(110, 25);
            this.btnAgregar.TabIndex = 5;
            this.btnAgregar.Text = "A&gregar al ticket";
            this.btnAgregar.UseVisualStyleBackColor = true;
            this.btnAgregar.Click += new System.EventHandler(this.btnAgregar_Click);
            //
            // lblInfoProducto
            //
            this.lblInfoProducto.AutoSize = true;
            this.lblInfoProducto.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(125)))), ((int)(((byte)(135)))));
            this.lblInfoProducto.Location = new System.Drawing.Point(562, 46);
            this.lblInfoProducto.Name = "lblInfoProducto";
            this.lblInfoProducto.Size = new System.Drawing.Size(0, 15);
            this.lblInfoProducto.TabIndex = 6;
            //
            // panelEncabezado
            //
            this.panelEncabezado.Controls.Add(this.lblSesion);
            this.panelEncabezado.Controls.Add(this.btnCerrarCaja);
            this.panelEncabezado.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelEncabezado.Location = new System.Drawing.Point(0, 0);
            this.panelEncabezado.Name = "panelEncabezado";
            this.panelEncabezado.Size = new System.Drawing.Size(800, 44);
            this.panelEncabezado.TabIndex = 0;
            //
            // lblSesion
            //
            this.lblSesion.AutoSize = true;
            this.lblSesion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(125)))), ((int)(((byte)(135)))));
            this.lblSesion.Location = new System.Drawing.Point(0, 12);
            this.lblSesion.Name = "lblSesion";
            this.lblSesion.Size = new System.Drawing.Size(0, 15);
            this.lblSesion.TabIndex = 0;
            //
            // btnCerrarCaja
            //
            this.btnCerrarCaja.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrarCaja.Location = new System.Drawing.Point(666, 6);
            this.btnCerrarCaja.Name = "btnCerrarCaja";
            this.btnCerrarCaja.Size = new System.Drawing.Size(126, 28);
            this.btnCerrarCaja.TabIndex = 1;
            this.btnCerrarCaja.Text = "Ce&rrar caja";
            this.btnCerrarCaja.UseVisualStyleBackColor = true;
            this.btnCerrarCaja.Click += new System.EventHandler(this.btnCerrarCaja_Click);
            //
            // ucCaja
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelCerrada);
            this.Controls.Add(this.panelAbierta);
            this.Name = "ucCaja";
            this.Size = new System.Drawing.Size(800, 560);
            this.Load += new System.EventHandler(this.ucCaja_Load);
            this.panelCerrada.ResumeLayout(false);
            this.panelCerrada.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMontoInicial)).EndInit();
            this.panelAbierta.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTicket)).EndInit();
            this.panelResumen.ResumeLayout(false);
            this.panelResumen.PerformLayout();
            this.panelBusqueda.ResumeLayout(false);
            this.panelBusqueda.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCantidad)).EndInit();
            this.panelEncabezado.ResumeLayout(false);
            this.panelEncabezado.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelCerrada;
        private System.Windows.Forms.Label lblTituloCerrada;
        private System.Windows.Forms.Label lblAyudaCerrada;
        private System.Windows.Forms.Label lblMontoInicial;
        private System.Windows.Forms.NumericUpDown numMontoInicial;
        private System.Windows.Forms.Button btnAbrirCaja;
        private System.Windows.Forms.Panel panelAbierta;
        private System.Windows.Forms.Panel panelEncabezado;
        private System.Windows.Forms.Label lblSesion;
        private System.Windows.Forms.Button btnCerrarCaja;
        private System.Windows.Forms.Panel panelBusqueda;
        private System.Windows.Forms.TextBox txtBuscar;
        private System.Windows.Forms.Button btnBuscar;
        private System.Windows.Forms.ComboBox cboProducto;
        private System.Windows.Forms.Label lblCantidad;
        private System.Windows.Forms.NumericUpDown numCantidad;
        private System.Windows.Forms.Button btnAgregar;
        private System.Windows.Forms.Label lblInfoProducto;
        private System.Windows.Forms.Panel panelResumen;
        private System.Windows.Forms.Label lblTotalTitulo;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Button btnCobrar;
        private System.Windows.Forms.Button btnQuitar;
        private System.Windows.Forms.Button btnCancelarVenta;
        private System.Windows.Forms.DataGridView dgvTicket;
    }
}
