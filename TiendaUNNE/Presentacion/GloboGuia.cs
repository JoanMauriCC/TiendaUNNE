using System;
using System.Drawing;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// El globo de la guía: «2 / 6 · BIENVENIDA», un título, el texto del paso y los botones
    /// Saltar, Anterior y Siguiente (Terminar en el último). Solo muestra y avisa qué se tocó:
    /// qué paso sigue lo decide VisitaGuiada. Teclas: Enter o → avanza, ← vuelve, Esc cierra.
    /// </summary>
    internal sealed class GloboGuia : Form
    {
        private const int Ancho = 380;
        private const int Margen = 18;

        private static readonly Color ColorTexto = Color.FromArgb(45, 48, 54);
        private static readonly Color ColorGris = Color.FromArgb(120, 125, 135);
        private static readonly Color ColorBorde = Color.FromArgb(214, 216, 220);
        private static readonly Color ColorBotonPrincipal = Color.FromArgb(24, 24, 27);

        private readonly Label _lblPaso = new Label();
        private readonly Label _lblTitulo = new Label();
        private readonly Label _lblTexto = new Label();
        private readonly Button _btnCerrar = new Button();
        private readonly Button _btnSaltar = new Button();
        private readonly Button _btnAnterior = new Button();
        private readonly Button _btnSiguiente = new Button();

        public event EventHandler Anterior;
        public event EventHandler Siguiente;
        public event EventHandler Cerrar;

        public GloboGuia()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9.75f);
            KeyPreview = true;

            _lblPaso.AutoSize = true;
            _lblPaso.Font = new Font("Segoe UI", 8.25f);
            _lblPaso.ForeColor = ColorGris;

            _lblTitulo.AutoSize = true;
            _lblTitulo.Font = new Font("Segoe UI", 11.25f, FontStyle.Bold);
            _lblTitulo.ForeColor = ColorTexto;

            _lblTexto.AutoSize = true;
            _lblTexto.MaximumSize = new Size(Ancho - 2 * Margen, 0);
            _lblTexto.ForeColor = ColorTexto;

            EstiloBoton(_btnCerrar, "✕", Color.White, ColorGris, sinBorde: true);
            _btnCerrar.Size = new Size(28, 28);
            _btnCerrar.Font = new Font("Segoe UI", 10f);

            EstiloBoton(_btnSaltar, "Saltar guía", Color.White, ColorTexto, sinBorde: true);
            EstiloBoton(_btnAnterior, "Anterior", Color.White, ColorTexto, sinBorde: false);
            EstiloBoton(_btnSiguiente, "Siguiente", ColorBotonPrincipal, Color.White, sinBorde: true);

            _btnCerrar.Click += (s, e) => Cerrar?.Invoke(this, EventArgs.Empty);
            _btnSaltar.Click += (s, e) => Cerrar?.Invoke(this, EventArgs.Empty);
            _btnAnterior.Click += (s, e) => Anterior?.Invoke(this, EventArgs.Empty);
            _btnSiguiente.Click += (s, e) => Siguiente?.Invoke(this, EventArgs.Empty);

            Controls.AddRange(new Control[]
            {
                _lblPaso, _lblTitulo, _lblTexto, _btnCerrar, _btnSaltar, _btnAnterior, _btnSiguiente
            });

            AcceptButton = _btnSiguiente;
            CancelButton = _btnCerrar;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_EX_TOOLWINDOW = 0x80;
                const int CS_DROPSHADOW = 0x20000;
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TOOLWINDOW;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        private static void EstiloBoton(Button boton, string texto, Color fondo, Color letra, bool sinBorde)
        {
            boton.Text = texto;
            boton.AutoSize = true;
            boton.Padding = new Padding(8, 2, 8, 2);
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = sinBorde ? 0 : 1;
            boton.FlatAppearance.BorderColor = ColorBorde;
            boton.BackColor = fondo;
            boton.ForeColor = letra;
            boton.Cursor = Cursors.Hand;
            boton.UseVisualStyleBackColor = false;
        }

        /// <summary>Carga un paso y acomoda el globo a su texto.</summary>
        public void MostrarPaso(string encabezado, string titulo, string texto, bool hayAnterior, bool esUltimo)
        {
            _lblPaso.Text = encabezado;
            _lblTitulo.Text = titulo;
            _lblTexto.Text = texto;
            _btnAnterior.Enabled = hayAnterior;
            _btnSiguiente.Text = esUltimo ? "Terminar" : "Siguiente";
            Acomodar();
        }

        private void Acomodar()
        {
            _lblPaso.Location = new Point(Margen, Margen);
            _btnCerrar.Location = new Point(Ancho - Margen - _btnCerrar.Width + 8, Margen - 8);
            _lblTitulo.Location = new Point(Margen, _lblPaso.Bottom + 4);
            _lblTexto.Location = new Point(Margen, _lblTitulo.Bottom + 8);

            int filaBotones = _lblTexto.Bottom + 16;
            _btnSaltar.Location = new Point(Margen - 8, filaBotones);
            _btnSiguiente.Location = new Point(Ancho - Margen - _btnSiguiente.Width, filaBotones);
            _btnAnterior.Location = new Point(_btnSiguiente.Left - 8 - _btnAnterior.Width, filaBotones);

            ClientSize = new Size(Ancho, _btnSiguiente.Bottom + Margen);
            Invalidate();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Right) { Siguiente?.Invoke(this, EventArgs.Empty); return true; }
            if (keyData == Keys.Left && _btnAnterior.Enabled) { Anterior?.Invoke(this, EventArgs.Empty); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var borde = new Pen(ColorBorde))
                e.Graphics.DrawRectangle(borde, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
        }
    }
}
