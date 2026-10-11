using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Pantalla de apertura de caja: el cajero declara el efectivo que hay en el cajón, ya sea
    /// escribiendo el total o contando billetes, y ve el resumen del turno antes de abrir.
    /// Solo muestra y suma lo que se carga; si el monto es válido, si coincide con el cierre
    /// anterior y qué queda en la auditoría lo resuelve NegocioCaja.
    /// </summary>
    public partial class ucCaja
    {
        private static readonly decimal[] Billetes = { 20000m, 10000m, 2000m, 1000m, 500m, 200m, 100m };
        private static readonly decimal[] MontosRapidos = { 0m, 2000m, 5000m, 10000m, 20000m };

        private static readonly Color Gris = Color.FromArgb(120, 125, 135);
        private static readonly Color Tinta = Color.FromArgb(27, 35, 48);
        private static readonly Color AcentoSuave = Color.FromArgb(231, 239, 255);
        private static readonly Color FondoFila = Color.FromArgb(247, 248, 250);
        private static readonly Color ColorAviso = Color.FromArgb(154, 91, 0);
        private static readonly Color FondoAviso = Color.FromArgb(255, 241, 214);

        private Button btnAbrirCaja;
        private NumericUpDown numMontoInicial;
        private NumericUpDown numMonedas;
        private Button btnModoMonto;
        private Button btnModoBilletes;
        private Panel cardEfectivo;
        private Panel panelModoMonto;
        private FlowLayoutPanel flowRapidos;
        private FlowLayoutPanel flowBilletes;
        private Panel panelAviso;
        private Label lblAviso;
        private TextBox txtObservacionesApertura;
        private Label lblMontoGrande;
        private Label lblResumenCaja;
        private Label lblResumenCajero;
        private Label lblResumenFecha;
        private Label lblResumenCierre;

        private readonly List<NumericUpDown> _cantidades = new List<NumericUpDown>();
        private readonly List<Label> _subtotales = new List<Label>();

        private DatosApertura _datosApertura;
        private bool _modoBilletes;
        private bool _cargandoApertura;

        // -----------------------------------------------------------------
        // Armado de la pantalla
        // -----------------------------------------------------------------

        private void ConstruirApertura()
        {
            panelCerrada.SuspendLayout();
            panelCerrada.Padding = new Padding(24, 16, 24, 16);

            var encabezado = new Panel { Dock = DockStyle.Top, Height = 66 };
            encabezado.Controls.Add(new Label
            {
                Text = "Abrir caja",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 0)
            });
            encabezado.Controls.Add(new Label
            {
                Text = "Contá el efectivo que hay en el cajón y declaralo. Con ese monto arranca el turno y contra él se calcula el cierre.",
                ForeColor = Gris,
                AutoSize = true,
                Location = new Point(2, 38)
            });

            var izquierda = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(0, 0, 16, 0) };

            // Los paneles se apilan con Dock = Top: el último que se agrega queda arriba.
            izquierda.Controls.Add(ConstruirObservaciones());
            izquierda.Controls.Add(Separador());
            izquierda.Controls.Add(ConstruirAviso());
            izquierda.Controls.Add(Separador());
            izquierda.Controls.Add(ConstruirCardEfectivo());

            panelCerrada.Controls.Add(izquierda);
            panelCerrada.Controls.Add(ConstruirResumen());
            panelCerrada.Controls.Add(encabezado);

            panelCerrada.ResumeLayout(false);
            panelCerrada.PerformLayout();
        }

        private static Panel Separador()
        {
            return new Panel { Dock = DockStyle.Top, Height = 12 };
        }

        private static Label Seccion(string texto, int x, int y)
        {
            return new Label
            {
                Text = texto,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Gris,
                AutoSize = true,
                Location = new Point(x, y)
            };
        }

        private Panel ConstruirCardEfectivo()
        {
            cardEfectivo = new Panel { Dock = DockStyle.Top, BackColor = Color.White, Height = 240 };
            cardEfectivo.Controls.Add(Seccion("EFECTIVO EN EL CAJÓN", 18, 14));

            btnModoMonto = CrearBotonModo("Ingresar monto", 18);
            btnModoBilletes = CrearBotonModo("Contar billetes", 176);
            btnModoMonto.Click += (s, e) => CambiarModo(false);
            btnModoBilletes.Click += (s, e) => CambiarModo(true);
            cardEfectivo.Controls.Add(btnModoMonto);
            cardEfectivo.Controls.Add(btnModoBilletes);

            // Modo "ingresar monto": un campo grande y montos frecuentes.
            panelModoMonto = new Panel
            {
                Location = new Point(18, 88),
                Size = new Size(cardEfectivo.Width - 36, 130),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            numMontoInicial = new NumericUpDown
            {
                DecimalPlaces = 2,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                Maximum = NegocioCaja.MontoMaximo,
                ThousandsSeparator = true,
                TextAlign = HorizontalAlignment.Right,
                Location = new Point(0, 0),
                Size = new Size(panelModoMonto.Width, 45),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            numMontoInicial.ValueChanged += (s, e) => ActualizarApertura();
            numMontoInicial.Enter += (s, e) => numMontoInicial.Select(0, numMontoInicial.Text.Length);
            panelModoMonto.Controls.Add(numMontoInicial);

            flowRapidos = new FlowLayoutPanel
            {
                Location = new Point(0, 60),
                Size = new Size(panelModoMonto.Width, 66),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            panelModoMonto.Controls.Add(flowRapidos);
            cardEfectivo.Controls.Add(panelModoMonto);

            // Modo "contar billetes": una fila por denominación y un campo para monedas.
            flowBilletes = new FlowLayoutPanel
            {
                Location = new Point(18, 88),
                Size = new Size(cardEfectivo.Width - 36, 200),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Visible = false
            };
            foreach (decimal valor in Billetes)
                flowBilletes.Controls.Add(CrearFilaBillete(valor));
            flowBilletes.Controls.Add(CrearFilaMonedas());
            cardEfectivo.Controls.Add(flowBilletes);
            cardEfectivo.Resize += (s, e) => AjustarAlturaDeLaTarjeta();

            return cardEfectivo;
        }

        private Button CrearBotonModo(string texto, int x)
        {
            var b = new Button
            {
                Text = texto,
                Location = new Point(x, 42),
                Size = new Size(148, 34),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private Control CrearFilaBillete(decimal valor)
        {
            Panel fila = CrearFila(valor.ToString("N0"), "$ ");

            var cantidad = new NumericUpDown
            {
                Maximum = 9999,
                TextAlign = HorizontalAlignment.Center,
                Location = new Point(112, 9),
                Size = new Size(70, 26),
                Tag = valor
            };
            cantidad.ValueChanged += (s, e) => ActualizarApertura();
            cantidad.Enter += (s, e) => cantidad.Select(0, cantidad.Text.Length);

            var subtotal = new Label
            {
                Text = "0,00",
                ForeColor = Gris,
                TextAlign = ContentAlignment.MiddleRight,
                Location = new Point(190, 8),
                Size = new Size(84, 28)
            };

            fila.Controls.Add(cantidad);
            fila.Controls.Add(subtotal);
            _cantidades.Add(cantidad);
            _subtotales.Add(subtotal);
            return fila;
        }

        private Control CrearFilaMonedas()
        {
            Panel fila = CrearFila("Monedas y otros", string.Empty);

            numMonedas = new NumericUpDown
            {
                DecimalPlaces = 2,
                Maximum = 1000000,
                ThousandsSeparator = true,
                TextAlign = HorizontalAlignment.Right,
                Location = new Point(144, 9),
                Size = new Size(130, 26)
            };
            numMonedas.ValueChanged += (s, e) => ActualizarApertura();
            numMonedas.Enter += (s, e) => numMonedas.Select(0, numMonedas.Text.Length);
            fila.Controls.Add(numMonedas);
            return fila;
        }

        private static Panel CrearFila(string rotulo, string prefijo)
        {
            var fila = new Panel
            {
                Size = new Size(282, 44),
                Margin = new Padding(0, 0, 10, 8),
                BackColor = FondoFila
            };
            fila.Controls.Add(new Label
            {
                Text = prefijo + rotulo,
                Font = new Font("Consolas", 10.5F, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(10, 8),
                Size = new Size(rotulo.Length > 8 ? 130 : 100, 28)
            });
            return fila;
        }

        private Panel ConstruirAviso()
        {
            panelAviso = new Panel
            {
                Dock = DockStyle.Top,
                Height = 62,
                BackColor = FondoAviso,
                Padding = new Padding(14, 8, 14, 8),
                Visible = false
            };
            lblAviso = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = ColorAviso,
                TextAlign = ContentAlignment.MiddleLeft
            };
            panelAviso.Controls.Add(lblAviso);
            return panelAviso;
        }

        private Panel ConstruirObservaciones()
        {
            var card = new Panel { Dock = DockStyle.Top, BackColor = Color.White, Height = 122 };
            card.Controls.Add(Seccion("OBSERVACIONES (OPCIONAL)", 18, 12));

            txtObservacionesApertura = new TextBox
            {
                Multiline = true,
                MaxLength = NegocioCaja.LargoMaximoObservaciones,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(18, 38),
                Size = new Size(card.Width - 36, 66),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            card.Controls.Add(txtObservacionesApertura);
            return card;
        }

        private Panel ConstruirResumen()
        {
            var panel = new Panel { Dock = DockStyle.Right, Width = 330, BackColor = Color.White };
            panel.Controls.Add(Seccion("RESUMEN DEL TURNO", 18, 14));

            var caja = new Panel
            {
                BackColor = AcentoSuave,
                Location = new Point(18, 44),
                Size = new Size(294, 84),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            caja.Controls.Add(new Label
            {
                Text = "Monto inicial",
                ForeColor = Gris,
                AutoSize = true,
                Location = new Point(12, 10)
            });
            lblMontoGrande = new Label
            {
                Text = "$ 0,00",
                Font = new Font("Consolas", 22F, FontStyle.Bold),
                ForeColor = ColorAcento,
                TextAlign = ContentAlignment.MiddleRight,
                Location = new Point(8, 32),
                Size = new Size(278, 44),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            caja.Controls.Add(lblMontoGrande);
            panel.Controls.Add(caja);

            lblResumenCaja = AgregarDato(panel, "Caja", 146);
            lblResumenCajero = AgregarDato(panel, "Cajero", 176);
            lblResumenFecha = AgregarDato(panel, "Fecha", 206);
            lblResumenCierre = AgregarDato(panel, "Cierre anterior", 236);

            btnAbrirCaja = new Button
            {
                Text = "Abrir caja   (Ctrl+Enter)",
                BackColor = ColorAcento,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(18, 282),
                Size = new Size(294, 48),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            btnAbrirCaja.FlatAppearance.BorderSize = 0;
            btnAbrirCaja.Click += btnAbrirCaja_Click;
            panel.Controls.Add(btnAbrirCaja);

            panel.Controls.Add(new Label
            {
                Text = "Una vez abierta, el monto queda registrado y se puede ver en Auditoría. " +
                       "Para corregirlo hay que cerrar la caja y abrir otra vez.",
                ForeColor = Gris,
                Font = new Font("Segoe UI", 8.5F),
                Location = new Point(18, 342),
                Size = new Size(294, 62),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            });

            return panel;
        }

        private static Label AgregarDato(Panel panel, string rotulo, int y)
        {
            panel.Controls.Add(new Label { Text = rotulo, ForeColor = Gris, AutoSize = true, Location = new Point(18, y) });

            var valor = new Label
            {
                TextAlign = ContentAlignment.TopRight,
                Location = new Point(122, y),
                Size = new Size(190, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            panel.Controls.Add(valor);
            return valor;
        }

        // -----------------------------------------------------------------
        // Datos y comportamiento
        // -----------------------------------------------------------------

        /// <summary>Deja la pantalla lista para un turno nuevo: campos en cero y datos de la caja al día.</summary>
        private void CargarApertura()
        {
            _cargandoApertura = true;
            try
            {
                try
                {
                    _datosApertura = NegocioCaja.PrepararApertura();
                }
                catch (Exception)
                {
                    _datosApertura = null;
                }

                numMontoInicial.Value = 0;
                numMonedas.Value = 0;
                foreach (NumericUpDown n in _cantidades) n.Value = 0;
                txtObservacionesApertura.Clear();

                lblResumenCaja.Text = _datosApertura == null ? "Sin caja configurada" : _datosApertura.NombreCaja;
                lblResumenCajero.Text = SesionActual.Usuario.NombreCompleto;
                lblResumenFecha.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                lblResumenCierre.Text = _datosApertura == null || !_datosApertura.CierreAnterior.HasValue
                    ? "Sin turnos anteriores"
                    : "$ " + _datosApertura.CierreAnterior.Value.ToString(NegocioCaja.FormatoImporte);

                ArmarMontosRapidos();
                CambiarModo(false);
            }
            finally
            {
                _cargandoApertura = false;
            }

            ActualizarApertura();
        }

        private void ArmarMontosRapidos()
        {
            foreach (Control c in flowRapidos.Controls.Cast<Control>().ToList())
            {
                flowRapidos.Controls.Remove(c);
                c.Dispose();
            }

            if (_datosApertura != null && _datosApertura.CierreAnterior.HasValue)
            {
                decimal anterior = _datosApertura.CierreAnterior.Value;
                Button previo = CrearBotonRapido(
                    "Usar el cierre anterior · $ " + anterior.ToString(NegocioCaja.FormatoImporte), anterior);
                previo.ForeColor = ColorAcento;
                previo.BackColor = AcentoSuave;
                previo.FlatAppearance.BorderColor = ColorAcento;
                previo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                flowRapidos.Controls.Add(previo);
            }

            foreach (decimal monto in MontosRapidos)
                flowRapidos.Controls.Add(CrearBotonRapido("$ " + monto.ToString("N0"), monto));
        }

        private Button CrearBotonRapido(string texto, decimal monto)
        {
            var b = new Button
            {
                Text = texto,
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 8, 6),
                Padding = new Padding(6, 1, 6, 1),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderColor = Color.FromArgb(214, 216, 220);
            b.Click += (s, e) =>
            {
                numMontoInicial.Value = Math.Min(monto, numMontoInicial.Maximum);
                numMontoInicial.Focus();
            };
            return b;
        }

        private void CambiarModo(bool billetes)
        {
            // Al volver a "ingresar monto" queda cargado lo que se contó en billetes.
            if (_modoBilletes && !billetes)
                numMontoInicial.Value = Math.Min(SumaBilletes(), numMontoInicial.Maximum);

            _modoBilletes = billetes;
            panelModoMonto.Visible = !billetes;
            flowBilletes.Visible = billetes;

            EstilizarModo(btnModoMonto, !billetes);
            EstilizarModo(btnModoBilletes, billetes);

            AjustarAlturaDeLaTarjeta();
            ActualizarApertura();
        }

        private static void EstilizarModo(Button boton, bool activo)
        {
            boton.BackColor = activo ? AcentoSuave : FondoFila;
            boton.ForeColor = activo ? ColorAcento : Gris;
            boton.Font = new Font("Segoe UI", 9.5F, activo ? FontStyle.Bold : FontStyle.Regular);
        }

        /// <summary>La tarjeta crece lo justo: en modo billetes depende de cuántas columnas entran.</summary>
        private void AjustarAlturaDeLaTarjeta()
        {
            if (cardEfectivo == null) return;

            if (!_modoBilletes)
            {
                cardEfectivo.Height = 240;
                return;
            }

            int columnas = Math.Max(1, flowBilletes.Width / 292);
            int filas = (int)Math.Ceiling((Billetes.Length + 1) / (double)columnas);
            flowBilletes.Height = filas * 52;
            cardEfectivo.Height = 88 + flowBilletes.Height + 14;
        }

        private decimal SumaBilletes()
        {
            decimal total = numMonedas.Value;
            foreach (NumericUpDown n in _cantidades)
                total += (decimal)n.Tag * n.Value;
            return total;
        }

        private decimal TotalDeclarado => _modoBilletes ? SumaBilletes() : numMontoInicial.Value;

        private void ActualizarApertura()
        {
            if (_cargandoApertura || lblMontoGrande == null) return;

            decimal total = TotalDeclarado;
            lblMontoGrande.Text = "$ " + total.ToString(NegocioCaja.FormatoImporte);

            for (int i = 0; i < _cantidades.Count; i++)
            {
                decimal subtotal = (decimal)_cantidades[i].Tag * _cantidades[i].Value;
                _subtotales[i].Text = subtotal.ToString(NegocioCaja.FormatoImporte);
                _subtotales[i].ForeColor = subtotal > 0 ? ColorAcento : Gris;
            }

            // Si no coincide con lo que quedó al cerrar el turno anterior, se avisa y se pide el motivo.
            decimal? anterior = _datosApertura == null ? null : _datosApertura.CierreAnterior;
            bool difiere = anterior.HasValue && anterior.Value != total;

            panelAviso.Visible = difiere;
            if (difiere)
            {
                decimal dif = total - anterior.Value;
                lblAviso.Text = string.Format(
                    "El cierre anterior fue de $ {0} y ahora declarás $ {1} (diferencia {2}$ {3}). " +
                    "Anotá el motivo en las observaciones: queda en la auditoría.",
                    anterior.Value.ToString(NegocioCaja.FormatoImporte),
                    total.ToString(NegocioCaja.FormatoImporte),
                    dif > 0 ? "+" : "−",
                    Math.Abs(dif).ToString(NegocioCaja.FormatoImporte));
            }
        }
    }
}
