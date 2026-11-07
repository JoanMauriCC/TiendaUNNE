using System;
using System.Drawing;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Botón «Historial de ventas» del encabezado de la Caja: abre la lista de ventas (del turno o de
    /// un período más largo), desde donde se puede volver a abrir y guardar el ticket de cualquier venta.
    /// </summary>
    public partial class ucCaja
    {
        private Button btnVentas;

        private void ConstruirVentas()
        {
            btnVentas = new Button
            {
                Text = "Historial de ventas (F6)",
                Size = new Size(170, 28),
                Location = new Point(btnCerrarCaja.Left - 170 - 8, btnCerrarCaja.Top),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                UseVisualStyleBackColor = true
            };
            btnVentas.Click += (s, e) => VerHistorial();
            panelEncabezado.Controls.Add(btnVentas);
        }

        private void VerHistorial()
        {
            if (_sesion == null) return;

            using (var ventana = new frmHistorialVentas(_sesion))
                ventana.ShowDialog(this);
        }
    }
}