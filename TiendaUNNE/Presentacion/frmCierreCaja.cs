using System;
using System.Drawing;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Cierre del turno de caja. Muestra el arqueo: cuánto efectivo debería haber
    /// según el sistema contra cuánto se contó a mano, y la diferencia entre ambos.
    /// Las cuentas las hace NegocioCaja.
    /// </summary>
    public partial class frmCierreCaja : Form
    {
        private static readonly Color ColorDiferenciaOk = Color.FromArgb(13, 148, 136);
        private static readonly Color ColorDiferenciaMal = Color.FromArgb(200, 60, 60);

        private readonly CajaSesion _sesion;

        public frmCierreCaja(CajaSesion sesion)
        {
            InitializeComponent();
            _sesion = sesion;
        }

        private void frmCierreCaja_Load(object sender, EventArgs e)
        {
            try
            {
                ArqueoCaja arqueo = NegocioCaja.CalcularArqueo(_sesion, 0);

                lblMontoInicial.Text = arqueo.MontoInicial.ToString(NegocioCaja.FormatoImporte);
                lblVentas.Text = arqueo.VentasEnEfectivo.ToString(NegocioCaja.FormatoImporte);
                lblEsperado.Text = arqueo.EfectivoEsperado.ToString(NegocioCaja.FormatoImporte);

                // Se propone lo esperado: si el cajón cuadra, el cajero confirma y listo.
                numDeclarado.Value = Math.Min(arqueo.EfectivoEsperado, numDeclarado.Maximum);

                MostrarDiferencia();
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Cerrar caja",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.Cancel;
                Close();
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo calcular el arqueo de la caja.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void numDeclarado_ValueChanged(object sender, EventArgs e)
        {
            MostrarDiferencia();
        }

        private void MostrarDiferencia()
        {
            ArqueoCaja arqueo = NegocioCaja.CalcularArqueo(_sesion, numDeclarado.Value);

            lblDiferencia.Text = "Diferencia: " +
                arqueo.Diferencia.ToString(NegocioCaja.FormatoImporte);
            lblDiferencia.ForeColor = arqueo.Diferencia == 0
                ? ColorDiferenciaOk
                : ColorDiferenciaMal;
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                ArqueoCaja arqueo = NegocioCaja.CerrarCaja(
                    _sesion, numDeclarado.Value, SesionActual.Usuario.IdUsuario, txtObservaciones.Text);

                string mensaje = "Caja cerrada.";
                if (arqueo.Diferencia != 0)
                    mensaje += Environment.NewLine + Environment.NewLine +
                               "Quedó registrada una diferencia de " +
                               arqueo.Diferencia.ToString(NegocioCaja.FormatoImporte) + ".";

                MessageBox.Show(mensaje, "Cerrar caja",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo cerrar la caja",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo cerrar la caja.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
