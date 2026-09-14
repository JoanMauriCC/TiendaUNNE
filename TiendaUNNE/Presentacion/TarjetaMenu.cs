using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>Ícono simple dibujado con GDI+ para las tarjetas del dashboard.</summary>
    public enum IconoTarjeta
    {
        Usuarios,
        Producto,
        Auditoria,
        Caja,
        Etiqueta,
        Reportes
    }

    /// <summary>
    /// Tarjeta clickeable estilo dashboard: tarjeta blanca con esquinas redondeadas,
    /// franja de color a la izquierda, ícono dentro de una placa de color e
    /// "Ingresar →" al pie. El evento Click se dispara al hacer clic en cualquier
    /// parte de la tarjeta.
    /// </summary>
    public class TarjetaMenu : Panel
    {
        private const int Radio = 14;
        private const int RadioPlaca = 10;

        private bool _habilitada = true;
        private bool _hover;

        public TarjetaMenu()
        {
            DoubleBuffered = true;
            Size = new Size(266, 168);
            Margin = new Padding(14);
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        public IconoTarjeta Icono { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        public bool Habilitada
        {
            get { return _habilitada; }
            set
            {
                _habilitada = value;
                Cursor = value ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        /// <summary>Color de acento (franja izquierda, placa de ícono, link "Ingresar") según el tipo de tarjeta.</summary>
        private Color ColorAcento
        {
            get
            {
                switch (Icono)
                {
                    case IconoTarjeta.Usuarios: return Color.FromArgb(59, 130, 246);   // azul
                    case IconoTarjeta.Producto: return Color.FromArgb(13, 148, 136);   // verde azulado
                    case IconoTarjeta.Auditoria: return Color.FromArgb(139, 92, 246);  // violeta
                    case IconoTarjeta.Caja: return Color.FromArgb(234, 88, 12);        // naranja
                    case IconoTarjeta.Etiqueta: return Color.FromArgb(220, 38, 38);    // rojo
                    case IconoTarjeta.Reportes: return Color.FromArgb(217, 119, 6);    // ámbar
                    default: return Color.FromArgb(74, 130, 200);
                }
            }
        }

        private static Color Tinte(Color acento, float hacaBlanco)
        {
            int r = (int)(acento.R + (255 - acento.R) * hacaBlanco);
            int g = (int)(acento.G + (255 - acento.G) * hacaBlanco);
            int b = (int)(acento.B + (255 - acento.B) * hacaBlanco);
            return Color.FromArgb(r, g, b);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = _habilitada;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        private static GraphicsPath RectRedondeado(Rectangle r, int radio)
        {
            int d = radio * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Color acento = _habilitada ? ColorAcento : Color.FromArgb(170, 174, 180);
            Color tinta = _habilitada ? Color.FromArgb(31, 35, 45) : Color.FromArgb(150, 150, 150);
            Color gris = Color.FromArgb(120, 125, 135);

            var marco = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var fondoCard = RectRedondeado(marco, Radio))
            {
                using (var b = new SolidBrush(Color.White))
                    g.FillPath(b, fondoCard);

                // Franja de color a la izquierda (recortada a la forma redondeada de la tarjeta).
                Region clipOriginal = g.Clip;
                g.SetClip(fondoCard, CombineMode.Replace);
                using (var b = new SolidBrush(acento))
                    g.FillRectangle(b, 0, 0, 5, Height);
                g.Clip = clipOriginal;

                using (var p = new Pen(_hover && _habilitada ? acento : Color.FromArgb(228, 230, 234)))
                    g.DrawPath(p, fondoCard);
            }

            // Placa del ícono.
            var placa = new Rectangle(18, 18, 40, 40);
            using (var pathPlaca = RectRedondeado(placa, RadioPlaca))
            using (var b = new SolidBrush(Tinte(acento, _habilitada ? 0.85f : 0.9f)))
                g.FillPath(b, pathPlaca);

            // El ícono se dibuja centrado en el origen y escalado hacia abajo para que
            // entre cómodo dentro de la placa de 40x40 (los trazos GDI+ a mano fueron
            // pensados sueltos sobre la tarjeta, no encerrados en un cuadrado chico).
            GraphicsState estado = g.Save();
            g.TranslateTransform(placa.X + placa.Width / 2f, placa.Y + placa.Height / 2f);
            g.ScaleTransform(0.62f, 0.62f);
            DibujarIcono(g, new Rectangle(-20, -20, 40, 40), acento);
            g.Restore(estado);

            using (var fTitulo = new Font("Segoe UI", 11f, FontStyle.Bold))
            using (var fDesc = new Font("Segoe UI", 8.5f))
            using (var fLink = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (var brTitulo = new SolidBrush(tinta))
            using (var brDesc = new SolidBrush(gris))
            using (var brLink = new SolidBrush(acento))
            {
                var izquierda = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };

                g.DrawString(Titulo, fTitulo, brTitulo, new RectangleF(18, 68, Width - 36, 24), izquierda);
                g.DrawString(Descripcion, fDesc, brDesc, new RectangleF(18, 92, Width - 36, 40), izquierda);

                string pie = _habilitada ? "Ingresar  →" : "Próximamente";
                using (var brPie = _habilitada ? brLink : new SolidBrush(Color.FromArgb(170, 174, 180)))
                    g.DrawString(pie, fLink, brPie, new RectangleF(18, Height - 34, Width - 36, 24), izquierda);
            }
        }

        private void DibujarIcono(Graphics g, Rectangle r, Color color)
        {
            using (var pen = new Pen(color, 2f) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
            using (var brush = new SolidBrush(color))
            {
                switch (Icono)
                {
                    case IconoTarjeta.Usuarios:
                        g.DrawEllipse(pen, r.X + 2, r.Y + 3, 13, 13);
                        g.DrawArc(pen, r.X - 3, r.Y + 16, 24, 24, 180, 180);
                        g.DrawEllipse(pen, r.X + 20, r.Y + 7, 11, 11);
                        g.DrawArc(pen, r.X + 15, r.Y + 19, 22, 22, 200, 160);
                        break;

                    case IconoTarjeta.Producto:
                        var caja = new Rectangle(r.X + 4, r.Y + 12, 28, 24);
                        g.DrawRectangle(pen, caja);
                        g.DrawLine(pen, caja.Left, caja.Top + 8, caja.Right, caja.Top + 8);
                        g.DrawLine(pen, caja.Left + 14, caja.Top, caja.Left + 14, caja.Top + 8);
                        g.DrawLine(pen, caja.Left, caja.Top, caja.Left + 7, r.Y + 4);
                        g.DrawLine(pen, caja.Right, caja.Top, caja.Right + 7, r.Y + 4);
                        g.DrawLine(pen, caja.Left + 7, r.Y + 4, caja.Right + 7, r.Y + 4);
                        break;

                    case IconoTarjeta.Auditoria:
                        g.DrawEllipse(pen, r.X + 4, r.Y + 4, 30, 30);
                        g.DrawLine(pen, r.X + 19, r.Y + 19, r.X + 19, r.Y + 10);
                        g.DrawLine(pen, r.X + 19, r.Y + 19, r.X + 27, r.Y + 22);
                        break;

                    case IconoTarjeta.Caja:
                        g.DrawRectangle(pen, r.X + 3, r.Y + 15, 32, 21);
                        g.DrawRectangle(pen, r.X + 9, r.Y + 5, 19, 10);
                        g.FillEllipse(brush, r.X + 8, r.Y + 24, 4, 4);
                        g.FillEllipse(brush, r.X + 16, r.Y + 24, 4, 4);
                        g.FillEllipse(brush, r.X + 24, r.Y + 24, 4, 4);
                        break;

                    case IconoTarjeta.Etiqueta:
                        Point[] tag =
                        {
                            new Point(r.X + 3, r.Y + 7),
                            new Point(r.X + 23, r.Y + 7),
                            new Point(r.X + 34, r.Y + 20),
                            new Point(r.X + 23, r.Y + 33),
                            new Point(r.X + 3, r.Y + 33)
                        };
                        g.DrawPolygon(pen, tag);
                        g.DrawEllipse(pen, r.X + 8, r.Y + 16, 6, 6);
                        break;

                    case IconoTarjeta.Reportes:
                        g.DrawLine(pen, r.X + 3, r.Y + 35, r.X + 37, r.Y + 35);
                        g.DrawRectangle(pen, r.X + 7, r.Y + 21, 6, 14);
                        g.DrawRectangle(pen, r.X + 17, r.Y + 9, 6, 26);
                        g.DrawRectangle(pen, r.X + 27, r.Y + 15, 6, 20);
                        break;
                }
            }
        }
    }
}
