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
            this.panelAbierta = new System.Windows.Forms.Panel();
            this.panelCatalogo = new System.Windows.Forms.Panel();
            this.flowProductos = new System.Windows.Forms.FlowLayoutPanel();
            this.panelOpciones = new System.Windows.Forms.Panel();
            this.lblCuenta = new System.Windows.Forms.Label();
            this.chkOcultarSinStock = new System.Windows.Forms.CheckBox();
            this.flowCategorias = new System.Windows.Forms.FlowLayoutPanel();
            this.panelBusqueda = new System.Windows.Forms.Panel();
            this.txtBuscar = new System.Windows.Forms.TextBox();
            this.lblAtajoBuscar = new System.Windows.Forms.Label();
            this.panelTicket = new System.Windows.Forms.Panel();
            this.dgvTicket = new System.Windows.Forms.DataGridView();
            this.panelResumen = new System.Windows.Forms.Panel();
            this.lblTotalTitulo = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            this.btnCobrar = new System.Windows.Forms.Button();
            this.btnMenos = new System.Windows.Forms.Button();
            this.btnMas = new System.Windows.Forms.Button();
            this.btnQuitar = new System.Windows.Forms.Button();
            this.btnCancelarVenta = new System.Windows.Forms.Button();
            this.lblTituloTicket = new System.Windows.Forms.Label();
            this.panelEncabezado = new System.Windows.Forms.Panel();
            this.lblSesion = new System.Windows.Forms.Label();
            this.btnCerrarCaja = new System.Windows.Forms.Button();
            this.panelCerrada.SuspendLayout();
            this.panelAbierta.SuspendLayout();
            this.panelCatalogo.SuspendLayout();
            this.panelOpciones.SuspendLayout();
            this.panelBusqueda.SuspendLayout();
            this.panelTicket.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTicket)).BeginInit();
            this.panelResumen.SuspendLayout();
            this.panelEncabezado.SuspendLayout();
            this.SuspendLayout();
            //
            // panelCerrada
            //
            this.panelCerrada.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelCerrada.Location = new System.Drawing.Point(0, 0);
            this.panelCerrada.Name = "panelCerrada";
            this.panelCerrada.Size = new System.Drawing.Size(800, 560);
            this.panelCerrada.TabIndex = 0;
            //
            // panelAbierta
            //
            this.panelAbierta.Controls.Add(this.panelCatalogo);
            this.panelAbierta.Controls.Add(this.panelTicket);
            this.panelAbierta.Controls.Add(this.panelEncabezado);
            this.panelAbierta.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAbierta.Location = new System.Drawing.Point(0, 0);
            this.panelAbierta.Name = "panelAbierta";
            this.panelAbierta.Size = new System.Drawing.Size(800, 560);
            this.panelAbierta.TabIndex = 1;
            //
            // panelCatalogo
            //
            this.panelCatalogo.Controls.Add(this.flowProductos);
            this.panelCatalogo.Controls.Add(this.panelOpciones);
            this.panelCatalogo.Controls.Add(this.flowCategorias);
            this.panelCatalogo.Controls.Add(this.panelBusqueda);
            this.panelCatalogo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelCatalogo.Location = new System.Drawing.Point(0, 44);
            this.panelCatalogo.Name = "panelCatalogo";
            this.panelCatalogo.Padding = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.panelCatalogo.Size = new System.Drawing.Size(450, 516);
            this.panelCatalogo.TabIndex = 1;
            //
            // flowProductos
            //
            this.flowProductos.AutoScroll = true;
            this.flowProductos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowProductos.Location = new System.Drawing.Point(0, 118);
            this.flowProductos.Name = "flowProductos";
            this.flowProductos.Size = new System.Drawing.Size(438, 398);
            this.flowProductos.TabIndex = 3;
            //
            // panelOpciones
            //
            this.panelOpciones.Controls.Add(this.lblCuenta);
            this.panelOpciones.Controls.Add(this.chkOcultarSinStock);
            this.panelOpciones.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelOpciones.Location = new System.Drawing.Point(0, 90);
            this.panelOpciones.Name = "panelOpciones";
            this.panelOpciones.Size = new System.Drawing.Size(438, 28);
            this.panelOpciones.TabIndex = 2;
            //
            // lblCuenta
            //
            this.lblCuenta.AutoSize = true;
            this.lblCuenta.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(125)))), ((int)(((byte)(135)))));
            this.lblCuenta.Location = new System.Drawing.Point(180, 6);
            this.lblCuenta.Name = "lblCuenta";
            this.lblCuenta.Size = new System.Drawing.Size(0, 15);
            this.lblCuenta.TabIndex = 1;
            //
            // chkOcultarSinStock
            //
            this.chkOcultarSinStock.AutoSize = true;
            this.chkOcultarSinStock.Checked = true;
            this.chkOcultarSinStock.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOcultarSinStock.Location = new System.Drawing.Point(2, 5);
            this.chkOcultarSinStock.Name = "chkOcultarSinStock";
            this.chkOcultarSinStock.Size = new System.Drawing.Size(150, 19);
            this.chkOcultarSinStock.TabIndex = 0;
            this.chkOcultarSinStock.Text = "&Ocultar sin stock";
            this.chkOcultarSinStock.UseVisualStyleBackColor = true;
            this.chkOcultarSinStock.CheckedChanged += new System.EventHandler(this.chkOcultarSinStock_CheckedChanged);
            //
            // flowCategorias
            //
            this.flowCategorias.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowCategorias.Location = new System.Drawing.Point(0, 46);
            this.flowCategorias.Name = "flowCategorias";
            this.flowCategorias.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.flowCategorias.Size = new System.Drawing.Size(438, 44);
            this.flowCategorias.TabIndex = 1;
            //
            // panelBusqueda
            //
            this.panelBusqueda.Controls.Add(this.txtBuscar);
            this.panelBusqueda.Controls.Add(this.lblAtajoBuscar);
            this.panelBusqueda.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelBusqueda.Location = new System.Drawing.Point(0, 0);
            this.panelBusqueda.Name = "panelBusqueda";
            this.panelBusqueda.Size = new System.Drawing.Size(438, 46);
            this.panelBusqueda.TabIndex = 0;
            //
            // txtBuscar
            //
            this.txtBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBuscar.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtBuscar.Location = new System.Drawing.Point(0, 6);
            this.txtBuscar.MaxLength = 150;
            this.txtBuscar.Name = "txtBuscar";
            this.txtBuscar.Size = new System.Drawing.Size(390, 27);
            this.txtBuscar.TabIndex = 0;
            this.txtBuscar.TextChanged += new System.EventHandler(this.txtBuscar_TextChanged);
            this.txtBuscar.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtBuscar_KeyDown);
            //
            // lblAtajoBuscar
            //
            this.lblAtajoBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAtajoBuscar.AutoSize = true;
            this.lblAtajoBuscar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(125)))), ((int)(((byte)(135)))));
            this.lblAtajoBuscar.Location = new System.Drawing.Point(398, 12);
            this.lblAtajoBuscar.Name = "lblAtajoBuscar";
            this.lblAtajoBuscar.Size = new System.Drawing.Size(22, 15);
            this.lblAtajoBuscar.TabIndex = 1;
            this.lblAtajoBuscar.Text = "F2";
            //
            // panelTicket
            //
            this.panelTicket.BackColor = System.Drawing.Color.White;
            this.panelTicket.Controls.Add(this.dgvTicket);
            this.panelTicket.Controls.Add(this.panelResumen);
            this.panelTicket.Controls.Add(this.lblTituloTicket);
            this.panelTicket.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelTicket.Location = new System.Drawing.Point(450, 44);
            this.panelTicket.Name = "panelTicket";
            this.panelTicket.Size = new System.Drawing.Size(350, 516);
            this.panelTicket.TabIndex = 2;
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
            this.dgvTicket.Location = new System.Drawing.Point(0, 40);
            this.dgvTicket.MultiSelect = false;
            this.dgvTicket.Name = "dgvTicket";
            this.dgvTicket.ReadOnly = true;
            this.dgvTicket.RowHeadersVisible = false;
            this.dgvTicket.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTicket.Size = new System.Drawing.Size(350, 262);
            this.dgvTicket.TabIndex = 1;
            //
            // panelResumen
            //
            this.panelResumen.BackColor = System.Drawing.Color.White;
            this.panelResumen.Controls.Add(this.lblTotalTitulo);
            this.panelResumen.Controls.Add(this.lblTotal);
            this.panelResumen.Controls.Add(this.btnCobrar);
            this.panelResumen.Controls.Add(this.btnMenos);
            this.panelResumen.Controls.Add(this.btnMas);
            this.panelResumen.Controls.Add(this.btnQuitar);
            this.panelResumen.Controls.Add(this.btnCancelarVenta);
            this.panelResumen.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelResumen.Location = new System.Drawing.Point(0, 302);
            this.panelResumen.Name = "panelResumen";
            this.panelResumen.Size = new System.Drawing.Size(350, 214);
            this.panelResumen.TabIndex = 2;
            //
            // lblTotalTitulo
            //
            this.lblTotalTitulo.AutoSize = true;
            this.lblTotalTitulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(125)))), ((int)(((byte)(135)))));
            this.lblTotalTitulo.Location = new System.Drawing.Point(16, 10);
            this.lblTotalTitulo.Name = "lblTotalTitulo";
            this.lblTotalTitulo.Size = new System.Drawing.Size(33, 15);
            this.lblTotalTitulo.TabIndex = 0;
            this.lblTotalTitulo.Text = "Total";
            //
            // lblTotal
            //
            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.lblTotal.Location = new System.Drawing.Point(12, 26);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(90, 41);
            this.lblTotal.TabIndex = 1;
            this.lblTotal.Text = "0,00";
            //
            // btnCobrar
            //
            this.btnCobrar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(111)))), ((int)(((byte)(237)))));
            this.btnCobrar.FlatAppearance.BorderSize = 0;
            this.btnCobrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCobrar.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnCobrar.ForeColor = System.Drawing.Color.White;
            this.btnCobrar.Location = new System.Drawing.Point(16, 76);
            this.btnCobrar.Name = "btnCobrar";
            this.btnCobrar.Size = new System.Drawing.Size(318, 46);
            this.btnCobrar.TabIndex = 2;
            this.btnCobrar.Text = "C&obrar  (F4)";
            this.btnCobrar.UseVisualStyleBackColor = false;
            this.btnCobrar.EnabledChanged += new System.EventHandler(this.btnCobrar_EnabledChanged);
            this.btnCobrar.Click += new System.EventHandler(this.btnCobrar_Click);
            //
            // btnMenos
            //
            this.btnMenos.Location = new System.Drawing.Point(16, 130);
            this.btnMenos.Name = "btnMenos";
            this.btnMenos.Size = new System.Drawing.Size(100, 30);
            this.btnMenos.TabIndex = 3;
            this.btnMenos.Text = "− 1 unidad";
            this.btnMenos.UseVisualStyleBackColor = true;
            this.btnMenos.Click += new System.EventHandler(this.btnMenos_Click);
            //
            // btnMas
            //
            this.btnMas.Location = new System.Drawing.Point(122, 130);
            this.btnMas.Name = "btnMas";
            this.btnMas.Size = new System.Drawing.Size(100, 30);
            this.btnMas.TabIndex = 4;
            this.btnMas.Text = "+ 1 unidad";
            this.btnMas.UseVisualStyleBackColor = true;
            this.btnMas.Click += new System.EventHandler(this.btnMas_Click);
            //
            // btnQuitar
            //
            this.btnQuitar.Location = new System.Drawing.Point(228, 130);
            this.btnQuitar.Name = "btnQuitar";
            this.btnQuitar.Size = new System.Drawing.Size(106, 30);
            this.btnQuitar.TabIndex = 5;
            this.btnQuitar.Text = "&Quitar renglón";
            this.btnQuitar.UseVisualStyleBackColor = true;
            this.btnQuitar.Click += new System.EventHandler(this.btnQuitar_Click);
            //
            // btnCancelarVenta
            //
            this.btnCancelarVenta.Location = new System.Drawing.Point(16, 168);
            this.btnCancelarVenta.Name = "btnCancelarVenta";
            this.btnCancelarVenta.Size = new System.Drawing.Size(318, 32);
            this.btnCancelarVenta.TabIndex = 6;
            this.btnCancelarVenta.Text = "Cancelar &venta";
            this.btnCancelarVenta.UseVisualStyleBackColor = true;
            this.btnCancelarVenta.Click += new System.EventHandler(this.btnCancelarVenta_Click);
            //
            // lblTituloTicket
            //
            this.lblTituloTicket.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTituloTicket.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblTituloTicket.Location = new System.Drawing.Point(0, 0);
            this.lblTituloTicket.Name = "lblTituloTicket";
            this.lblTituloTicket.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
            this.lblTituloTicket.Size = new System.Drawing.Size(350, 40);
            this.lblTituloTicket.TabIndex = 0;
            this.lblTituloTicket.Text = "Ticket en curso";
            this.lblTituloTicket.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.panelAbierta.ResumeLayout(false);
            this.panelCatalogo.ResumeLayout(false);
            this.panelOpciones.ResumeLayout(false);
            this.panelOpciones.PerformLayout();
            this.panelBusqueda.ResumeLayout(false);
            this.panelBusqueda.PerformLayout();
            this.panelTicket.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTicket)).EndInit();
            this.panelResumen.ResumeLayout(false);
            this.panelResumen.PerformLayout();
            this.panelEncabezado.ResumeLayout(false);
            this.panelEncabezado.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelCerrada;
        private System.Windows.Forms.Panel panelAbierta;
        private System.Windows.Forms.Panel panelEncabezado;
        private System.Windows.Forms.Label lblSesion;
        private System.Windows.Forms.Button btnCerrarCaja;
        private System.Windows.Forms.Panel panelCatalogo;
        private System.Windows.Forms.Panel panelBusqueda;
        private System.Windows.Forms.TextBox txtBuscar;
        private System.Windows.Forms.Label lblAtajoBuscar;
        private System.Windows.Forms.FlowLayoutPanel flowCategorias;
        private System.Windows.Forms.Panel panelOpciones;
        private System.Windows.Forms.CheckBox chkOcultarSinStock;
        private System.Windows.Forms.Label lblCuenta;
        private System.Windows.Forms.FlowLayoutPanel flowProductos;
        private System.Windows.Forms.Panel panelTicket;
        private System.Windows.Forms.Label lblTituloTicket;
        private System.Windows.Forms.Panel panelResumen;
        private System.Windows.Forms.Label lblTotalTitulo;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Button btnCobrar;
        private System.Windows.Forms.Button btnMenos;
        private System.Windows.Forms.Button btnMas;
        private System.Windows.Forms.Button btnQuitar;
        private System.Windows.Forms.Button btnCancelarVenta;
        private System.Windows.Forms.DataGridView dgvTicket;
    }
}
