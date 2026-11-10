using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Sección Asistente (solo administrador): un chat con un asistente de ventas que usa Google
    /// Gemini. La pantalla solo muestra la conversación y manda la pregunta; qué se le pregunta a
    /// Gemini y cómo se interpretan los errores lo decide NegocioAsistente.
    /// </summary>
    public partial class ucAsistente : UserControl
    {
        private enum TipoBurbuja { Usuario, Asistente, Aviso, Error }

        private static readonly Color ColorUsuario = Color.FromArgb(59, 130, 246);
        private static readonly Color ColorTexto = Color.FromArgb(45, 48, 54);
        private static readonly Color ColorBorde = Color.FromArgb(214, 216, 220);
        private static readonly Color FondoAviso = Color.FromArgb(254, 243, 199);
        private static readonly Color TextoAviso = Color.FromArgb(120, 53, 15);
        private static readonly Color FondoError = Color.FromArgb(253, 228, 228);
        private static readonly Color TextoError = Color.FromArgb(163, 45, 45);

        private const int SeparacionBurbujas = 10;
        private const double AnchoMaximoBurbuja = 0.75;   // proporción del ancho del chat

        /// <summary>Lo que se habló en esta sesión: se manda entero en cada pregunta para que tenga contexto.</summary>
        private readonly List<MensajeChat> _conversacion = new List<MensajeChat>();
        private Control _pensando;

        public ucAsistente()
        {
            InitializeComponent();
        }

        private void ucAsistente_Load(object sender, EventArgs e)
        {
            if (NegocioAsistente.EstaConfigurado())
            {
                AgregarBurbuja(TipoBurbuja.Asistente,
                    "¡Hola! Soy tu asistente de ventas. Preguntame lo que quieras: qué promociones armar, " +
                    "qué productos conviene sumar para la temporada o cómo mover lo que no se vende.");
            }
            else
            {
                AgregarBurbuja(TipoBurbuja.Aviso, NegocioAsistente.AyudaConfiguracion);
                txtPregunta.Enabled = false;
                btnEnviar.Enabled = false;
            }

            txtPregunta.Focus();
        }

        // -----------------------------------------------------------------
        // Enviar
        // -----------------------------------------------------------------

        private void txtPregunta_KeyDown(object sender, KeyEventArgs e)
        {
            // Enter manda; Shift+Enter deja escribir una línea nueva.
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                btnEnviar.PerformClick();
            }
        }

        private async void btnEnviar_Click(object sender, EventArgs e)
        {
            string pregunta = txtPregunta.Text.Trim();
            if (pregunta.Length == 0 || !btnEnviar.Enabled) return;

            AgregarBurbuja(TipoBurbuja.Usuario, pregunta);
            txtPregunta.Clear();
            Ocupado(true);

            try
            {
                string respuesta = await NegocioAsistente.PreguntarAsync(_conversacion, pregunta);
                if (IsDisposed) return;   // se cambió de sección mientras esperaba

                _conversacion.Add(new MensajeChat { Rol = RolMensaje.Usuario, Texto = pregunta });
                _conversacion.Add(new MensajeChat { Rol = RolMensaje.Asistente, Texto = respuesta });
                QuitarPensando();
                AgregarBurbuja(TipoBurbuja.Asistente, respuesta);
            }
            catch (ReglaNegocioException ex)
            {
                if (IsDisposed) return;
                QuitarPensando();
                AgregarBurbuja(TipoBurbuja.Error, ex.Message);
            }
            catch (Exception)
            {
                if (IsDisposed) return;
                QuitarPensando();
                AgregarBurbuja(TipoBurbuja.Error, "No se pudo consultar al asistente.");
            }
            finally
            {
                if (!IsDisposed)
                {
                    Ocupado(false);
                    txtPregunta.Focus();
                }
            }
        }

        private void Ocupado(bool ocupado)
        {
            txtPregunta.Enabled = !ocupado;
            btnEnviar.Enabled = !ocupado;
            Cursor = ocupado ? Cursors.AppStarting : Cursors.Default;

            if (ocupado)
                _pensando = AgregarBurbuja(TipoBurbuja.Asistente, "Pensando…");
        }

        private void QuitarPensando()
        {
            if (_pensando == null) return;
            flowConversacion.Controls.Remove(_pensando);
            _pensando.Dispose();
            _pensando = null;
        }

        // -----------------------------------------------------------------
        // Burbujas del chat
        // -----------------------------------------------------------------

        /// <summary>
        /// Agrega un mensaje como burbuja: las del usuario a la derecha y en azul, las del asistente a
        /// la izquierda. Cada burbuja va en una fila del ancho del chat, así se puede alinear.
        /// </summary>
        private Control AgregarBurbuja(TipoBurbuja tipo, string texto)
        {
            Color fondo, letra;
            switch (tipo)
            {
                case TipoBurbuja.Usuario: fondo = ColorUsuario; letra = Color.White; break;
                case TipoBurbuja.Aviso: fondo = FondoAviso; letra = TextoAviso; break;
                case TipoBurbuja.Error: fondo = FondoError; letra = TextoError; break;
                default: fondo = Color.White; letra = ColorTexto; break;
            }

            var etiqueta = new Label
            {
                AutoSize = true,
                Text = texto,
                ForeColor = letra,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10f),
                UseMnemonic = false
            };

            var burbuja = new Panel { AutoSize = true, BackColor = fondo, Padding = new Padding(12, 9, 12, 9) };
            etiqueta.Location = new Point(burbuja.Padding.Left, burbuja.Padding.Top);
            burbuja.Controls.Add(etiqueta);
            if (tipo == TipoBurbuja.Asistente)
                burbuja.Paint += (s, e) =>
                {
                    using (var borde = new Pen(ColorBorde))
                        e.Graphics.DrawRectangle(borde, 0, 0, burbuja.Width - 1, burbuja.Height - 1);
                };

            var fila = new Panel { Margin = new Padding(0, 0, 0, SeparacionBurbujas), Tag = tipo };
            fila.Controls.Add(burbuja);

            flowConversacion.Controls.Add(fila);
            AcomodarFila(fila);
            flowConversacion.ScrollControlIntoView(fila);
            return fila;
        }

        private void AcomodarFila(Control fila)
        {
            int ancho = Math.Max(200, flowConversacion.ClientSize.Width - flowConversacion.Padding.Horizontal
                                      - SystemInformation.VerticalScrollBarWidth);
            Control burbuja = fila.Controls[0];
            Control etiqueta = burbuja.Controls[0];

            etiqueta.MaximumSize = new Size((int)(ancho * AnchoMaximoBurbuja), 0);
            burbuja.PerformLayout();
            fila.Width = ancho;
            fila.Height = burbuja.Height;

            bool deUsuario = (TipoBurbuja)fila.Tag == TipoBurbuja.Usuario;
            burbuja.Location = new Point(deUsuario ? ancho - burbuja.Width : 0, 0);
        }

        private void flowConversacion_Resize(object sender, EventArgs e)
        {
            flowConversacion.SuspendLayout();
            foreach (Control fila in flowConversacion.Controls)
                AcomodarFila(fila);
            flowConversacion.ResumeLayout();
        }
    }
}
