namespace TiendaUNNE
{
    partial class ucBackup
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
            this.panelResumen = new System.Windows.Forms.Panel();
            this.lblCarpetaTitulo = new System.Windows.Forms.Label();
            this.lblCarpeta = new System.Windows.Forms.Label();
            this.lblCantidadTitulo = new System.Windows.Forms.Label();
            this.lblCantidad = new System.Windows.Forms.Label();
            this.lblUltimaTitulo = new System.Windows.Forms.Label();
            this.lblUltima = new System.Windows.Forms.Label();
            this.panelRiel = new System.Windows.Forms.Panel();
            this.tarjetaCrear = new TiendaUNNE.TarjetaAccion();
            this.tarjetaRestaurar = new TiendaUNNE.TarjetaAccion();
            this.tarjetaCopiar = new TiendaUNNE.TarjetaAccion();
            this.tarjetaActualizar = new TiendaUNNE.TarjetaAccion();
            this.lblAviso = new System.Windows.Forms.Label();
            this.dgvBackups = new System.Windows.Forms.DataGridView();
            this.panelResumen.SuspendLayout();
            this.panelRiel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBackups)).BeginInit();
            this.SuspendLayout();
            //
            // panelResumen
            //
            this.panelResumen.BackColor = System.Drawing.Color.White;
            this.panelResumen.Controls.Add(this.lblCarpetaTitulo);
            this.panelResumen.Controls.Add(this.lblCarpeta);
            this.panelResumen.Controls.Add(this.lblCantidadTitulo);
            this.panelResumen.Controls.Add(this.lblCantidad);
            this.panelResumen.Controls.Add(this.lblUltimaTitulo);
            this.panelResumen.Controls.Add(this.lblUltima);
            this.panelResumen.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelResumen.Location = new System.Drawing.Point(0, 0);
            this.panelResumen.Name = "panelResumen";
            this.panelResumen.Size = new System.Drawing.Size(844, 66);
            this.panelResumen.TabIndex = 0;
            //
            // lblCarpetaTitulo
            //
            this.lblCarpetaTitulo.AutoSize = true;
            this.lblCarpetaTitulo.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblCarpetaTitulo.Location = new System.Drawing.Point(12, 10);
            this.lblCarpetaTitulo.Name = "lblCarpetaTitulo";
            this.lblCarpetaTitulo.TabIndex = 0;
            this.lblCarpetaTitulo.Text = "Carpeta de copias";
            //
            // lblCarpeta
            //
            this.lblCarpeta.AutoEllipsis = true;
            this.lblCarpeta.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblCarpeta.Location = new System.Drawing.Point(12, 32);
            this.lblCarpeta.Name = "lblCarpeta";
            this.lblCarpeta.Size = new System.Drawing.Size(260, 22);
            this.lblCarpeta.TabIndex = 1;
            this.lblCarpeta.UseMnemonic = false;
            //
            // lblCantidadTitulo
            //
            this.lblCantidadTitulo.AutoSize = true;
            this.lblCantidadTitulo.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblCantidadTitulo.Location = new System.Drawing.Point(292, 10);
            this.lblCantidadTitulo.Name = "lblCantidadTitulo";
            this.lblCantidadTitulo.TabIndex = 2;
            this.lblCantidadTitulo.Text = "Copias guardadas";
            //
            // lblCantidad
            //
            this.lblCantidad.AutoEllipsis = true;
            this.lblCantidad.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblCantidad.Location = new System.Drawing.Point(292, 32);
            this.lblCantidad.Name = "lblCantidad";
            this.lblCantidad.Size = new System.Drawing.Size(260, 22);
            this.lblCantidad.TabIndex = 3;
            this.lblCantidad.UseMnemonic = false;
            //
            // lblUltimaTitulo
            //
            this.lblUltimaTitulo.AutoSize = true;
            this.lblUltimaTitulo.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblUltimaTitulo.Location = new System.Drawing.Point(572, 10);
            this.lblUltimaTitulo.Name = "lblUltimaTitulo";
            this.lblUltimaTitulo.TabIndex = 4;
            this.lblUltimaTitulo.Text = "Última copia";
            //
            // lblUltima
            //
            this.lblUltima.AutoEllipsis = true;
            this.lblUltima.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblUltima.Location = new System.Drawing.Point(572, 32);
            this.lblUltima.Name = "lblUltima";
            this.lblUltima.Size = new System.Drawing.Size(260, 22);
            this.lblUltima.TabIndex = 5;
            this.lblUltima.UseMnemonic = false;
            //
            // panelRiel
            //
            this.panelRiel.Controls.Add(this.tarjetaCrear);
            this.panelRiel.Controls.Add(this.tarjetaRestaurar);
            this.panelRiel.Controls.Add(this.tarjetaCopiar);
            this.panelRiel.Controls.Add(this.tarjetaActualizar);
            this.panelRiel.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelRiel.Location = new System.Drawing.Point(552, 66);
            this.panelRiel.Name = "panelRiel";
            this.panelRiel.Size = new System.Drawing.Size(292, 383);
            this.panelRiel.TabIndex = 1;
            //
            // tarjetaCrear
            //
            this.tarjetaCrear.Icono = TiendaUNNE.IconoAccion.BackupCrear;
            this.tarjetaCrear.Titulo = "Crear copia ahora";
            this.tarjetaCrear.Descripcion = "Guarda el estado actual de la base";
            this.tarjetaCrear.Location = new System.Drawing.Point(12, 12);
            this.tarjetaCrear.Name = "tarjetaCrear";
            this.tarjetaCrear.Size = new System.Drawing.Size(268, 44);
            this.tarjetaCrear.TabIndex = 0;
            this.tarjetaCrear.Click += new System.EventHandler(this.tarjetaCrear_Click);
            //
            // tarjetaRestaurar
            //
            this.tarjetaRestaurar.Icono = TiendaUNNE.IconoAccion.BackupRestaurar;
            this.tarjetaRestaurar.Titulo = "Restaurar esta copia";
            this.tarjetaRestaurar.Descripcion = "Elegí una copia de la lista";
            this.tarjetaRestaurar.Enabled = false;
            this.tarjetaRestaurar.Location = new System.Drawing.Point(12, 64);
            this.tarjetaRestaurar.Name = "tarjetaRestaurar";
            this.tarjetaRestaurar.Size = new System.Drawing.Size(268, 44);
            this.tarjetaRestaurar.TabIndex = 1;
            this.tarjetaRestaurar.Click += new System.EventHandler(this.tarjetaRestaurar_Click);
            //
            // tarjetaCopiar
            //
            this.tarjetaCopiar.Icono = TiendaUNNE.IconoAccion.BackupCopiar;
            this.tarjetaCopiar.Titulo = "Guardar copia en…";
            this.tarjetaCopiar.Descripcion = "Para llevarla a un pendrive";
            this.tarjetaCopiar.Enabled = false;
            this.tarjetaCopiar.Location = new System.Drawing.Point(12, 116);
            this.tarjetaCopiar.Name = "tarjetaCopiar";
            this.tarjetaCopiar.Size = new System.Drawing.Size(268, 44);
            this.tarjetaCopiar.TabIndex = 2;
            this.tarjetaCopiar.Click += new System.EventHandler(this.tarjetaCopiar_Click);
            //
            // tarjetaActualizar
            //
            this.tarjetaActualizar.Icono = TiendaUNNE.IconoAccion.Actualizar;
            this.tarjetaActualizar.Titulo = "Actualizar listado";
            this.tarjetaActualizar.Descripcion = "Vuelve a leer la carpeta";
            this.tarjetaActualizar.Location = new System.Drawing.Point(12, 168);
            this.tarjetaActualizar.Name = "tarjetaActualizar";
            this.tarjetaActualizar.Size = new System.Drawing.Size(268, 44);
            this.tarjetaActualizar.TabIndex = 3;
            this.tarjetaActualizar.Click += new System.EventHandler(this.tarjetaActualizar_Click);
            //
            // lblAviso
            //
            this.lblAviso.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblAviso.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblAviso.Location = new System.Drawing.Point(0, 449);
            this.lblAviso.Name = "lblAviso";
            this.lblAviso.Padding = new System.Windows.Forms.Padding(12, 6, 12, 6);
            this.lblAviso.Size = new System.Drawing.Size(844, 52);
            this.lblAviso.TabIndex = 2;
            this.lblAviso.Text = "Las copias contienen todos los datos del sistema, incluidas las contraseñas cifradas. " +
    "Guardalas solo en la carpeta indicada y no las envíes por mail. Solo se pueden restaurar copias creadas desde acá.";
            this.lblAviso.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblAviso.UseMnemonic = false;
            //
            // dgvBackups
            //
            this.dgvBackups.AllowUserToAddRows = false;
            this.dgvBackups.AllowUserToDeleteRows = false;
            this.dgvBackups.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvBackups.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvBackups.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvBackups.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgvBackups.Location = new System.Drawing.Point(0, 66);
            this.dgvBackups.MultiSelect = false;
            this.dgvBackups.Name = "dgvBackups";
            this.dgvBackups.ReadOnly = true;
            this.dgvBackups.RowHeadersVisible = false;
            this.dgvBackups.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvBackups.Size = new System.Drawing.Size(552, 383);
            this.dgvBackups.TabIndex = 0;
            this.dgvBackups.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvBackups_CellFormatting);
            this.dgvBackups.SelectionChanged += new System.EventHandler(this.dgvBackups_SelectionChanged);
            //
            // ucBackup
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dgvBackups);
            this.Controls.Add(this.panelRiel);
            this.Controls.Add(this.lblAviso);
            this.Controls.Add(this.panelResumen);
            this.Name = "ucBackup";
            this.Size = new System.Drawing.Size(844, 501);
            this.Load += new System.EventHandler(this.ucBackup_Load);
            this.panelResumen.ResumeLayout(false);
            this.panelResumen.PerformLayout();
            this.panelRiel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvBackups)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelResumen;
        private System.Windows.Forms.Label lblCarpetaTitulo;
        private System.Windows.Forms.Label lblCarpeta;
        private System.Windows.Forms.Label lblCantidadTitulo;
        private System.Windows.Forms.Label lblCantidad;
        private System.Windows.Forms.Label lblUltimaTitulo;
        private System.Windows.Forms.Label lblUltima;
        private System.Windows.Forms.Panel panelRiel;
        private TarjetaAccion tarjetaCrear;
        private TarjetaAccion tarjetaRestaurar;
        private TarjetaAccion tarjetaCopiar;
        private TarjetaAccion tarjetaActualizar;
        private System.Windows.Forms.Label lblAviso;
        private System.Windows.Forms.DataGridView dgvBackups;
    }
}
