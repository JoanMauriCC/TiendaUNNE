using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Sección de Caja: abre el turno, muestra el catálogo con buscador y filtro por categoría,
    /// arma el ticket y dispara el cobro y el cierre.
    /// Solo muestra datos y recoge lo que hace el cajero; validar stock, filtrar el catálogo,
    /// calcular totales y registrar la venta es trabajo de NegocioVenta y NegocioCaja.
    ///
    ///   F2 / Ctrl+F  ir al buscador        F4  cobrar        F5  actualizar el catálogo
    ///   Esc          limpiar la búsqueda   Enter (en el buscador) agrega si queda un solo producto
    /// </summary>
    public partial class ucCaja : UserControl
    {
        private static readonly Color ColorAcento = Color.FromArgb(47, 111, 237);
        private static readonly Color ColorBordeChip = Color.FromArgb(214, 216, 220);

        private CajaSesion _sesion;
        private VentaEditModel _venta;
        private List<ProductoVentaItem> _catalogo = new List<ProductoVentaItem>();
        private string _categoria;   // null = todas

        // Cada 5 minutos se renuevan las reservas del ticket abierto (vencen a los 30).
        private readonly Timer _timerReservas = new Timer { Interval = 5 * 60 * 1000 };

        public ucCaja()
        {
            InitializeComponent();
            ConstruirApertura();

            _timerReservas.Tick += (s, e) => NegocioVenta.RenovarReservas(_venta);
            _timerReservas.Start();

            dgvTicket.SelectionChanged += (s, e) => ActualizarBotonesDeRenglon();

            // Al salir de la sección o cerrar la aplicación, el stock reservado vuelve a estar libre.
            Disposed += (s, e) =>
            {
                _timerReservas.Dispose();
                NegocioVenta.LiberarReservas(_venta);
            };
        }

        private void ucCaja_Load(object sender, EventArgs e)
        {
            RefrescarEstadoDeCaja();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (panelAbierta.Visible)
            {
                switch (keyData)
                {
                    case Keys.F2:
                    case Keys.Control | Keys.F:
                        txtBuscar.Focus();
                        txtBuscar.SelectAll();
                        return true;

                    case Keys.F4:
                        if (btnCobrar.Enabled) btnCobrar.PerformClick();
                        return true;

                    case Keys.F5:
                        CargarCatalogo();
                        return true;

                    case Keys.Escape:
                        if (txtBuscar.Text.Length > 0 || _categoria != null)
                        {
                            txtBuscar.Clear();
                            _categoria = null;
                            MostrarCatalogo();
                            return true;
                        }
                        break;
                }
            }
            else if (panelCerrada.Visible && keyData == (Keys.Control | Keys.Enter))
            {
                btnAbrirCaja.PerformClick();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>Muestra la pantalla de apertura o la de venta según cómo esté la caja.</summary>
        private void RefrescarEstadoDeCaja()
        {
            try
            {
                _sesion = NegocioCaja.ObtenerSesionAbierta();
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Caja", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _sesion = null;
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo consultar el estado de la caja.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _sesion = null;
            }

            bool abierta = _sesion != null;
            panelAbierta.Visible = abierta;
            panelCerrada.Visible = !abierta;

            if (abierta)
            {
                lblSesion.Text = string.Format("{0} · abierta {1:dd/MM/yyyy HH:mm} · {2}",
                    _sesion.NombreCaja, _sesion.FechaApertura, _sesion.UsuarioApertura);

                IniciarVenta();
                txtBuscar.Focus();
            }
            else
            {
                CargarApertura();
            }
        }

        // -----------------------------------------------------------------
        // Apertura y cierre del turno
        // -----------------------------------------------------------------

        private void btnAbrirCaja_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                NegocioCaja.AbrirCaja(
                    TotalDeclarado, SesionActual.Usuario.IdUsuario, txtObservacionesApertura.Text);
                RefrescarEstadoDeCaja();
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo abrir la caja",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo abrir la caja.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnCerrarCaja_Click(object sender, EventArgs e)
        {
            if (_venta != null && !_venta.EstaVacia)
            {
                MessageBox.Show("Hay una venta sin cobrar. Cobrala o cancelala antes de cerrar la caja.",
                    "Venta en curso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var cierre = new frmCierreCaja(_sesion))
            {
                if (cierre.ShowDialog(this) == DialogResult.OK)
                    RefrescarEstadoDeCaja();
            }
        }

        // -----------------------------------------------------------------
        // Catálogo: buscador, categorías y tarjetas de productos
        // -----------------------------------------------------------------

        /// <summary>Lee el catálogo de la base (con el stock libre para este ticket) y lo vuelve a mostrar.</summary>
        private void CargarCatalogo()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                _catalogo = NegocioVenta.ListarCatalogo(_venta);

                // Si la categoría elegida ya no existe en el catálogo, se vuelve a "todas".
                if (_categoria != null &&
                    !NegocioVenta.CategoriasDelCatalogo(_catalogo)
                        .Any(c => string.Equals(c, _categoria, StringComparison.CurrentCultureIgnoreCase)))
                    _categoria = null;
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo leer el catálogo de productos.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _catalogo = new List<ProductoVentaItem>();
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            MostrarCatalogo();
        }

        private void MostrarCatalogo()
        {
            MostrarCategorias();
            MostrarProductos();
        }

        private bool OcultarSinStock => chkOcultarSinStock.Checked;

        private void MostrarCategorias()
        {
            flowCategorias.SuspendLayout();
            LimpiarContenedor(flowCategorias);

            flowCategorias.Controls.Add(CrearChip("Todas", null));
            foreach (string categoria in NegocioVenta.CategoriasDelCatalogo(_catalogo))
                flowCategorias.Controls.Add(CrearChip(categoria, categoria));

            flowCategorias.ResumeLayout();
        }

        private Button CrearChip(string texto, string categoria)
        {
            // El número de cada chip respeta el buscador y la casilla de sin stock.
            int cantidad = NegocioVenta.FiltrarCatalogo(
                _catalogo, _venta, txtBuscar.Text, categoria, OcultarSinStock).Count;

            bool activo = string.Equals(categoria, _categoria, StringComparison.CurrentCultureIgnoreCase);

            var chip = new Button
            {
                Text = texto + "  " + cantidad,
                Tag = categoria,
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, activo ? FontStyle.Bold : FontStyle.Regular),
                BackColor = activo ? ColorAcento : Color.White,
                ForeColor = activo ? Color.White : Color.FromArgb(45, 48, 54),
                Margin = new Padding(0, 0, 8, 0),
                Padding = new Padding(6, 1, 6, 1),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            chip.FlatAppearance.BorderColor = activo ? ColorAcento : ColorBordeChip;
            chip.Click += (s, e) =>
            {
                _categoria = (string)((Button)s).Tag;
                MostrarCatalogo();
            };
            return chip;
        }

        private void MostrarProductos()
        {
            List<ProductoVentaItem> lista = NegocioVenta.FiltrarCatalogo(
                _catalogo, _venta, txtBuscar.Text, _categoria, OcultarSinStock);

            lblCuenta.Text = lista.Count + (lista.Count == 1 ? " producto" : " productos");

            flowProductos.SuspendLayout();
            LimpiarContenedor(flowProductos);

            if (lista.Count == 0)
            {
                flowProductos.Controls.Add(new Label
                {
                    AutoSize = true,
                    ForeColor = Color.FromArgb(120, 125, 135),
                    Margin = new Padding(4, 16, 0, 0),
                    Text = "Sin resultados. Probá con otra palabra o elegí «Todas»."
                });
            }

            foreach (ProductoVentaItem producto in lista)
            {
                var tarjeta = new TarjetaProducto { Margin = new Padding(0, 0, 10, 10) };
                tarjeta.Configurar(producto, NegocioVenta.Disponible(_venta, producto));
                tarjeta.Click += Tarjeta_Click;
                flowProductos.Controls.Add(tarjeta);
            }

            flowProductos.ResumeLayout();
        }

        /// <summary>Después de agregar o quitar algo, actualiza el "disponible" sin rearmar ni mover la lista.</summary>
        private void ActualizarTarjetas()
        {
            foreach (TarjetaProducto tarjeta in flowProductos.Controls.OfType<TarjetaProducto>())
                tarjeta.Configurar(tarjeta.Producto, NegocioVenta.Disponible(_venta, tarjeta.Producto));
        }

        private static void LimpiarContenedor(Control contenedor)
        {
            foreach (Control c in contenedor.Controls.Cast<Control>().ToList())
            {
                contenedor.Controls.Remove(c);
                c.Dispose();
            }
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            MostrarCatalogo();
        }

        private void chkOcultarSinStock_CheckedChanged(object sender, EventArgs e)
        {
            MostrarCatalogo();
        }

        private void txtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            e.SuppressKeyPress = true;   // evita el "beep" de Windows al apretar Enter

            // Con un solo resultado posible, Enter lo agrega: el cajero no necesita el mouse.
            List<ProductoVentaItem> lista = NegocioVenta.FiltrarCatalogo(
                _catalogo, _venta, txtBuscar.Text, _categoria, true);

            if (lista.Count == 1)
            {
                AgregarAlTicket(lista[0]);
                txtBuscar.SelectAll();
            }
        }

        private void Tarjeta_Click(object sender, EventArgs e)
        {
            var tarjeta = sender as TarjetaProducto;
            if (tarjeta != null)
                AgregarAlTicket(tarjeta.Producto);
        }

        // -----------------------------------------------------------------
        // Armado del ticket
        // -----------------------------------------------------------------

        private void IniciarVenta()
        {
            NegocioVenta.LiberarReservas(_venta);   // suelta lo que tuviera el ticket anterior
            _venta = NegocioVenta.NuevaVenta(_sesion, SesionActual.Usuario.IdUsuario);
            txtBuscar.Clear();
            _categoria = null;
            RefrescarTicket();
            CargarCatalogo();
        }

        private void RefrescarTicket()
        {
            dgvTicket.DataSource = null;
            dgvTicket.DataSource = new BindingList<RenglonVenta>(_venta.Renglones);

            if (dgvTicket.Columns.Contains("IdProducto"))
                dgvTicket.Columns["IdProducto"].Visible = false;

            AplicarFormatoTicket();

            lblTotal.Text = _venta.Total.ToString(NegocioVenta.FormatoImporte);
            btnCobrar.Enabled = !_venta.EstaVacia;
            btnCancelarVenta.Enabled = !_venta.EstaVacia;
            btnCobrar_EnabledChanged(btnCobrar, EventArgs.Empty);

            // La grilla marca sola el primer renglón al cargarse, pero sin que esté realmente elegido:
            // se deja sin selección y el cajero elige el renglón que quiere tocar.
            dgvTicket.CurrentCell = null;
            dgvTicket.ClearSelection();
            ActualizarBotonesDeRenglon();
        }

        /// <summary>Quitar y +/− 1 unidad solo tienen sentido con un renglón elegido.</summary>
        private void ActualizarBotonesDeRenglon()
        {
            bool hayRenglon = dgvTicket.CurrentRow != null;
            btnQuitar.Enabled = hayRenglon;
            btnMas.Enabled = hayRenglon;
            btnMenos.Enabled = hayRenglon;
        }

        // Un botón plano deshabilitado no se ve "apagado" por sí solo: se le cambia el color.
        private void btnCobrar_EnabledChanged(object sender, EventArgs e)
        {
            btnCobrar.BackColor = btnCobrar.Enabled ? ColorAcento : Color.FromArgb(214, 216, 220);
            btnCobrar.ForeColor = btnCobrar.Enabled ? Color.White : Color.FromArgb(120, 125, 135);
        }

        private void AplicarFormatoTicket()
        {
            SetColumna("Descripcion", "Producto", null, 150);
            SetColumna("Cantidad", "Cant.", NegocioVenta.FormatoCantidadTicket, 45);
            SetColumna("PrecioUnitario", "Precio", NegocioVenta.FormatoImporte, 70);
            SetColumna("Subtotal", "Subtotal", NegocioVenta.FormatoImporte, 75);
        }

        private void SetColumna(string columna, string encabezado, string formato, float peso)
        {
            if (!dgvTicket.Columns.Contains(columna)) return;

            dgvTicket.Columns[columna].HeaderText = encabezado;
            dgvTicket.Columns[columna].FillWeight = peso;

            if (formato == null) return;

            dgvTicket.Columns[columna].DefaultCellStyle.Format = formato;
            dgvTicket.Columns[columna].DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleRight;
        }

        private void AgregarAlTicket(ProductoVentaItem producto)
        {
            try
            {
                NegocioVenta.AgregarProducto(_venta, producto, 1);
                RefrescarTicket();
                ActualizarTarjetas();
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo agregar",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                CargarCatalogo();   // otra caja pudo llevarse stock: se muestra lo real
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo agregar el producto al ticket.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnMas_Click(object sender, EventArgs e)
        {
            int indice = IndiceSeleccionado();
            if (indice < 0 || indice >= _venta.Renglones.Count) return;

            int idProducto = _venta.Renglones[indice].IdProducto;
            ProductoVentaItem producto = _catalogo.FirstOrDefault(p => p.IdProducto == idProducto);
            if (producto != null)
            {
                AgregarAlTicket(producto);
                SeleccionarRenglon(indice);
            }
        }

        private void btnMenos_Click(object sender, EventArgs e)
        {
            int indice = IndiceSeleccionado();

            try
            {
                int antes = _venta.Renglones.Count;
                NegocioVenta.RestarUnaUnidad(_venta, indice);
                RefrescarTicket();
                ActualizarTarjetas();

                if (_venta.Renglones.Count == antes)
                    SeleccionarRenglon(indice);
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo quitar",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo actualizar el ticket.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnQuitar_Click(object sender, EventArgs e)
        {
            try
            {
                NegocioVenta.QuitarRenglon(_venta, IndiceSeleccionado());
                RefrescarTicket();
                ActualizarTarjetas();
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo quitar",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private int IndiceSeleccionado()
        {
            return dgvTicket.CurrentRow == null ? -1 : dgvTicket.CurrentRow.Index;
        }

        private void SeleccionarRenglon(int indice)
        {
            if (indice < 0 || indice >= dgvTicket.Rows.Count) return;

            dgvTicket.ClearSelection();
            dgvTicket.Rows[indice].Selected = true;
            dgvTicket.CurrentCell = dgvTicket.Rows[indice].Cells["Descripcion"];
            ActualizarBotonesDeRenglon();
        }

        private void btnCancelarVenta_Click(object sender, EventArgs e)
        {
            var r = MessageBox.Show("¿Descartar la venta en curso?", "Cancelar venta",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (r == DialogResult.Yes)
                IniciarVenta();
        }

        // -----------------------------------------------------------------
        // Cobro
        // -----------------------------------------------------------------

        private void btnCobrar_Click(object sender, EventArgs e)
        {
            using (var cobro = new frmCobro(_venta))
            {
                if (cobro.ShowDialog(this) != DialogResult.OK)
                    return;

                MessageBox.Show(
                    "Cobro completado. Ticket Nº " +
                    cobro.NumeroTicket.ToString(NegocioVenta.FormatoNumeroTicket) + "." +
                    (cobro.Vuelto > 0
                        ? Environment.NewLine + Environment.NewLine + "Vuelto: " +
                          cobro.Vuelto.ToString(NegocioVenta.FormatoImporte)
                        : string.Empty),
                    "Cobro", MessageBoxButtons.OK, MessageBoxIcon.Information);

                IniciarVenta();
                txtBuscar.Focus();
            }
        }
    }
}
