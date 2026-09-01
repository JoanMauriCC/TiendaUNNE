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
        Etiqueta
    }

    /// <summary>
    /// Tarjeta clickeable estilo dashboard: ícono GDI+, título en negrita y descripción en gris.
    /// El evento Click del Panel se dispara al hacer clic en cualquier parte de la tarjeta.
    /// </summary>
    public class TarjetaMenu : Panel
    {
        private bool _habilitada = true;
        private bool _hover;

        public TarjetaMenu()
        {
            DoubleBuffered = true;
            Size = new Size(160, 140);
            Margin = new Padding(12);
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            var marco = new Rectangle(0, 0, Width - 1, Height - 1);

            Color fondo = !_habilitada
                ? Color.FromArgb(245, 245, 245)
                : (_hover ? Color.FromArgb(234, 242, 251) : Color.White);
            Color borde = (_hover && _habilitada)
                ? Color.FromArgb(74, 130, 200)
                : Color.FromArgb(214, 216, 220);
            Color tinta = _habilitada ? Color.FromArgb(45, 48, 54) : Color.FromArgb(150, 150, 150);
            Color gris = Color.FromArgb(128, 132, 140);
            Color colorIcono = _habilitada ? Color.FromArgb(64, 108, 168) : Color.FromArgb(176, 178, 182);

            using (var b = new SolidBrush(fondo))
                g.FillRectangle(b, marco);
            using (var p = new Pen(borde))
                g.DrawRectangle(p, marco);

            var areaIcono = new Rectangle(Width / 2 - 20, 16, 40, 40);
            DibujarIcono(g, areaIcono, colorIcono);

            using (var fTitulo = new Font("Segoe UI", 10.5f, FontStyle.Bold))
            using (var fDesc = new Font("Segoe UI", 8f))
            using (var brTitulo = new SolidBrush(tinta))
            using (var brDesc = new SolidBrush(gris))
            {
                var centro = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Near,
                    Trimming = StringTrimming.EllipsisCharacter
                };
                g.DrawString(Titulo, fTitulo, brTitulo, new RectangleF(6, 66, Width - 12, 22), centro);
                g.DrawString(Descripcion, fDesc, brDesc, new RectangleF(6, 90, Width - 12, 40), centro);
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
                }
            }
        }
    }
}
