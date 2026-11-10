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
    public partial class ucAsistente : UserControl, IPantallaConGuia
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

        /// <summary>Lo que se habló en esta sesión: es la memoria que se le manda en cada pregunta.</summary>
        private readonly List<MensajeChat> _conversacion = new List<MensajeChat>();
        private Control _pensando;
        private bool _configurado;
        private readonly ToolTip _tips = new ToolTip();

        public ucAsistente()
        {
            InitializeComponent();
        }

        private void ucAsistente_Load(object sender, EventArgs e)
        {
            _configurado = NegocioAsistente.EstaConfigurado();
            CrearSugerencias();
            Saludar();
            Ocupado(false);
            txtPregunta.Focus();
        }

        private void Saludar()
        {
            if (_configurado)
                AgregarBurbuja(TipoBurbuja.Asistente,
                    "¡Hola! Soy tu asistente de ventas. Conozco las ventas y el catálogo del local: preguntame " +
                    "qué promociones armar, qué conviene reponer o sumar para la temporada, o cómo mover lo que " +
                    "no se vende. Abajo tenés algunas preguntas para empezar.");
            else
                AgregarBurbuja(TipoBurbuja.Aviso, NegocioAsistente.AyudaConfiguracion);
        }

        /// <summary>Un botón por cada pregunta sugerida (las define NegocioAsistente): tocarlo la manda.</summary>
        private void CrearSugerencias()
        {
            foreach (PreguntaSugerida sugerida in NegocioAsistente.PreguntasSugeridas())
            {
                string pregunta = sugerida.Pregunta;
                var boton = new Button
                {
                    Text = sugerida.Titulo,
                    AutoSize = true,
                    Height = 30,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    ForeColor = ColorUsuario,
                    Cursor = Cursors.Hand,
                    Margin = new Padding(3, 0, 3, 6),
                    UseMnemonic = false,
                    UseVisualStyleBackColor = false
                };
                boton.FlatAppearance.BorderColor = ColorBorde;
                _tips.SetToolTip(boton, pregunta);
                boton.Click += (s, e) =>
                {
                    txtPregunta.Text = pregunta;
                    btnEnviar.PerformClick();
                };
                flowSugerencias.Controls.Add(boton);
            }
        }

        /// <summary>Borra el chat y la memoria: la próxima pregunta arranca de cero.</summary>
        private void btnNueva_Click(object sender, EventArgs e)
        {
            if (_pensando != null) return;   // no se corta una respuesta en camino

            flowConversacion.SuspendLayout();
            while (flowConversacion.Controls.Count > 0)
                flowConversacion.Controls[0].Dispose();
            flowConversacion.ResumeLayout();

            _conversacion.Clear();
            Saludar();
            txtPregunta.Clear();
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

        /// <summary>Mientras espera la respuesta (o si falta la clave) no se puede preguntar otra cosa.</summary>
        private void Ocupado(bool ocupado)
        {
            bool habilitado = _configurado && !ocupado;
            txtPregunta.Enabled = habilitado;
            btnEnviar.Enabled = habilitado;
            btnNueva.Enabled = !ocupado;
            foreach (Control sugerencia in flowSugerencias.Controls)
                sugerencia.Enabled = habilitado;
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

            // Clic derecho sobre una respuesta: copiarla (para pegarla en un mensaje, un cartel, etc.).
            if (tipo == TipoBurbuja.Asistente)
            {
                var menu = new ContextMenuStrip();
                menu.Items.Add("Copiar respuesta", null, (s, e) => Clipboard.SetText(texto));
                burbuja.ContextMenuStrip = menu;
                etiqueta.ContextMenuStrip = menu;
                burbuja.Disposed += (s, e) => menu.Dispose();
            }
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

        // -----------------------------------------------------------------
        // Ayuda: el control de cada parte que señala la guía (NegocioGuia)
        // -----------------------------------------------------------------

        public Control Ancla(string clave)
        {
            switch (clave)
            {
                case AnclaGuia.AsistenteConversacion: return flowConversacion;
                case AnclaGuia.AsistenteSugerencias: return flowSugerencias;
                case AnclaGuia.AsistentePregunta: return txtPregunta;
                case AnclaGuia.AsistenteNueva: return btnNueva;
                default: return null;
            }
        }

    }
}
