using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Ventana única del sistema: menú lateral a la izquierda y, a la derecha, la sección
    /// activa. Cada sección es un UserControl que se muestra dentro de panelHost, así que
    /// navegar NO abre ventanas nuevas. Qué opciones ve cada rol lo decide NegocioSeguridad.
    /// </summary>
    public partial class frmPrincipal : Form
    {
        private static readonly Color ColorAcento = Color.FromArgb(59, 130, 246);
        private static readonly Color ColorAcentoFondo = Color.FromArgb(234, 242, 251);
        private static readonly Color ColorTexto = Color.FromArgb(45, 48, 54);
        private static readonly Color ColorTextoInactivo = Color.FromArgb(160, 163, 170);

        private readonly Dictionary<OpcionMenu, Button> _botonesMenu = new Dictionary<OpcionMenu, Button>();
        private OpcionMenu _seccionActual = OpcionMenu.Inicio;

        public frmPrincipal()
        {
            InitializeComponent();
        }

        private void frmPrincipal_Load(object sender, EventArgs e)
        {
            if (SesionActual.HaySesion)
                lblUsuario.Text = SesionActual.Usuario.NombreCompleto +
                                  Environment.NewLine + SesionActual.Usuario.Rol;

            ConstruirMenu();
            MostrarSeccion(OpcionMenu.Inicio);
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

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            NegocioAutenticacion.CerrarSesion();
            Application.Restart();
        }
    }
}
