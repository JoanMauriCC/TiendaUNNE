using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Ventana única del sistema: menú lateral a la izquierda y, a la derecha, la sección
    /// activa. Cada sección es un UserControl que se muestra dentro de panelHost, así que
    /// navegar NO abre ventanas nuevas. Qué opciones ve cada rol lo decide NegocioSeguridad.
    /// Arriba a la derecha está la ayuda (botón ? o F1), que recorre la pantalla abierta.
    /// </summary>
    public partial class frmPrincipal : Form, IPantallaConGuia
    {
        private static readonly Color ColorAcento = Color.FromArgb(59, 130, 246);
        private static readonly Color ColorAcentoFondo = Color.FromArgb(234, 242, 251);
        private static readonly Color ColorTexto = Color.FromArgb(45, 48, 54);
        private static readonly Color ColorTextoInactivo = Color.FromArgb(160, 163, 170);

        private readonly Dictionary<OpcionMenu, Button> _botonesMenu = new Dictionary<OpcionMenu, Button>();
        private OpcionMenu _seccionActual = OpcionMenu.Inicio;

        private VisitaGuiada _visita;
        private readonly ToolTip _tipAyuda = new ToolTip();
        private bool _ayudaResaltada;

        public frmPrincipal()
        {
            InitializeComponent();
        }

        private void frmPrincipal_Load(object sender, EventArgs e)
        {
            if (SesionActual.HaySesion)
                lblUsuario.Text = SesionActual.Usuario.NombreCompleto +
                                  Environment.NewLine + SesionActual.Usuario.Rol;

            _tipAyuda.SetToolTip(btnAyuda, "Ayuda: recorrer esta pantalla (F1)");

            ConstruirMenu();
            MostrarSeccion(OpcionMenu.Inicio);

            // Al entrar, el foco arranca en la primera tarjeta para poder usar las flechas
            // enseguida, sin tener que tocar Tab antes.
            TarjetaMenu primera = panelHost.Controls.Count == 0 ? null : PrimeraTarjeta(panelHost);
            if (primera != null)
                ActiveControl = primera;
        }

        /// <summary>La primera vez que el usuario entra al sistema, la Bienvenida arranca sola.</summary>
        private void frmPrincipal_Shown(object sender, EventArgs e)
        {
            if (NegocioGuia.DebeMostrarBienvenida(SesionActual.Usuario))
                MostrarAyuda(NegocioGuia.Bienvenida());
        }

        private static TarjetaMenu PrimeraTarjeta(Control raiz)
        {
            foreach (Control hijo in raiz.Controls)
            {
                var tarjeta = hijo as TarjetaMenu;
                if (tarjeta != null && tarjeta.Habilitada)
                    return tarjeta;

                tarjeta = PrimeraTarjeta(hijo);
                if (tarjeta != null)
                    return tarjeta;
            }
            return null;
        }

        // -----------------------------------------------------------------
        // Menú lateral
        // -----------------------------------------------------------------

        private void ConstruirMenu()
        {
            panelNav.SuspendLayout();
            panelNav.Controls.Clear();
            _botonesMenu.Clear();

            foreach (OpcionMenu opcion in NegocioSeguridad.OpcionesDelMenu())
            {
                bool disponible = NegocioSeguridad.ModuloDisponible(opcion);

                // Las opciones sin permiso no se muestran; las que existen pero todavía
                // no están implementadas se muestran deshabilitadas.
                if (!disponible || NegocioSeguridad.PuedeAcceder(opcion))
                {
                    Button boton = CrearBotonMenu(opcion, disponible);
                    panelNav.Controls.Add(boton);
                    _botonesMenu.Add(opcion, boton);
                }
            }

            panelNav.ResumeLayout();
        }

        private Button CrearBotonMenu(OpcionMenu opcion, bool habilitado)
        {
            var boton = new Button
            {
                Text = "   " + NombreDeSeccion(opcion),
                Tag = opcion,
                Enabled = habilitado,
                Width = 168,
                Height = 38,
                Margin = new Padding(0, 1, 0, 1),
                TextAlign = ContentAlignment.MiddleLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f),
                Cursor = habilitado ? Cursors.Hand : Cursors.Default,
                UseVisualStyleBackColor = false
            };

            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor = ColorAcentoFondo;

            if (habilitado)
                boton.Click += (s, e) => MostrarSeccion((OpcionMenu)((Button)s).Tag);

            return boton;
        }

        private void PintarBotonesMenu()
        {
            foreach (var par in _botonesMenu)
            {
                bool activo = par.Key == _seccionActual;
                Button boton = par.Value;

                boton.BackColor = activo ? ColorAcentoFondo : Color.White;
                boton.ForeColor = !boton.Enabled ? ColorTextoInactivo
                                : activo ? ColorAcento
                                : ColorTexto;
                boton.Font = new Font("Segoe UI", 9.5f, activo ? FontStyle.Bold : FontStyle.Regular);
            }
        }

        private static string NombreDeSeccion(OpcionMenu opcion)
        {
            switch (opcion)
            {
                case OpcionMenu.Inicio: return "Inicio";
                case OpcionMenu.Usuarios: return "Usuarios";
                case OpcionMenu.Productos: return "Productos";
                case OpcionMenu.Categorias: return "Categorías";
                case OpcionMenu.Auditoria: return "Auditoría";
                case OpcionMenu.Reportes: return "Reportes";
                case OpcionMenu.Caja: return "Caja";
                case OpcionMenu.Backup: return "Backup";
                default: return opcion.ToString();
            }
        }

        // -----------------------------------------------------------------
        // Navegación entre secciones
        // -----------------------------------------------------------------

        /// <summary>
        /// Reemplaza el contenido del panel derecho por la sección pedida. Se vuelve a
        /// pedir permiso acá aunque el botón ya estuviera visible: es la segunda barrera.
        /// </summary>
        private void MostrarSeccion(OpcionMenu opcion)
        {
            try
            {
                NegocioSeguridad.ValidarAcceso(opcion);
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Acceso denegado",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // La guía señala controles de la pantalla que se va: se cierra antes de cambiarla.
            if (_visita != null) _visita.Cerrar();

            Control contenido = CrearContenido(opcion);
            contenido.Dock = DockStyle.Fill;

            panelHost.SuspendLayout();
            foreach (Control anterior in panelHost.Controls)
                anterior.Dispose();
            panelHost.Controls.Clear();
            panelHost.Controls.Add(contenido);
            panelHost.ResumeLayout();

            _seccionActual = opcion;
            lblTituloSeccion.Text = NombreDeSeccion(opcion);
            PintarBotonesMenu();
        }

        private Control CrearContenido(OpcionMenu opcion)
        {
            switch (opcion)
            {
                case OpcionMenu.Usuarios: return new ucUsuarios();
                case OpcionMenu.Productos: return new ucProductos();
                case OpcionMenu.Categorias: return new ucCategorias();
                case OpcionMenu.Auditoria: return new ucAuditoria();
                case OpcionMenu.Reportes: return new ucReportes();
                case OpcionMenu.Caja: return new ucCaja();
                case OpcionMenu.Backup: return new ucBackup();
                default: return CrearPanelInicio();
            }
        }

        // -----------------------------------------------------------------
        // Sección Inicio: tarjetas de acceso rápido
        // -----------------------------------------------------------------

        private Control CrearPanelInicio()
        {
            var panel = new FlowLayoutPanel
            {
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(6, 6, 6, 6)
            };

            AgregarTarjetaInicio(panel, OpcionMenu.Usuarios, IconoTarjeta.Usuarios, "Altas, bajas y roles");
            AgregarTarjetaInicio(panel, OpcionMenu.Productos, IconoTarjeta.Producto, "Productos y stock");
            AgregarTarjetaInicio(panel, OpcionMenu.Categorias, IconoTarjeta.Etiqueta, "Organizá los rubros");
            AgregarTarjetaInicio(panel, OpcionMenu.Auditoria, IconoTarjeta.Auditoria, "Historial de cambios");
            AgregarTarjetaInicio(panel, OpcionMenu.Reportes, IconoTarjeta.Reportes, "Ventas y recaudación");
            AgregarTarjetaInicio(panel, OpcionMenu.Caja, IconoTarjeta.Caja, "Cobrar productos");
            AgregarTarjetaInicio(panel, OpcionMenu.Backup, IconoTarjeta.Backup, "Copias de seguridad");

            panel.Dock = DockStyle.Fill;

            // Al agregar, el último control con Dock.Top queda arriba de todo:
            // por eso primero las tarjetas, después el subtítulo y por último el saludo.
            var contenedor = new Panel { BackColor = Color.Transparent };
            contenedor.Controls.Add(panel);
            contenedor.Controls.Add(CrearSubtituloInicio());
            contenedor.Controls.Add(CrearSaludoInicio());
            return contenedor;
        }

        private Label CrearSaludoInicio()
        {
            string saludo = SesionActual.HaySesion && !string.IsNullOrWhiteSpace(SesionActual.Usuario.Nombre)
                ? "Bienvenido, " + SesionActual.Usuario.Nombre + "."
                : "Bienvenido.";

            return new Label
            {
                Text = saludo,
                Dock = DockStyle.Top,
                AutoSize = false,
                AutoEllipsis = true,
                Height = 64,
                Padding = new Padding(8, 0, 0, 0),
                Font = new Font("Segoe UI", 28f, FontStyle.Bold),
                ForeColor = ColorTexto,
                TextAlign = ContentAlignment.BottomLeft
            };
        }

        private Label CrearSubtituloInicio()
        {
            return new Label
            {
                Text = "Elegí un módulo para empezar.",
                Dock = DockStyle.Top,
                AutoSize = false,
                Height = 34,
                Padding = new Padding(10, 0, 0, 0),
                Font = new Font("Segoe UI", 11f),
                ForeColor = ColorTextoInactivo,
                TextAlign = ContentAlignment.TopLeft
            };
        }

        private void AgregarTarjetaInicio(Control contenedor, OpcionMenu opcion,
                                          IconoTarjeta icono, string descripcion)
        {
            bool disponible = NegocioSeguridad.ModuloDisponible(opcion);
            if (disponible && !NegocioSeguridad.PuedeAcceder(opcion))
                return;

            var tarjeta = new TarjetaMenu
            {
                Icono = icono,
                Titulo = NombreDeSeccion(opcion),
                Descripcion = descripcion,
                Habilitada = disponible
            };

            if (disponible)
                tarjeta.Click += (s, e) => MostrarSeccion(opcion);

            contenedor.Controls.Add(tarjeta);
        }

        // -----------------------------------------------------------------
        // Teclado en el dashboard: flechas para moverse entre tarjetas, Enter para abrir
        // -----------------------------------------------------------------

        /// <summary>
        /// Se intercepta acá y no en la tarjeta porque Windows procesa las flechas y Enter
        /// como teclas de diálogo antes de que el control llegue a verlas.
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F1)
            {
                MostrarAyuda(NegocioGuia.CapituloDe(_seccionActual));
                return true;
            }

            var tarjeta = ActiveControl as TarjetaMenu;
            if (tarjeta != null && ManejarTeclaEnTarjeta(tarjeta, keyData))
                return true;

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private static bool ManejarTeclaEnTarjeta(TarjetaMenu actual, Keys tecla)
        {
            if (tecla == Keys.Enter || tecla == Keys.Space)
            {
                actual.Activar();
                return true;
            }

            if (tecla != Keys.Left && tecla != Keys.Right && tecla != Keys.Up && tecla != Keys.Down)
                return false;

            List<TarjetaMenu> tarjetas = actual.Parent.Controls
                .OfType<TarjetaMenu>()
                .Where(t => t.Habilitada)
                .OrderBy(t => t.Top).ThenBy(t => t.Left)
                .ToList();

            int indice = tarjetas.IndexOf(actual);
            TarjetaMenu destino = null;

            if (tecla == Keys.Left)
                destino = indice > 0 ? tarjetas[indice - 1] : null;
            else if (tecla == Keys.Right)
                destino = indice < tarjetas.Count - 1 ? tarjetas[indice + 1] : null;
            else
                destino = TarjetaEnFilaVecina(tarjetas, actual, hacia: tecla == Keys.Up ? -1 : 1);

            if (destino != null)
                destino.Focus();

            return true;   // aunque no haya destino: que la flecha no mueva el foco a otro lado
        }

        /// <summary>Tarjeta de la fila de arriba (-1) o de abajo (1) más alineada con la actual.</summary>
        private static TarjetaMenu TarjetaEnFilaVecina(List<TarjetaMenu> tarjetas, TarjetaMenu actual, int hacia)
        {
            IEnumerable<TarjetaMenu> otras = hacia < 0
                ? tarjetas.Where(t => t.Top < actual.Top)
                : tarjetas.Where(t => t.Top > actual.Top);

            if (!otras.Any())
                return null;

            int filaVecina = hacia < 0 ? otras.Max(t => t.Top) : otras.Min(t => t.Top);

            return otras
                .Where(t => t.Top == filaVecina)
                .OrderBy(t => Math.Abs(t.Left - actual.Left))
                .First();
        }

        // -----------------------------------------------------------------
        // Ayuda: visita guiada de la pantalla abierta
        // -----------------------------------------------------------------

        private void btnAyuda_Click(object sender, EventArgs e)
        {
            MostrarAyuda(NegocioGuia.CapituloDe(_seccionActual));
        }

        private void MostrarAyuda(CapituloGuia capitulo)
        {
            if (_visita != null) _visita.Cerrar();

            var visita = new VisitaGuiada(this, this, capitulo);
            visita.Terminada += (s, e) => { if (_visita == s) _visita = null; };
            _visita = visita;
            visita.Iniciar();
        }

        /// <summary>
        /// Las partes de la ventana principal las resuelve ella; las de cada módulo, la pantalla
        /// que está abierta (si es una IPantallaConGuia).
        /// </summary>
        public Control Ancla(string clave)
        {
            switch (clave)
            {
                case AnclaGuia.Menu: return panelNav;
                case AnclaGuia.Titulo: return lblTituloSeccion;
                case AnclaGuia.Ayuda: return btnAyuda;
                case AnclaGuia.Sesion: return panelPie;
                case AnclaGuia.Contenido: return panelHost;
            }

            var pantalla = panelHost.Controls.Count > 0 ? panelHost.Controls[0] as IPantallaConGuia : null;
            return pantalla == null ? null : pantalla.Ancla(clave);
        }

        // El botón es un círculo con un signo de pregunta, como en la mayoría de los sistemas.
        private void btnAyuda_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(btnAyuda.BackColor);

            var circulo = new Rectangle(3, 3, btnAyuda.Width - 7, btnAyuda.Height - 7);
            Color color = _ayudaResaltada ? ColorAcento : ColorTexto;

            if (_ayudaResaltada)
                using (var fondo = new SolidBrush(ColorAcentoFondo))
                    g.FillEllipse(fondo, circulo);

            using (var borde = new Pen(color, 1.8f))
                g.DrawEllipse(borde, circulo);

            using (var fuente = new Font("Segoe UI", 11f, FontStyle.Bold))
                TextRenderer.DrawText(g, "?", fuente, circulo, color,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private void btnAyuda_MouseEnterLeave(object sender, EventArgs e)
        {
            _ayudaResaltada = btnAyuda.ClientRectangle.Contains(btnAyuda.PointToClient(Cursor.Position));
            btnAyuda.Invalidate();
        }

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            NegocioAutenticacion.CerrarSesion();
            Application.Restart();
        }
    }
}
