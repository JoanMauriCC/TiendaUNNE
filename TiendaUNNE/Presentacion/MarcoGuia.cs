using System.Drawing;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// El recuadro de color que rodea la parte de la pantalla que explica la guía. Es una ventana
    /// sin bordes recortada en forma de marco: el centro es un hueco, así que no tapa lo que
    /// rodea ni le quita los clics, y nunca toma el foco.
    /// </summary>
    internal sealed class MarcoGuia : Form
    {
        private const int Grosor = 3;
        private const int Separacion = 4;   // aire entre el control y el recuadro

        public MarcoGuia(Color color)
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            BackColor = color;
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_EX_TOOLWINDOW = 0x80;
                const int WS_EX_NOACTIVATE = 0x08000000;
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
                return cp;
            }
        }

        /// <summary>Rodea el rectángulo (en coordenadas de pantalla) sin salirse de la ventana dueña.</summary>
        public void Rodear(Rectangle objetivo, Rectangle limite)
        {
            Rectangle exterior = Rectangle.Inflate(objetivo, Separacion + Grosor, Separacion + Grosor);
            exterior.Intersect(limite);
            if (exterior.Width <= 2 * Grosor || exterior.Height <= 2 * Grosor) return;

            Bounds = exterior;

            var marco = new Region(new Rectangle(0, 0, exterior.Width, exterior.Height));
            marco.Exclude(new Rectangle(Grosor, Grosor, exterior.Width - 2 * Grosor, exterior.Height - 2 * Grosor));

            Region anterior = Region;
            Region = marco;
            if (anterior != null) anterior.Dispose();
        }
    }
}
