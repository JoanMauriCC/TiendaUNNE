using System;
using System.Drawing;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Botón "Ventas del turno" del encabezado de la Caja: abre la lista de ventas del turno,
    /// desde donde un Administrador o un Supervisor puede anular una.
    /// </summary>
    public partial class ucCaja
    {
        private Button btnVentas;

        private void ConstruirVentas()
        {
            btnVentas = new Button
            {
                Text = "Ventas del turno (F6)",
                Size = new Size(150, 28),
                Location = new Point(btnCerrarCaja.Left - 150 - 8, btnCerrarCaja.Top),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                UseVisualStyleBackColor = true
            };
            btnVentas.Click += (s, e) => VerVentasDelTurno();
            panelEncabezado.Controls.Add(btnVentas);
        }

        private void VerVentasDelTurno()
        {
            if (_sesion == null) return;

            using (var ventana = new frmVentasTurno(_sesion))
            {
                ventana.ShowDialog(this);

                // Si se anuló una venta, volvió stock al depósito: el catálogo tiene que mostrarlo.
                if (ventana.HuboAnulaciones)
                    CargarCatalogo();
            }
        }
    }
}
