using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Tarjeta de un producto en el catálogo de la caja: categoría, nombre, precio y estado de
    /// stock (verde, amarillo o rojo). Solo dibuja lo que le pasa la pantalla; los números ya
    /// vienen calculados por NegocioVenta. Sin stock disponible queda deshabilitada.
    /// </summary>
    public class TarjetaProducto : Control
    {
        private static readonly Color Verde = Color.FromArgb(21, 127, 75);
        private static readonly Color VerdeFondo = Color.FromArgb(226, 245, 234);
        private static readonly Color Ambar = Color.FromArgb(154, 91, 0);
        private static readonly Color AmbarFondo = Color.FromArgb(255, 241, 214);
        private static readonly Color Rojo = Color.FromArgb(180, 35, 24);
        private static readonly Color RojoFondo = Color.FromArgb(253, 232, 230);

        private static readonly Font _fuenteCategoria = new Font("Segoe UI", 7.5F);
        private static readonly Font _fuenteNombre = new Font("Segoe UI Semibold", 9.5F);
        private static readonly Font _fuentePrecio = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        private static readonly Font _fuenteEstado = new Font("Segoe UI", 8F);

        private bool _resaltada;
        private decimal _disponible;

        public TarjetaProducto()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable | ControlStyles.StandardClick, true);
            TabStop = true;
            Cursor = Cursors.Hand;
            Size = new Size(200, 104);
        }

        public ProductoVentaItem Producto { get; private set; }

        /// <summary>Asigna el producto y cuántas unidades se pueden todavía agregar al ticket.</summary>
        public void Configurar(ProductoVentaItem producto, decimal disponible)
        {
            Producto = producto;
            _disponible = disponible;
            Enabled = disponible > 0;
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e) { _resaltada = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _resaltada = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

        protected override bool IsInputKey(Keys keyData)
        {
            return keyData == Keys.Enter || keyData == Keys.Space || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Enabled && (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space))
            {
                e.Handled = true;
                OnClick(EventArgs.Empty);
            }
            base.OnKeyDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent == null ? SystemColors.Control : Parent.BackColor);

            if (Producto == null) return;

            bool resaltar = (_resaltada || Focused) && Enabled;
            Color fondo = Enabled ? Color.White : Color.FromArgb(245, 245, 245);
            Color borde = resaltar ? Color.FromArgb(47, 111, 237) : Color.FromArgb(214, 216, 220);
            Color tinta = Enabled ? Color.FromArgb(27, 35, 48) : Color.FromArgb(150, 150, 150);
            Color gris = Color.FromArgb(128, 132, 140);

            Rectangle caja = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath ruta = Redondeado(caja, 8))
            using (var pincel = new SolidBrush(fondo))
            using (var lapiz = new Pen(borde, resaltar ? 2f : 1f))
            {
                g.FillPath(pincel, ruta);
                g.DrawPath(lapiz, ruta);
            }

            const int m = 12;
            int ancho = Width - 2 * m;

            TextRenderer.DrawText(g, Producto.Categoria.ToUpperInvariant(), _fuenteCategoria,
                new Rectangle(m, 9, ancho, 14), gris,
                TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

            TextRenderer.DrawText(g, Producto.Nombre, _fuenteNombre,
                new Rectangle(m, 25, ancho, 38), tinta,
                TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

            int base_y = Height - 30;

            string precio = "$ " + Producto.PrecioVenta.ToString(NegocioVenta.FormatoImporte);
            TextRenderer.DrawText(g, precio, _fuentePrecio, new Rectangle(m, base_y, ancho, 20), tinta,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            string texto; Color colorTexto, colorFondo;
            if (_disponible <= 0)
            {
                texto = "Sin stock"; colorTexto = Rojo; colorFondo = RojoFondo;
            }
            else if (Producto.StockMinimo > 0 && _disponible <= Producto.StockMinimo)
            {
                texto = "Quedan " + _disponible.ToString(NegocioVenta.FormatoCantidadTicket);
                colorTexto = Ambar; colorFondo = AmbarFondo;
            }
            else
            {
                texto = "Disp. " + _disponible.ToString(NegocioVenta.FormatoCantidadTicket);
                colorTexto = Verde; colorFondo = VerdeFondo;
            }

            Size t = TextRenderer.MeasureText(g, texto, _fuenteEstado, new Size(ancho, 20),
                TextFormatFlags.NoPadding);
            var chip = new Rectangle(Width - m - t.Width - 14, base_y + 1, t.Width + 14, 18);
            using (GraphicsPath ruta = Redondeado(chip, 9))
            using (var pincel = new SolidBrush(colorFondo))
                g.FillPath(pincel, ruta);
            TextRenderer.DrawText(g, texto, _fuenteEstado, chip, colorTexto,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            if (Producto.ReservadoPorOtros > 0)
            {
                string reservado = Producto.ReservadoPorOtros.ToString(NegocioVenta.FormatoCantidadTicket) + " reservado";
                TextRenderer.DrawText(g, reservado, _fuenteEstado,
                    new Rectangle(m, base_y - 15, ancho, 14), Ambar,
                    TextFormatFlags.Right | TextFormatFlags.NoPadding);
            }
        }

        private static GraphicsPath Redondeado(Rectangle r, int radio)
        {
            int d = radio * 2;
            var ruta = new GraphicsPath();
            ruta.AddArc(r.X, r.Y, d, d, 180, 90);
            ruta.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            ruta.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            ruta.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            ruta.CloseFigure();
            return ruta;
        }
    }
}
