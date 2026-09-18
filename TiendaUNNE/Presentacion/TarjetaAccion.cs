using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>Íconos disponibles para <see cref="TarjetaAccion"/>, dibujados con GDI+.</summary>
    public enum IconoAccion
    {
        Ver,
        Baja,
        Alta,
        BajaProducto,
        AltaProducto,
        Actualizar
    }

    /// <summary>
    /// Botón de acción en formato tarjeta (ícono + título + descripción), mismo lenguaje
    /// visual que <see cref="TarjetaMenu"/> pero en fila horizontal, pensado para una
    /// columna angosta de acciones (como la de la derecha en Usuarios).
    /// </summary>
    public class TarjetaAccion : Panel
    {
        private bool _hover;
        private bool _activa;

        public TarjetaAccion()
        {
            DoubleBuffered = true;
            Height = 44;
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
        }

        public IconoAccion Icono { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>Resaltada de forma permanente (p. ej. un switch que quedó "activado").</summary>
        public bool Activa
        {
            get => _activa;
            set { _activa = value; Invalidate(); }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            Invalidate();
            base.OnEnabledChanged(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true;
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

            bool resaltada = Enabled && (_hover || _activa);
            var marco = new Rectangle(0, 0, Width - 1, Height - 1);

            Color fondo = !Enabled ? Color.FromArgb(245, 245, 245)
                        : resaltada ? Color.FromArgb(234, 242, 251) : Color.White;
            Color borde = resaltada ? Color.FromArgb(74, 130, 200) : Color.FromArgb(214, 216, 220);
            Color colorIcono = Enabled ? Color.FromArgb(64, 108, 168) : Color.FromArgb(176, 178, 182);
            Color tinta = Enabled ? Color.FromArgb(45, 48, 54) : Color.FromArgb(150, 150, 150);
            Color gris = Color.FromArgb(128, 132, 140);

            using (var b = new SolidBrush(fondo))
                g.FillRectangle(b, marco);
            using (var p = new Pen(borde))
                g.DrawRectangle(p, marco);

            var areaIcono = new Rectangle(10, (Height - 24) / 2, 24, 24);
            DibujarIcono(g, areaIcono, colorIcono);

            int xTexto = areaIcono.Right + 10;
            using (var fTitulo = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (var fDesc = new Font("Segoe UI", 7.75f))
            using (var brTitulo = new SolidBrush(tinta))
            using (var brDesc = new SolidBrush(gris))
            {
                g.DrawString(Titulo, fTitulo, brTitulo, new RectangleF(xTexto, 7, Width - xTexto - 8, 16));
                g.DrawString(Descripcion, fDesc, brDesc, new RectangleF(xTexto, 24, Width - xTexto - 8, 16));
            }
        }

        /// <summary>Caja de producto en la esquina superior izquierda, para dejar lugar a la X o el tilde.</summary>
        private static void DibujarCaja(Graphics g, Pen pen, Rectangle r)
        {
            var caja = new Rectangle(r.X + 1, r.Y + 5, 14, 12);
            g.DrawRectangle(pen, caja);
            g.DrawLine(pen, caja.Left, caja.Top + 4, caja.Right, caja.Top + 4);
            g.DrawLine(pen, caja.Left + 7, caja.Top, caja.Left + 7, caja.Top + 4);
            g.DrawLine(pen, caja.Left, caja.Top, caja.Left + 3, r.Y + 1);
            g.DrawLine(pen, caja.Right, caja.Top, caja.Right - 3, r.Y + 1);
            g.DrawLine(pen, caja.Left + 3, r.Y + 1, caja.Right - 3, r.Y + 1);
        }

        private void DibujarIcono(Graphics g, Rectangle r, Color color)
        {
            using (var pen = new Pen(color, 1.8f) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
            using (var brush = new SolidBrush(color))
            {
                switch (Icono)
                {
                    case IconoAccion.Ver:
                        g.DrawArc(pen, r.X, r.Y + 4, r.Width, r.Height - 8, 20, 140);
                        g.DrawArc(pen, r.X, r.Y + 4, r.Width, r.Height - 8, 200, 140);
                        g.FillEllipse(brush, r.X + r.Width / 2 - 3, r.Y + r.Height / 2 - 3, 6, 6);
                        break;

                    case IconoAccion.Baja:
                        g.DrawEllipse(pen, r.X + 5, r.Y + 1, 10, 10);
                        g.DrawArc(pen, r.X, r.Y + 9, 20, 16, 180, 180);
                        g.DrawLine(pen, r.X + 15, r.Y + 15, r.X + 22, r.Y + 22);
                        g.DrawLine(pen, r.X + 22, r.Y + 15, r.X + 15, r.Y + 22);
                        break;

                    case IconoAccion.Alta:
                        g.DrawEllipse(pen, r.X + 5, r.Y + 1, 10, 10);
                        g.DrawArc(pen, r.X, r.Y + 9, 20, 16, 180, 180);
                        g.DrawLines(pen, new[]
                        {
                            new Point(r.X + 14, r.Y + 19),
                            new Point(r.X + 18, r.Y + 23),
                            new Point(r.X + 24, r.Y + 14)
                        });
                        break;

                    case IconoAccion.BajaProducto:
                        DibujarCaja(g, pen, r);
                        g.DrawLine(pen, r.X + 15, r.Y + 15, r.X + 22, r.Y + 22);
                        g.DrawLine(pen, r.X + 22, r.Y + 15, r.X + 15, r.Y + 22);
                        break;

                    case IconoAccion.AltaProducto:
                        DibujarCaja(g, pen, r);
                        g.DrawLines(pen, new[]
                        {
                            new Point(r.X + 14, r.Y + 19),
                            new Point(r.X + 18, r.Y + 23),
                            new Point(r.X + 24, r.Y + 14)
                        });
                        break;

                    case IconoAccion.Actualizar:
                        g.DrawArc(pen, r.X + 1, r.Y + 1, r.Width - 2, r.Height - 2, -30, 260);
                        Point[] flecha = { new Point(r.Right - 6, r.Y + 1), new Point(r.Right + 1, r.Y + 6), new Point(r.Right - 8, r.Y + 8) };
                        g.FillPolygon(brush, flecha);
                        break;
                }
            }
        }
    }
}
