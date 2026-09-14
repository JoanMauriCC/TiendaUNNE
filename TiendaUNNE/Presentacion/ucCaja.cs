using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Sección de Caja: abre el turno, arma el ticket y dispara el cobro y el cierre.
    /// Solo muestra datos y recoge lo que hace el cajero; validar stock, calcular
    /// totales y registrar la venta es trabajo de NegocioVenta y NegocioCaja.
    /// </summary>
    public partial class ucCaja : UserControl
    {
        private CajaSesion _sesion;
        private VentaEditModel _venta;

        public ucCaja()
        {
            InitializeComponent();
        }

        private void ucCaja_Load(object sender, EventArgs e)
        {
            RefrescarEstadoDeCaja();
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
        }

        // -----------------------------------------------------------------
        // Apertura y cierre del turno
        // -----------------------------------------------------------------

        private void btnAbrirCaja_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                NegocioCaja.AbrirCaja(numMontoInicial.Value, SesionActual.Usuario.IdUsuario);
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
        // Armado del ticket
        // -----------------------------------------------------------------

        private void IniciarVenta()
        {
            _venta = NegocioVenta.NuevaVenta(_sesion, SesionActual.Usuario.IdUsuario);
            txtBuscar.Clear();
            cboProducto.DataSource = null;
            lblInfoProducto.Text = string.Empty;
            numCantidad.Value = 1;
            RefrescarTicket();
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
            btnQuitar.Enabled = !_venta.EstaVacia;
            btnCancelarVenta.Enabled = !_venta.EstaVacia;
        }

        private void AplicarFormatoTicket()
        {
            SetColumna("Descripcion", "Producto", null);
            SetColumna("Cantidad", "Cantidad", NegocioVenta.FormatoCantidad);
            SetColumna("PrecioUnitario", "Precio unit.", NegocioVenta.FormatoImporte);
            SetColumna("Subtotal", "Subtotal", NegocioVenta.FormatoImporte);
        }

        private void SetColumna(string columna, string encabezado, string formato)
        {
            if (!dgvTicket.Columns.Contains(columna)) return;

            dgvTicket.Columns[columna].HeaderText = encabezado;

            if (formato == null) return;

            dgvTicket.Columns[columna].DefaultCellStyle.Format = formato;
            dgvTicket.Columns[columna].DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleRight;
        }

        private void txtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            e.SuppressKeyPress = true;   // evita el "beep" de Windows al apretar Enter
            btnBuscar_Click(sender, e);
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                List<ProductoVentaItem> resultados = NegocioVenta.BuscarProductos(txtBuscar.Text);

                cboProducto.DataSource = null;
                cboProducto.DataSource = resultados;
                cboProducto.DisplayMember = "Nombre";
                cboProducto.ValueMember = "IdProducto";

                if (resultados.Count == 0)
                {
                    lblInfoProducto.Text = "Sin resultados";
                    return;
                }

                cboProducto.SelectedIndex = 0;
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo buscar productos.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void cboProducto_SelectedIndexChanged(object sender, EventArgs e)
        {
            var producto = cboProducto.SelectedItem as ProductoVentaItem;

            lblInfoProducto.Text = producto == null
                ? string.Empty
                : string.Format("{0}  ·  stock {1}",
                    producto.PrecioVenta.ToString(NegocioVenta.FormatoImporte),
                    producto.Stock.ToString(NegocioVenta.FormatoCantidad));
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            try
            {
                NegocioVenta.AgregarProducto(
                    _venta, cboProducto.SelectedItem as ProductoVentaItem, numCantidad.Value);

                RefrescarTicket();

                numCantidad.Value = 1;
                txtBuscar.SelectAll();
                txtBuscar.Focus();
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo agregar",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnQuitar_Click(object sender, EventArgs e)
        {
            try
            {
                NegocioVenta.QuitarRenglon(_venta, IndiceSeleccionado());
                RefrescarTicket();
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
                    "Cobro completado." +
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
