using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Cobro de la venta: se cargan uno o varios medios de pago hasta cubrir el total.
    /// Cuánto falta, cuánto sobra y si la venta se puede confirmar lo resuelve NegocioVenta.
    /// </summary>
    public partial class frmCobro : Form
    {
        private readonly VentaEditModel _venta;

        public frmCobro(VentaEditModel venta)
        {
            InitializeComponent();
            _venta = venta;
        }

        /// <summary>Vuelto que hay que devolverle al cliente.</summary>
        public decimal Vuelto { get; private set; }

        private void frmCobro_Load(object sender, EventArgs e)
        {
            // Se empieza siempre de cero: si antes se abrió el cobro y se canceló,
            // los pagos de aquella vez no tienen que arrastrarse.
            _venta.Pagos.Clear();

            try
            {
                cboMedioPago.DataSource = NegocioVenta.ListarMediosDePago();
                cboMedioPago.DisplayMember = "Nombre";
                cboMedioPago.ValueMember = "Id";
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudieron cargar los medios de pago.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            lblTotal.Text = _venta.Total.ToString(NegocioVenta.FormatoImporte);
            RefrescarPagos();
            numImporte.Focus();
        }

        private void RefrescarPagos()
        {
            dgvPagos.DataSource = null;
            dgvPagos.DataSource = new BindingList<PagoVenta>(_venta.Pagos);

            foreach (string oculta in new[] { "IdMedioPago", "EsEfectivo", "Referencia" })
                if (dgvPagos.Columns.Contains(oculta))
                    dgvPagos.Columns[oculta].Visible = false;

            if (dgvPagos.Columns.Contains("NombreMedioPago"))
                dgvPagos.Columns["NombreMedioPago"].HeaderText = "Medio de pago";

            if (dgvPagos.Columns.Contains("Importe"))
            {
                dgvPagos.Columns["Importe"].HeaderText = "Importe";
                dgvPagos.Columns["Importe"].DefaultCellStyle.Format = NegocioVenta.FormatoImporte;
                dgvPagos.Columns["Importe"].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleRight;
            }

            decimal restante = NegocioVenta.CalcularRestante(_venta);
            Vuelto = NegocioVenta.CalcularVuelto(_venta);

            lblRestante.Text = "Restante: " + restante.ToString(NegocioVenta.FormatoImporte);
            lblVuelto.Text = Vuelto > 0
                ? "Vuelto: " + Vuelto.ToString(NegocioVenta.FormatoImporte)
                : string.Empty;

            // Proponer lo que falta agiliza el caso más común: un solo medio de pago.
            numImporte.Value = Math.Min(restante, numImporte.Maximum);

            btnQuitarPago.Enabled = _venta.Pagos.Count > 0;
            btnConfirmar.Enabled = _venta.Pagos.Count > 0 && restante == 0;
        }

        private void btnAgregarPago_Click(object sender, EventArgs e)
        {
            var medio = cboMedioPago.SelectedItem as MedioPagoItem;

            if (medio == null)
            {
                MessageBox.Show("Elegí un medio de pago.", "Cobrar",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (numImporte.Value <= 0)
            {
                MessageBox.Show("El importe tiene que ser mayor a cero.", "Cobrar",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _venta.Pagos.Add(new PagoVenta
            {
                IdMedioPago = medio.Id,
                NombreMedioPago = medio.Nombre,
                EsEfectivo = medio.EsEfectivo,
                Importe = numImporte.Value
            });

            RefrescarPagos();
        }

        private void btnQuitarPago_Click(object sender, EventArgs e)
        {
            if (dgvPagos.CurrentRow == null) return;

            int indice = dgvPagos.CurrentRow.Index;
            if (indice < 0 || indice >= _venta.Pagos.Count) return;

            _venta.Pagos.RemoveAt(indice);
            RefrescarPagos();
        }

        private void btnConfirmar_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                // El vuelto se guarda antes de confirmar, porque al confirmar
                // los pagos se ajustan para que sumen exactamente el total.
                Vuelto = NegocioVenta.CalcularVuelto(_venta);
                NegocioVenta.ConfirmarVenta(_venta);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo cobrar",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo completar el cobro.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            _venta.Pagos.Clear();
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
