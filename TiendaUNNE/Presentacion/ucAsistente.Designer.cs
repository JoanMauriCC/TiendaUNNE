namespace TiendaUNNE
{
    partial class ucAsistente
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
            this.panelEncabezado = new System.Windows.Forms.Panel();
            this.lblSubtitulo = new System.Windows.Forms.Label();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.btnNueva = new System.Windows.Forms.Button();
            this.flowSugerencias = new System.Windows.Forms.FlowLayoutPanel();
            this.flowConversacion = new System.Windows.Forms.FlowLayoutPanel();
            this.panelEntrada = new System.Windows.Forms.Panel();
            this.lblAyudaEntrada = new System.Windows.Forms.Label();
            this.btnEnviar = new System.Windows.Forms.Button();
            this.txtPregunta = new System.Windows.Forms.TextBox();
            this.lblPregunta = new System.Windows.Forms.Label();
            this.panelEncabezado.SuspendLayout();
            this.panelEntrada.SuspendLayout();
            this.SuspendLayout();
            //
            // panelEncabezado
            //
            this.panelEncabezado.Controls.Add(this.lblSubtitulo);
            this.panelEncabezado.Controls.Add(this.lblTitulo);
            this.panelEncabezado.Controls.Add(this.btnNueva);
            this.panelEncabezado.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelEncabezado.Location = new System.Drawing.Point(0, 0);
            this.panelEncabezado.Name = "panelEncabezado";
            this.panelEncabezado.Size = new System.Drawing.Size(800, 62);
            this.panelEncabezado.TabIndex = 0;
            //
            // lblTitulo
            //
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(48)))), ((int)(((byte)(54)))));
            this.lblTitulo.Location = new System.Drawing.Point(12, 6);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Asistente de ventas";
            this.lblTitulo.UseMnemonic = false;
            //
            // lblSubtitulo
            //
            this.lblSubtitulo.AutoSize = true;
            this.lblSubtitulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(125)))), ((int)(((byte)(135)))));
            this.lblSubtitulo.Location = new System.Drawing.Point(14, 38);
            this.lblSubtitulo.Name = "lblSubtitulo";
            this.lblSubtitulo.TabIndex = 1;
            this.lblSubtitulo.Text = "Preguntale en lenguaje natural sobre ventas, promociones o productos. Funciona con Google Gemini.";
            this.lblSubtitulo.UseMnemonic = false;
            //
            // btnNueva
            //
            this.btnNueva.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNueva.BackColor = System.Drawing.Color.White;
            this.btnNueva.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(216)))), ((int)(((byte)(220)))));
            this.btnNueva.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNueva.Location = new System.Drawing.Point(632, 14);
            this.btnNueva.Name = "btnNueva";
            this.btnNueva.Size = new System.Drawing.Size(156, 32);
            this.btnNueva.TabIndex = 2;
            this.btnNueva.Text = "&Nueva conversación";
            this.btnNueva.UseVisualStyleBackColor = false;
            this.btnNueva.Click += new System.EventHandler(this.btnNueva_Click);
            //
            // flowSugerencias
            //
            this.flowSugerencias.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowSugerencias.Name = "flowSugerencias";
            this.flowSugerencias.Padding = new System.Windows.Forms.Padding(9, 8, 9, 0);
            this.flowSugerencias.AutoSize = true;
            this.flowSugerencias.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowSugerencias.Size = new System.Drawing.Size(800, 44);
            this.flowSugerencias.TabIndex = 3;
            this.flowSugerencias.WrapContents = true;
            //
            // flowConversacion
            //
            this.flowConversacion.AutoScroll = true;
            this.flowConversacion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.flowConversacion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowConversacion.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowConversacion.Location = new System.Drawing.Point(0, 62);
            this.flowConversacion.Name = "flowConversacion";
            this.flowConversacion.Padding = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this.flowConversacion.Size = new System.Drawing.Size(800, 406);
            this.flowConversacion.TabIndex = 1;
            this.flowConversacion.WrapContents = false;
            this.flowConversacion.Resize += new System.EventHandler(this.flowConversacion_Resize);
            //
            // panelEntrada
            //
            this.panelEntrada.Controls.Add(this.lblPregunta);
            this.panelEntrada.Controls.Add(this.txtPregunta);
            this.panelEntrada.Controls.Add(this.btnEnviar);
            this.panelEntrada.Controls.Add(this.lblAyudaEntrada);
            this.panelEntrada.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelEntrada.Location = new System.Drawing.Point(0, 468);
            this.panelEntrada.Name = "panelEntrada";
            this.panelEntrada.Size = new System.Drawing.Size(800, 112);
            this.panelEntrada.TabIndex = 2;
            //
            // lblPregunta
            //
            this.lblPregunta.AutoSize = true;
            this.lblPregunta.Location = new System.Drawing.Point(12, 8);
            this.lblPregunta.Name = "lblPregunta";
            this.lblPregunta.TabIndex = 0;
            this.lblPregunta.Text = "Tu &pregunta:";
            //
            // txtPregunta
            //
            this.txtPregunta.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPregunta.Location = new System.Drawing.Point(12, 28);
            this.txtPregunta.MaxLength = 1000;
            this.txtPregunta.Multiline = true;
            this.txtPregunta.Name = "txtPregunta";
            this.txtPregunta.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtPregunta.Size = new System.Drawing.Size(660, 56);
            this.txtPregunta.TabIndex = 1;
            this.txtPregunta.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtPregunta_KeyDown);
            //
            // btnEnviar
            //
            this.btnEnviar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEnviar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.btnEnviar.FlatAppearance.BorderSize = 0;
            this.btnEnviar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEnviar.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnEnviar.ForeColor = System.Drawing.Color.White;
            this.btnEnviar.Location = new System.Drawing.Point(684, 28);
            this.btnEnviar.Name = "btnEnviar";
            this.btnEnviar.Size = new System.Drawing.Size(104, 56);
            this.btnEnviar.TabIndex = 2;
            this.btnEnviar.Text = "&Enviar";
            this.btnEnviar.UseVisualStyleBackColor = false;
            this.btnEnviar.Click += new System.EventHandler(this.btnEnviar_Click);
            //
            // lblAyudaEntrada
            //
            this.lblAyudaEntrada.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblAyudaEntrada.AutoSize = true;
            this.lblAyudaEntrada.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(125)))), ((int)(((byte)(135)))));
            this.lblAyudaEntrada.Location = new System.Drawing.Point(12, 90);
            this.lblAyudaEntrada.Name = "lblAyudaEntrada";
            this.lblAyudaEntrada.TabIndex = 3;
            this.lblAyudaEntrada.Text = "Enter envía · Shift+Enter agrega una línea · Clic derecho sobre una respuesta para copiarla";
            this.lblAyudaEntrada.UseMnemonic = false;
            //
            // ucAsistente
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.flowConversacion);
            this.Controls.Add(this.flowSugerencias);
            this.Controls.Add(this.panelEntrada);
            this.Controls.Add(this.panelEncabezado);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "ucAsistente";
            this.Size = new System.Drawing.Size(800, 580);
            this.Load += new System.EventHandler(this.ucAsistente_Load);
            this.panelEncabezado.ResumeLayout(false);
            this.panelEncabezado.PerformLayout();
            this.panelEntrada.ResumeLayout(false);
            this.panelEntrada.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelEncabezado;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblSubtitulo;
        private System.Windows.Forms.Button btnNueva;
        private System.Windows.Forms.FlowLayoutPanel flowSugerencias;
        private System.Windows.Forms.FlowLayoutPanel flowConversacion;
        private System.Windows.Forms.Panel panelEntrada;
        private System.Windows.Forms.Label lblPregunta;
        private System.Windows.Forms.TextBox txtPregunta;
        private System.Windows.Forms.Button btnEnviar;
        private System.Windows.Forms.Label lblAyudaEntrada;
    }
}
