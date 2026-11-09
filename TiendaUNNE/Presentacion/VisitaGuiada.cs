using System;
using System.Drawing;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// El motor de la ayuda: recorre los pasos de un capítulo sobre una ventana. En cada paso le
    /// pide a la pantalla el control del ancla, lo rodea con un recuadro (MarcoGuia) y pone el
    /// globo (GloboGuia) al costado; si el paso no señala nada, o esa parte no está a la vista,
    /// el globo va centrado. El contenido de los pasos viene armado desde NegocioGuia.
    /// </summary>
    internal sealed class VisitaGuiada
    {
        private const int Distancia = 14;   // entre el control señalado y el globo

        private static readonly Color ColorRecuadro = Color.FromArgb(59, 130, 246);

        private readonly Form _ventana;
        private readonly IPantallaConGuia _pantalla;
        private readonly CapituloGuia _capitulo;

        private MarcoGuia _marco;
        private GloboGuia _globo;
        private int _indice;

        /// <summary>Se dispara al terminar o cerrar la guía.</summary>
        public event EventHandler Terminada;

        public VisitaGuiada(Form ventana, IPantallaConGuia pantalla, CapituloGuia capitulo)
        {
            if (ventana == null) throw new ArgumentNullException(nameof(ventana));
            if (pantalla == null) throw new ArgumentNullException(nameof(pantalla));
            if (capitulo == null) throw new ArgumentNullException(nameof(capitulo));

            _ventana = ventana;
            _pantalla = pantalla;
            _capitulo = capitulo;
        }

        public bool Activa => _globo != null;

        public void Iniciar()
        {
            if (_capitulo.Pasos.Count == 0 || Activa) return;

            _marco = new MarcoGuia(ColorRecuadro);
            _globo = new GloboGuia();
            _globo.Siguiente += (s, e) => Avanzar();
            _globo.Anterior += (s, e) => { if (_indice > 0) MostrarPaso(_indice - 1); };
            _globo.Cerrar += (s, e) => Cerrar();

            _ventana.LocationChanged += VentanaCambio;
            _ventana.SizeChanged += VentanaCambio;
            _ventana.FormClosed += VentanaCerrada;

            MostrarPaso(0);
        }

        public void Cerrar()
        {
            if (!Activa) return;

            _ventana.LocationChanged -= VentanaCambio;
            _ventana.SizeChanged -= VentanaCambio;
            _ventana.FormClosed -= VentanaCerrada;

            _marco.Close();
            _marco.Dispose();
            _globo.Close();
            _globo.Dispose();
            _marco = null;
            _globo = null;

            if (!_ventana.IsDisposed) _ventana.Activate();
            Terminada?.Invoke(this, EventArgs.Empty);
        }

        private void Avanzar()
        {
            if (_indice >= _capitulo.Pasos.Count - 1)
                Cerrar();
            else
                MostrarPaso(_indice + 1);
        }

        private void MostrarPaso(int indice)
        {
            _indice = indice;
            PasoGuia paso = _capitulo.Pasos[indice];

            string encabezado = string.Format("{0} / {1} · {2}",
                indice + 1, _capitulo.Pasos.Count, _capitulo.Nombre.ToUpperInvariant());
            _globo.MostrarPaso(encabezado, paso.Titulo, paso.Texto,
                hayAnterior: indice > 0, esUltimo: indice == _capitulo.Pasos.Count - 1);

            Rectangle area = _ventana.RectangleToScreen(_ventana.ClientRectangle);
            Rectangle? objetivo = RectanguloDe(paso.Ancla);

            if (objetivo.HasValue)
            {
                _marco.Rodear(objetivo.Value, area);
                if (!_marco.Visible) _marco.Show(_ventana);
            }
            else if (_marco.Visible)
            {
                _marco.Hide();
            }

            _globo.Location = UbicarGlobo(_globo.Size, objetivo, area);
            if (!_globo.Visible) _globo.Show(_ventana);
            _globo.Activate();
        }

        /// <summary>El rectángulo en pantalla del control del ancla, o null si no hay o no se ve.</summary>
        private Rectangle? RectanguloDe(string ancla)
        {
            if (string.IsNullOrEmpty(ancla)) return null;

            Control control = _pantalla.Ancla(ancla);
            if (control == null || control.IsDisposed || !control.Visible || control.Width == 0 || control.Height == 0)
                return null;

            return control.RectangleToScreen(control.ClientRectangle);
        }

        /// <summary>
        /// A la derecha del control si entra; si no, a la izquierda, abajo o arriba. Si el control es
        /// tan grande que no queda lugar afuera (por ejemplo, todo el contenido), va dentro de él.
        /// Sin control, centrado en la ventana. Nunca se sale de la ventana.
        /// </summary>
        private static Point UbicarGlobo(Size globo, Rectangle? objetivo, Rectangle area)
        {
            Point p;
            if (!objetivo.HasValue)
            {
                p = new Point(area.Left + (area.Width - globo.Width) / 2, area.Top + (area.Height - globo.Height) / 2);
            }
            else
            {
                Rectangle o = objetivo.Value;
                if (o.Right + Distancia + globo.Width <= area.Right)
                    p = new Point(o.Right + Distancia, o.Top);
                else if (o.Left - Distancia - globo.Width >= area.Left)
                    p = new Point(o.Left - Distancia - globo.Width, o.Top);
                else if (o.Bottom + Distancia + globo.Height <= area.Bottom)
                    p = new Point(o.Left, o.Bottom + Distancia);
                else if (o.Top - Distancia - globo.Height >= area.Top)
                    p = new Point(o.Left, o.Top - Distancia - globo.Height);
                else
                    p = new Point(o.Left + (o.Width - globo.Width) / 2, o.Top + (o.Height - globo.Height) / 2);
            }

            int x = Math.Max(area.Left, Math.Min(p.X, area.Right - globo.Width));
            int y = Math.Max(area.Top, Math.Min(p.Y, area.Bottom - globo.Height));
            return new Point(x, y);
        }

        private void VentanaCambio(object sender, EventArgs e)
        {
            if (!Activa) return;

            // Minimizada no hay dónde dibujar; al volver, se reacomoda en el paso donde estaba.
            if (_ventana.WindowState == FormWindowState.Minimized) return;
            MostrarPaso(_indice);
        }

        private void VentanaCerrada(object sender, FormClosedEventArgs e)
        {
            Cerrar();
        }
    }
}
