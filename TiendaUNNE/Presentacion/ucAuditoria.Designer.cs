namespace TiendaUNNE
{
    partial class ucAuditoria
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
            this.panelFiltros = new System.Windows.Forms.Panel();
            this.lblTabla = new System.Windows.Forms.Label();
            this.cboTabla = new System.Windows.Forms.ComboBox();
            this.lblUsuario = new System.Windows.Forms.Label();
            this.cboUsuario = new System.Windows.Forms.ComboBox();
            this.lblAccion = new System.Windows.Forms.Label();
            this.cboAccion = new System.Windows.Forms.ComboBox();
            this.lblBuscar = new System.Windows.Forms.Label();
            this.txtBuscar = new System.Windows.Forms.TextBox();
            this.lblDesde = new System.Windows.Forms.Label();
            this.dtpDesde = new System.Windows.Forms.DateTimePicker();
            this.lblHasta = new System.Windows.Forms.Label();
            this.dtpHasta = new System.Windows.Forms.DateTimePicker();
            this.btnHoy = new System.Windows.Forms.Button();
            this.btnSemana = new System.Windows.Forms.Button();
            this.btnMes = new System.Windows.Forms.Button();
            this.btnFiltrar = new System.Windows.Forms.Button();
            this.btnLimpiar = new System.Windows.Forms.Button();
            this.btnActualizar = new System.Windows.Forms.Button();
            this.dgvAuditoria = new System.Windows.Forms.DataGridView();
            this.panelFiltros.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAuditoria)).BeginInit();
            this.SuspendLayout();
            //
            // panelFiltros
            //
            this.panelFiltros.Controls.Add(this.lblTabla);
            this.panelFiltros.Controls.Add(this.cboTabla);
            this.panelFiltros.Controls.Add(this.lblUsuario);
            this.panelFiltros.Controls.Add(this.cboUsuario);
            this.panelFiltros.Controls.Add(this.lblAccion);
            this.panelFiltros.Controls.Add(this.cboAccion);
            this.panelFiltros.Controls.Add(this.lblBuscar);
            this.panelFiltros.Controls.Add(this.txtBuscar);
            this.panelFiltros.Controls.Add(this.lblDesde);
            this.panelFiltros.Controls.Add(this.dtpDesde);
            this.panelFiltros.Controls.Add(this.lblHasta);
            this.panelFiltros.Controls.Add(this.dtpHasta);
            this.panelFiltros.Controls.Add(this.btnHoy);
            this.panelFiltros.Controls.Add(this.btnSemana);
            this.panelFiltros.Controls.Add(this.btnMes);
            this.panelFiltros.Controls.Add(this.btnFiltrar);
            this.panelFiltros.Controls.Add(this.btnLimpiar);
            this.panelFiltros.Controls.Add(this.btnActualizar);
            this.panelFiltros.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFiltros.Location = new System.Drawing.Point(0, 0);
            this.panelFiltros.Name = "panelFiltros";
            this.panelFiltros.Size = new System.Drawing.Size(904, 116);
            this.panelFiltros.TabIndex = 0;
            //
            // lblTabla  (ahora se llama Módulo)
            //
            this.lblTabla.AutoSize = true;
            this.lblTabla.Location = new System.Drawing.Point(14, 8);
            this.lblTabla.Name = "lblTabla";
            this.lblTabla.Size = new System.Drawing.Size(47, 15);
            this.lblTabla.TabIndex = 0;
            this.lblTabla.Text = "&Módulo";
            //
            // cboTabla
            //
            this.cboTabla.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTabla.FormattingEnabled = true;
            this.cboTabla.Location = new System.Drawing.Point(14, 28);
            this.cboTabla.Name = "cboTabla";
            this.cboTabla.Size = new System.Drawing.Size(150, 23);
            this.cboTabla.TabIndex = 1;
            //
            // lblUsuario
            //
            this.lblUsuario.AutoSize = true;
            this.lblUsuario.Location = new System.Drawing.Point(180, 8);
            this.lblUsuario.Name = "lblUsuario";
            this.lblUsuario.Size = new System.Drawing.Size(47, 15);
            this.lblUsuario.TabIndex = 2;
            this.lblUsuario.Text = "&Usuario";
            //
            // cboUsuario
            //
            this.cboUsuario.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboUsuario.FormattingEnabled = true;
            this.cboUsuario.Location = new System.Drawing.Point(180, 28);
            this.cboUsuario.Name = "cboUsuario";
            this.cboUsuario.Size = new System.Drawing.Size(190, 23);
            this.cboUsuario.TabIndex = 3;
            //
            // lblAccion
            //
            this.lblAccion.AutoSize = true;
            this.lblAccion.Location = new System.Drawing.Point(386, 8);
            this.lblAccion.Name = "lblAccion";
            this.lblAccion.Size = new System.Drawing.Size(43, 15);
            this.lblAccion.TabIndex = 4;
            this.lblAccion.Text = "A&cción";
            //
            // cboAccion
            //
            this.cboAccion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAccion.FormattingEnabled = true;
            this.cboAccion.Location = new System.Drawing.Point(386, 28);
            this.cboAccion.Name = "cboAccion";
            this.cboAccion.Size = new System.Drawing.Size(130, 23);
            this.cboAccion.TabIndex = 5;
            //
            // lblBuscar
            //
            this.lblBuscar.AutoSize = true;
            this.lblBuscar.Location = new System.Drawing.Point(532, 8);
            this.lblBuscar.Name = "lblBuscar";
            this.lblBuscar.Size = new System.Drawing.Size(38, 15);
            this.lblBuscar.TabIndex = 6;
            this.lblBuscar.Text = "&Buscar";
            //
            // txtBuscar
            //
            this.txtBuscar.Location = new System.Drawing.Point(532, 28);
            this.txtBuscar.Name = "txtBuscar";
            this.txtBuscar.Size = new System.Drawing.Size(250, 23);
            this.txtBuscar.TabIndex = 7;
            //
            // lblDesde
            //
            this.lblDesde.AutoSize = true;
            this.lblDesde.Location = new System.Drawing.Point(14, 62);
            this.lblDesde.Name = "lblDesde";
            this.lblDesde.Size = new System.Drawing.Size(41, 15);
            this.lblDesde.TabIndex = 8;
            this.lblDesde.Text = "&Desde";
            //
            // dtpDesde
            //
            this.dtpDesde.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpDesde.Location = new System.Drawing.Point(14, 82);
            this.dtpDesde.Name = "dtpDesde";
            this.dtpDesde.ShowCheckBox = true;
            this.dtpDesde.Size = new System.Drawing.Size(120, 23);
            this.dtpDesde.TabIndex = 9;
            //
            // lblHasta
            //
            this.lblHasta.AutoSize = true;
            this.lblHasta.Location = new System.Drawing.Point(150, 62);
            this.lblHasta.Name = "lblHasta";
            this.lblHasta.Size = new System.Drawing.Size(37, 15);
            this.lblHasta.TabIndex = 10;
            this.lblHasta.Text = "&Hasta";
            //
            // dtpHasta
            //
            this.dtpHasta.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpHasta.Location = new System.Drawing.Point(150, 82);
            this.dtpHasta.Name = "dtpHasta";
            this.dtpHasta.ShowCheckBox = true;
            this.dtpHasta.Size = new System.Drawing.Size(120, 23);
            this.dtpHasta.TabIndex = 11;
            //
            // btnHoy
            //
            this.btnHoy.Location = new System.Drawing.Point(290, 81);
            this.btnHoy.Name = "btnHoy";
            this.btnHoy.Size = new System.Drawing.Size(60, 26);
            this.btnHoy.TabIndex = 12;
            this.btnHoy.Text = "Hoy";
            this.btnHoy.UseVisualStyleBackColor = true;
            this.btnHoy.Click += new System.EventHandler(this.btnHoy_Click);
            //
            // btnSemana
            //
            this.btnSemana.Location = new System.Drawing.Point(356, 81);
            this.btnSemana.Name = "btnSemana";
            this.btnSemana.Size = new System.Drawing.Size(70, 26);
            this.btnSemana.TabIndex = 13;
            this.btnSemana.Text = "7 días";
            this.btnSemana.UseVisualStyleBackColor = true;
            this.btnSemana.Click += new System.EventHandler(this.btnSemana_Click);
            //
            // btnMes
            //
            this.btnMes.Location = new System.Drawing.Point(432, 81);
            this.btnMes.Name = "btnMes";
            this.btnMes.Size = new System.Drawing.Size(70, 26);
            this.btnMes.TabIndex = 14;
            this.btnMes.Text = "30 días";
            this.btnMes.UseVisualStyleBackColor = true;
            this.btnMes.Click += new System.EventHandler(this.btnMes_Click);
            //
            // btnFiltrar
            //
            this.btnFiltrar.Location = new System.Drawing.Point(532, 81);
            this.btnFiltrar.Name = "btnFiltrar";
            this.btnFiltrar.Size = new System.Drawing.Size(90, 26);
            this.btnFiltrar.TabIndex = 15;
            this.btnFiltrar.Text = "&Filtrar";
            this.btnFiltrar.UseVisualStyleBackColor = true;
            this.btnFiltrar.Click += new System.EventHandler(this.btnFiltrar_Click);
            //
            // btnLimpiar
            //
            this.btnLimpiar.Location = new System.Drawing.Point(628, 81);
            this.btnLimpiar.Name = "btnLimpiar";
            this.btnLimpiar.Size = new System.Drawing.Size(120, 26);
            this.btnLimpiar.TabIndex = 16;
            this.btnLimpiar.Text = "&Limpiar filtros";
            this.btnLimpiar.UseVisualStyleBackColor = true;
            this.btnLimpiar.Click += new System.EventHandler(this.btnLimpiar_Click);
            //
            // btnActualizar
            //
            this.btnActualizar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnActualizar.Location = new System.Drawing.Point(752, 81);
            this.btnActualizar.Name = "btnActualizar";
            this.btnActualizar.Size = new System.Drawing.Size(140, 26);
            this.btnActualizar.TabIndex = 17;
            this.btnActualizar.Text = "&Actualizar listado";
            this.btnActualizar.UseVisualStyleBackColor = true;
            this.btnActualizar.Click += new System.EventHandler(this.btnActualizar_Click);
            //
            // dgvAuditoria
            //
            this.dgvAuditoria.AllowUserToAddRows = false;
            this.dgvAuditoria.AllowUserToDeleteRows = false;
            this.dgvAuditoria.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvAuditoria.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAuditoria.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAuditoria.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgvAuditoria.Location = new System.Drawing.Point(0, 116);
            this.dgvAuditoria.MultiSelect = false;
            this.dgvAuditoria.Name = "dgvAuditoria";
            this.dgvAuditoria.ReadOnly = true;
            this.dgvAuditoria.RowHeadersVisible = false;
            this.dgvAuditoria.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAuditoria.Size = new System.Drawing.Size(904, 405);
            this.dgvAuditoria.TabIndex = 1;
            //
            // ucAuditoria
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dgvAuditoria);
            this.Controls.Add(this.panelFiltros);
            this.Name = "ucAuditoria";
            this.Size = new System.Drawing.Size(904, 521);
            this.Load += new System.EventHandler(this.ucAuditoria_Load);
            this.panelFiltros.ResumeLayout(false);
            this.panelFiltros.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAuditoria)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelFiltros;
        private System.Windows.Forms.Label lblTabla;
        private System.Windows.Forms.ComboBox cboTabla;
        private System.Windows.Forms.Label lblUsuario;
        private System.Windows.Forms.ComboBox cboUsuario;
        private System.Windows.Forms.Label lblAccion;
        private System.Windows.Forms.ComboBox cboAccion;
        private System.Windows.Forms.Label lblBuscar;
        private System.Windows.Forms.TextBox txtBuscar;
        private System.Windows.Forms.Label lblDesde;
        private System.Windows.Forms.DateTimePicker dtpDesde;
        private System.Windows.Forms.Label lblHasta;
        private System.Windows.Forms.DateTimePicker dtpHasta;
        private System.Windows.Forms.Button btnHoy;
        private System.Windows.Forms.Button btnSemana;
        private System.Windows.Forms.Button btnMes;
        private System.Windows.Forms.Button btnFiltrar;
        private System.Windows.Forms.Button btnLimpiar;
        private System.Windows.Forms.Button btnActualizar;
        private System.Windows.Forms.DataGridView dgvAuditoria;
    }
}
