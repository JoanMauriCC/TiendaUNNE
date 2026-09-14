namespace TiendaUNNE
{
    partial class ucReportes
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
            this.panelBarra = new System.Windows.Forms.Panel();
            this.btnVentas = new System.Windows.Forms.Button();
            this.btnProductos = new System.Windows.Forms.Button();
            this.btnRecaudacion = new System.Windows.Forms.Button();
            this.lblPeriodo = new System.Windows.Forms.Label();
            this.cboPeriodo = new System.Windows.Forms.ComboBox();
            this.tblIndicadores = new System.Windows.Forms.TableLayoutPanel();
            this.panelIndicador1 = new System.Windows.Forms.Panel();
            this.lblIndTitulo1 = new System.Windows.Forms.Label();
            this.lblIndValor1 = new System.Windows.Forms.Label();
            this.panelIndicador2 = new System.Windows.Forms.Panel();
            this.lblIndTitulo2 = new System.Windows.Forms.Label();
            this.lblIndValor2 = new System.Windows.Forms.Label();
            this.panelIndicador3 = new System.Windows.Forms.Panel();
            this.lblIndTitulo3 = new System.Windows.Forms.Label();
            this.lblIndValor3 = new System.Windows.Forms.Label();
            this.tblCuerpo = new System.Windows.Forms.TableLayoutPanel();
            this.panelGrafico = new System.Windows.Forms.Panel();
            this.lblTituloGrafico = new System.Windows.Forms.Label();
            this.dgvDetalle = new System.Windows.Forms.DataGridView();
            this.lblAviso = new System.Windows.Forms.Label();
            this.panelBarra.SuspendLayout();
            this.tblIndicadores.SuspendLayout();
            this.panelIndicador1.SuspendLayout();
            this.panelIndicador2.SuspendLayout();
            this.panelIndicador3.SuspendLayout();
            this.tblCuerpo.SuspendLayout();
            this.panelGrafico.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetalle)).BeginInit();
            this.SuspendLayout();
            //
            // panelBarra
            //
            this.panelBarra.Controls.Add(this.btnVentas);
            this.panelBarra.Controls.Add(this.btnProductos);
            this.panelBarra.Controls.Add(this.btnRecaudacion);
            this.panelBarra.Controls.Add(this.lblPeriodo);
            this.panelBarra.Controls.Add(this.cboPeriodo);
            this.panelBarra.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelBarra.Location = new System.Drawing.Point(0, 0);
            this.panelBarra.Name = "panelBarra";
            this.panelBarra.Size = new System.Drawing.Size(800, 46);
            this.panelBarra.TabIndex = 0;
            //
            // btnVentas
            //
            this.btnVentas.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVentas.Location = new System.Drawing.Point(6, 8);
            this.btnVentas.Name = "btnVentas";
            this.btnVentas.Size = new System.Drawing.Size(110, 30);
            this.btnVentas.TabIndex = 0;
            this.btnVentas.Text = "Ventas";
            this.btnVentas.UseVisualStyleBackColor = false;
            this.btnVentas.Click += new System.EventHandler(this.btnVentas_Click);
            //
            // btnProductos
            //
            this.btnProductos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnProductos.Location = new System.Drawing.Point(122, 8);
            this.btnProductos.Name = "btnProductos";
            this.btnProductos.Size = new System.Drawing.Size(110, 30);
            this.btnProductos.TabIndex = 1;
            this.btnProductos.Text = "Productos";
            this.btnProductos.UseVisualStyleBackColor = false;
            this.btnProductos.Click += new System.EventHandler(this.btnProductos_Click);
            //
            // btnRecaudacion
            //
            this.btnRecaudacion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRecaudacion.Location = new System.Drawing.Point(238, 8);
            this.btnRecaudacion.Name = "btnRecaudacion";
            this.btnRecaudacion.Size = new System.Drawing.Size(120, 30);
            this.btnRecaudacion.TabIndex = 2;
            this.btnRecaudacion.Text = "Recaudación";
            this.btnRecaudacion.UseVisualStyleBackColor = false;
            this.btnRecaudacion.Click += new System.EventHandler(this.btnRecaudacion_Click);
            //
            // lblPeriodo
            //
            this.lblPeriodo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPeriodo.AutoSize = true;
            this.lblPeriodo.Location = new System.Drawing.Point(608, 16);
            this.lblPeriodo.Name = "lblPeriodo";
            this.lblPeriodo.Size = new System.Drawing.Size(51, 15);
            this.lblPeriodo.TabIndex = 3;
            this.lblPeriodo.Text = "Período";
            //
            // cboPeriodo
            //
            this.cboPeriodo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboPeriodo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPeriodo.Location = new System.Drawing.Point(664, 12);
            this.cboPeriodo.Name = "cboPeriodo";
            this.cboPeriodo.Size = new System.Drawing.Size(130, 23);
            this.cboPeriodo.TabIndex = 4;
            this.cboPeriodo.SelectedIndexChanged += new System.EventHandler(this.cboPeriodo_SelectedIndexChanged);
            //
            // tblIndicadores
            //
            this.tblIndicadores.ColumnCount = 3;
            this.tblIndicadores.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tblIndicadores.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tblIndicadores.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.34F));
            this.tblIndicadores.Controls.Add(this.panelIndicador1, 0, 0);
            this.tblIndicadores.Controls.Add(this.panelIndicador2, 1, 0);
            this.tblIndicadores.Controls.Add(this.panelIndicador3, 2, 0);
            this.tblIndicadores.Dock = System.Windows.Forms.DockStyle.Top;
            this.tblIndicadores.Location = new System.Drawing.Point(0, 46);
            this.tblIndicadores.Name = "tblIndicadores";
            this.tblIndicadores.RowCount = 1;
            this.tblIndicadores.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblIndicadores.Size = new System.Drawing.Size(800, 80);
            this.tblIndicadores.TabIndex = 1;
            //
            // panelIndicador1
            //
            this.panelIndicador1.BackColor = System.Drawing.Color.White;
            this.panelIndicador1.Controls.Add(this.lblIndTitulo1);
            this.panelIndicador1.Controls.Add(this.lblIndValor1);
            this.panelIndicador1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelIndicador1.Margin = new System.Windows.Forms.Padding(6);
            this.panelIndicador1.Name = "panelIndicador1";
            this.panelIndicador1.TabIndex = 0;
            //
            // lblIndTitulo1
            //
            this.lblIndTitulo1.AutoSize = true;
            this.lblIndTitulo1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(125)))), ((int)(((byte)(135)))));
            this.lblIndTitulo1.Location = new System.Drawing.Point(12, 10);
            this.lblIndTitulo1.Name = "lblIndTitulo1";
            this.lblIndTitulo1.TabIndex = 0;
            //
            // lblIndValor1
            //
            this.lblIndValor1.AutoSize = true;
            this.lblIndValor1.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblIndValor1.Location = new System.Drawing.Point(9, 28);
            this.lblIndValor1.Name = "lblIndValor1";
            this.lblIndValor1.TabIndex = 1;
            //
            // panelIndicador2
            //
            this.panelIndicador2.BackColor = System.Drawing.Color.White;
            this.panelIndicador2.Controls.Add(this.lblIndTitulo2);
            this.panelIndicador2.Controls.Add(this.lblIndValor2);
            this.panelIndicador2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelIndicador2.Margin = new System.Windows.Forms.Padding(6);
            this.panelIndicador2.Name = "panelIndicador2";
            this.panelIndicador2.TabIndex = 1;
            //
            // lblIndTitulo2
            //
            this.lblIndTitulo2.AutoSize = true;
            this.lblIndTitulo2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(125)))), ((int)(((byte)(135)))));
            this.lblIndTitulo2.Location = new System.Drawing.Point(12, 10);
            this.lblIndTitulo2.Name = "lblIndTitulo2";
            this.lblIndTitulo2.TabIndex = 0;
            //
            // lblIndValor2
            //
            this.lblIndValor2.AutoSize = true;
            this.lblIndValor2.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblIndValor2.Location = new System.Drawing.Point(9, 28);
            this.lblIndValor2.Name = "lblIndValor2";
            this.lblIndValor2.TabIndex = 1;
            //
            // panelIndicador3
            //
            this.panelIndicador3.BackColor = System.Drawing.Color.White;
            this.panelIndicador3.Controls.Add(this.lblIndTitulo3);
            this.panelIndicador3.Controls.Add(this.lblIndValor3);
            this.panelIndicador3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelIndicador3.Margin = new System.Windows.Forms.Padding(6);
            this.panelIndicador3.Name = "panelIndicador3";
            this.panelIndicador3.TabIndex = 2;
            //
            // lblIndTitulo3
            //
            this.lblIndTitulo3.AutoSize = true;
            this.lblIndTitulo3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(125)))), ((int)(((byte)(135)))));
            this.lblIndTitulo3.Location = new System.Drawing.Point(12, 10);
            this.lblIndTitulo3.Name = "lblIndTitulo3";
            this.lblIndTitulo3.TabIndex = 0;
            //
            // lblIndValor3
            //
            this.lblIndValor3.AutoSize = true;
            this.lblIndValor3.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblIndValor3.Location = new System.Drawing.Point(9, 28);
            this.lblIndValor3.Name = "lblIndValor3";
            this.lblIndValor3.TabIndex = 1;
            //
            // tblCuerpo
            //
            this.tblCuerpo.ColumnCount = 2;
            this.tblCuerpo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.tblCuerpo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tblCuerpo.Controls.Add(this.panelGrafico, 0, 0);
            this.tblCuerpo.Controls.Add(this.dgvDetalle, 1, 0);
            this.tblCuerpo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblCuerpo.Location = new System.Drawing.Point(0, 126);
            this.tblCuerpo.Name = "tblCuerpo";
            this.tblCuerpo.RowCount = 1;
            this.tblCuerpo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblCuerpo.Size = new System.Drawing.Size(800, 412);
            this.tblCuerpo.TabIndex = 2;
            //
            // panelGrafico
            //
            this.panelGrafico.BackColor = System.Drawing.Color.White;
            this.panelGrafico.Controls.Add(this.lblTituloGrafico);
            this.panelGrafico.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGrafico.Margin = new System.Windows.Forms.Padding(6);
            this.panelGrafico.Name = "panelGrafico";
            this.panelGrafico.Padding = new System.Windows.Forms.Padding(10);
            this.panelGrafico.TabIndex = 0;
            //
            // lblTituloGrafico
            //
            this.lblTituloGrafico.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTituloGrafico.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblTituloGrafico.Name = "lblTituloGrafico";
            this.lblTituloGrafico.Size = new System.Drawing.Size(400, 24);
            this.lblTituloGrafico.TabIndex = 0;
            //
            // dgvDetalle
            //
            this.dgvDetalle.AllowUserToAddRows = false;
            this.dgvDetalle.AllowUserToDeleteRows = false;
            this.dgvDetalle.AllowUserToResizeRows = false;
            this.dgvDetalle.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvDetalle.BackgroundColor = System.Drawing.Color.White;
            this.dgvDetalle.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvDetalle.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDetalle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDetalle.Margin = new System.Windows.Forms.Padding(6);
            this.dgvDetalle.Name = "dgvDetalle";
            this.dgvDetalle.ReadOnly = true;
            this.dgvDetalle.RowHeadersVisible = false;
            this.dgvDetalle.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDetalle.TabIndex = 1;
            //
            // lblAviso
            //
            this.lblAviso.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblAviso.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(125)))), ((int)(((byte)(135)))));
            this.lblAviso.Location = new System.Drawing.Point(0, 538);
            this.lblAviso.Name = "lblAviso";
            this.lblAviso.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.lblAviso.Size = new System.Drawing.Size(800, 22);
            this.lblAviso.TabIndex = 3;
            this.lblAviso.Text = "Datos de ejemplo: la pantalla todavía no está conectada a la base de datos.";
            this.lblAviso.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // ucReportes
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tblCuerpo);
            this.Controls.Add(this.lblAviso);
            this.Controls.Add(this.tblIndicadores);
            this.Controls.Add(this.panelBarra);
            this.Name = "ucReportes";
            this.Size = new System.Drawing.Size(800, 560);
            this.Load += new System.EventHandler(this.ucReportes_Load);
            this.panelBarra.ResumeLayout(false);
            this.panelBarra.PerformLayout();
            this.tblIndicadores.ResumeLayout(false);
            this.panelIndicador1.ResumeLayout(false);
            this.panelIndicador1.PerformLayout();
            this.panelIndicador2.ResumeLayout(false);
            this.panelIndicador2.PerformLayout();
            this.panelIndicador3.ResumeLayout(false);
            this.panelIndicador3.PerformLayout();
            this.tblCuerpo.ResumeLayout(false);
            this.panelGrafico.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetalle)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelBarra;
        private System.Windows.Forms.Button btnVentas;
        private System.Windows.Forms.Button btnProductos;
        private System.Windows.Forms.Button btnRecaudacion;
        private System.Windows.Forms.Label lblPeriodo;
        private System.Windows.Forms.ComboBox cboPeriodo;
        private System.Windows.Forms.TableLayoutPanel tblIndicadores;
        private System.Windows.Forms.Panel panelIndicador1;
        private System.Windows.Forms.Label lblIndTitulo1;
        private System.Windows.Forms.Label lblIndValor1;
        private System.Windows.Forms.Panel panelIndicador2;
        private System.Windows.Forms.Label lblIndTitulo2;
        private System.Windows.Forms.Label lblIndValor2;
        private System.Windows.Forms.Panel panelIndicador3;
        private System.Windows.Forms.Label lblIndTitulo3;
        private System.Windows.Forms.Label lblIndValor3;
        private System.Windows.Forms.TableLayoutPanel tblCuerpo;
        private System.Windows.Forms.Panel panelGrafico;
        private System.Windows.Forms.Label lblTituloGrafico;
        private System.Windows.Forms.DataGridView dgvDetalle;
        private System.Windows.Forms.Label lblAviso;
    }
}
