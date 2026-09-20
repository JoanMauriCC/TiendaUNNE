namespace TiendaUNNE
{
    partial class frmPrincipal
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
            this.panelLateral = new System.Windows.Forms.Panel();
            this.panelNav = new System.Windows.Forms.FlowLayoutPanel();
            this.panelPie = new System.Windows.Forms.Panel();
            this.lblUsuario = new System.Windows.Forms.Label();
            this.btnCerrarSesion = new System.Windows.Forms.Button();
            this.panelMarca = new System.Windows.Forms.Panel();
            this.lblMarca = new System.Windows.Forms.Label();
            this.separadorLateral = new System.Windows.Forms.Panel();
            this.panelContenido = new System.Windows.Forms.Panel();
            this.panelHost = new System.Windows.Forms.Panel();
            this.panelTitulo = new System.Windows.Forms.Panel();
            this.lblTituloSeccion = new System.Windows.Forms.Label();
            this.separadorTitulo = new System.Windows.Forms.Panel();
            this.panelLateral.SuspendLayout();
            this.panelPie.SuspendLayout();
            this.panelMarca.SuspendLayout();
            this.panelContenido.SuspendLayout();
            this.panelTitulo.SuspendLayout();
            this.SuspendLayout();
            //
            // panelLateral
            //
            this.panelLateral.BackColor = System.Drawing.Color.White;
            this.panelLateral.Controls.Add(this.panelNav);
            this.panelLateral.Controls.Add(this.panelPie);
            this.panelLateral.Controls.Add(this.panelMarca);
            this.panelLateral.Controls.Add(this.separadorLateral);
            this.panelLateral.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLateral.Location = new System.Drawing.Point(0, 0);
            this.panelLateral.Name = "panelLateral";
            this.panelLateral.Size = new System.Drawing.Size(196, 700);
            this.panelLateral.TabIndex = 0;
            //
            // panelNav
            //
            this.panelNav.AutoScroll = true;
            this.panelNav.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelNav.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.panelNav.Location = new System.Drawing.Point(0, 64);
            this.panelNav.Name = "panelNav";
            this.panelNav.Padding = new System.Windows.Forms.Padding(10, 6, 10, 6);
            this.panelNav.Size = new System.Drawing.Size(195, 552);
            this.panelNav.TabIndex = 1;
            this.panelNav.WrapContents = false;
            //
            // panelPie
            //
            this.panelPie.Controls.Add(this.lblUsuario);
            this.panelPie.Controls.Add(this.btnCerrarSesion);
            this.panelPie.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelPie.Location = new System.Drawing.Point(0, 616);
            this.panelPie.Name = "panelPie";
            this.panelPie.Size = new System.Drawing.Size(195, 84);
            this.panelPie.TabIndex = 2;
            //
            // lblUsuario
            //
            this.lblUsuario.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblUsuario.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(125)))), ((int)(((byte)(135)))));
            this.lblUsuario.Location = new System.Drawing.Point(14, 10);
            this.lblUsuario.Name = "lblUsuario";
            this.lblUsuario.Size = new System.Drawing.Size(168, 32);
            this.lblUsuario.TabIndex = 0;
            this.lblUsuario.Text = "Sin sesión";
            //
            // btnCerrarSesion
            //
            this.btnCerrarSesion.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(216)))), ((int)(((byte)(220)))));
            this.btnCerrarSesion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCerrarSesion.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnCerrarSesion.Location = new System.Drawing.Point(14, 46);
            this.btnCerrarSesion.Name = "btnCerrarSesion";
            this.btnCerrarSesion.Size = new System.Drawing.Size(168, 28);
            this.btnCerrarSesion.TabIndex = 1;
            this.btnCerrarSesion.Text = "Cerrar sesión";
            this.btnCerrarSesion.UseVisualStyleBackColor = true;
            this.btnCerrarSesion.Click += new System.EventHandler(this.btnCerrarSesion_Click);
            //
            // panelMarca
            //
            this.panelMarca.Controls.Add(this.lblMarca);
            this.panelMarca.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelMarca.Location = new System.Drawing.Point(0, 0);
            this.panelMarca.Name = "panelMarca";
            this.panelMarca.Size = new System.Drawing.Size(195, 64);
            this.panelMarca.TabIndex = 0;
            //
            // lblMarca
            //
            this.lblMarca.AutoSize = true;
            this.lblMarca.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblMarca.Location = new System.Drawing.Point(15, 20);
            this.lblMarca.Name = "lblMarca";
            this.lblMarca.Size = new System.Drawing.Size(122, 25);
            this.lblMarca.TabIndex = 0;
            this.lblMarca.Text = "TiendaUNNE";
            //
            // separadorLateral
            //
            this.separadorLateral.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(230)))), ((int)(((byte)(234)))));
            this.separadorLateral.Dock = System.Windows.Forms.DockStyle.Right;
            this.separadorLateral.Location = new System.Drawing.Point(195, 0);
            this.separadorLateral.Name = "separadorLateral";
            this.separadorLateral.Size = new System.Drawing.Size(1, 700);
            this.separadorLateral.TabIndex = 3;
            //
            // panelContenido
            //
            this.panelContenido.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.panelContenido.Controls.Add(this.panelHost);
            this.panelContenido.Controls.Add(this.panelTitulo);
            this.panelContenido.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContenido.Location = new System.Drawing.Point(196, 0);
            this.panelContenido.Name = "panelContenido";
            this.panelContenido.Size = new System.Drawing.Size(824, 700);
            this.panelContenido.TabIndex = 1;
            //
            // panelHost
            //
            this.panelHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelHost.Location = new System.Drawing.Point(0, 57);
            this.panelHost.Name = "panelHost";
            this.panelHost.Padding = new System.Windows.Forms.Padding(16, 12, 16, 16);
            this.panelHost.Size = new System.Drawing.Size(824, 643);
            this.panelHost.TabIndex = 1;
            //
            // panelTitulo
            //
            this.panelTitulo.BackColor = System.Drawing.Color.White;
            this.panelTitulo.Controls.Add(this.lblTituloSeccion);
            this.panelTitulo.Controls.Add(this.separadorTitulo);
            this.panelTitulo.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTitulo.Location = new System.Drawing.Point(0, 0);
            this.panelTitulo.Name = "panelTitulo";
            this.panelTitulo.Size = new System.Drawing.Size(824, 57);
            this.panelTitulo.TabIndex = 0;
            //
            // lblTituloSeccion
            //
            this.lblTituloSeccion.AutoSize = true;
            this.lblTituloSeccion.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTituloSeccion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(35)))), ((int)(((byte)(45)))));
            this.lblTituloSeccion.Location = new System.Drawing.Point(20, 15);
            this.lblTituloSeccion.Name = "lblTituloSeccion";
            this.lblTituloSeccion.Size = new System.Drawing.Size(69, 23);
            this.lblTituloSeccion.TabIndex = 0;
            this.lblTituloSeccion.Text = "Inicio";
            //
            // separadorTitulo
            //
            this.separadorTitulo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(230)))), ((int)(((byte)(234)))));
            this.separadorTitulo.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.separadorTitulo.Location = new System.Drawing.Point(0, 56);
            this.separadorTitulo.Name = "separadorTitulo";
            this.separadorTitulo.Size = new System.Drawing.Size(824, 1);
            this.separadorTitulo.TabIndex = 1;
            //
            // frmPrincipal
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1020, 700);
            this.Controls.Add(this.panelContenido);
            this.Controls.Add(this.panelLateral);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimumSize = new System.Drawing.Size(880, 600);
            this.Name = "frmPrincipal";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Text = "TiendaUNNE";
            this.Load += new System.EventHandler(this.frmPrincipal_Load);
            this.panelLateral.ResumeLayout(false);
            this.panelPie.ResumeLayout(false);
            this.panelMarca.ResumeLayout(false);
            this.panelMarca.PerformLayout();
            this.panelContenido.ResumeLayout(false);
            this.panelTitulo.ResumeLayout(false);
            this.panelTitulo.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelLateral;
        private System.Windows.Forms.Panel panelMarca;
        private System.Windows.Forms.Label lblMarca;
        private System.Windows.Forms.FlowLayoutPanel panelNav;
        private System.Windows.Forms.Panel panelPie;
        private System.Windows.Forms.Label lblUsuario;
        private System.Windows.Forms.Button btnCerrarSesion;
        private System.Windows.Forms.Panel separadorLateral;
        private System.Windows.Forms.Panel panelContenido;
        private System.Windows.Forms.Panel panelTitulo;
        private System.Windows.Forms.Label lblTituloSeccion;
        private System.Windows.Forms.Panel separadorTitulo;
        private System.Windows.Forms.Panel panelHost;
    }
}
