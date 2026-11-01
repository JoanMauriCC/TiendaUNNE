using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Lista de coincidencias que aparece debajo de un campo mientras se escribe en él: si lo que
    /// se tipea se parece a algo que ya existe (un DNI, un apellido, un nombre de producto), se
    /// ofrece para cargarlo en el formulario y no crear un duplicado.
    ///
    ///   - Se elige con el mouse, o con la flecha hacia abajo y Enter.
    ///   - Esc la cierra sin tocar lo escrito.
    ///   - Solo aparece mientras el formulario está en modo alta: al editar un registro, cambiar
    ///     un dato no tiene que saltar a otro ni pisar lo que se estaba editando.
    ///
    /// Qué se busca y cómo se rotula cada coincidencia lo decide Negocio (la función que llega
    /// por parámetro); esta clase solo dibuja la lista y avisa qué se eligió.
    /// </summary>
    public sealed class SugerenciasCampo
    {
        private const int FilasVisibles = 6;
        private const int AnchoMinimo = 320;

        private readonly Control _contenedor;
        private readonly TextBox _campo;
        private readonly Func<string, List<SugerenciaItem>> _buscar;
        private readonly Action<SugerenciaItem> _alElegir;
        private readonly Func<bool> _activa;
        private readonly ListBox _lista;

        private bool _eligiendo;

        /// <param name="contenedor">La pantalla donde vive la lista (no el panel del formulario, que la recortaría).</param>
        /// <param name="campo">El campo en el que se escribe.</param>
        /// <param name="buscar">Devuelve las coincidencias para lo escrito (viene de Negocio).</param>
        /// <param name="alElegir">Qué hacer con la coincidencia elegida (cargarla en el formulario).</param>
        /// <param name="activa">Si devuelve false, no se ofrece nada (por ejemplo, al editar).</param>
        public SugerenciasCampo(Control contenedor, TextBox campo,
                                Func<string, List<SugerenciaItem>> buscar,
                                Action<SugerenciaItem> alElegir, Func<bool> activa)
        {
            _contenedor = contenedor;
            _campo = campo;
            _buscar = buscar;
            _alElegir = alElegir;
            _activa = activa;

            _lista = new ListBox
            {
                Visible = false,
                BorderStyle = BorderStyle.FixedSingle,
                IntegralHeight = false,
                Font = new Font("Segoe UI", 9.5F),
                TabStop = false
            };
            _contenedor.Controls.Add(_lista);

            _campo.TextChanged += (s, e) => Actualizar();
            _campo.KeyDown += Campo_KeyDown;
            _campo.Leave += (s, e) => OcultarSiSePerdioElFoco();

            _lista.KeyDown += Lista_KeyDown;
            _lista.MouseClick += Lista_MouseClick;
            _lista.Leave += (s, e) => OcultarSiSePerdioElFoco();
        }

        public bool Visible => _lista.Visible;

        public void Ocultar()
        {
            _lista.Visible = false;
        }

        // -----------------------------------------------------------------
        // Mostrar y ocultar
        // -----------------------------------------------------------------

        private void Actualizar()
        {
            if (_eligiendo || !_campo.Focused || !_activa())
            {
                Ocultar();
                return;
            }

            List<SugerenciaItem> coincidencias;
            try
            {
                coincidencias = _buscar(_campo.Text);
            }
            catch (Exception)
            {
                // La sugerencia es una ayuda: si falla, el formulario sigue funcionando igual.
                Ocultar();
                return;
            }

            if (coincidencias == null || coincidencias.Count == 0)
            {
                Ocultar();
                return;
            }

            _lista.BeginUpdate();
            _lista.Items.Clear();
            foreach (SugerenciaItem s in coincidencias)
                _lista.Items.Add(s);
            _lista.EndUpdate();

            // Se ubica justo debajo del campo, en coordenadas de la pantalla que la contiene.
            Point abajo = _contenedor.PointToClient(
                _campo.Parent.PointToScreen(new Point(_campo.Left, _campo.Bottom)));

            int alto = Math.Min(coincidencias.Count, FilasVisibles) * _lista.ItemHeight + 4;
            _lista.SetBounds(abajo.X, abajo.Y, Math.Max(_campo.Width, AnchoMinimo), alto);
            _lista.BringToFront();
            _lista.Visible = true;
        }

        private void OcultarSiSePerdioElFoco()
        {
            // Al hacer clic en la lista el campo pierde el foco un instante antes de que lo tome ella:
            // se espera a que se asiente para no cerrarla justo cuando se la quiere usar.
            _contenedor.BeginInvoke(new Action(() =>
            {
                if (!_campo.Focused && !_lista.Focused)
                    Ocultar();
            }));
        }

        // -----------------------------------------------------------------
        // Teclado y mouse
        // -----------------------------------------------------------------

        private void Campo_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_lista.Visible) return;

            if (e.KeyCode == Keys.Down)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                _lista.SelectedIndex = 0;
                _lista.Focus();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                Ocultar();
            }
        }

        private void Lista_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                Elegir(_lista.SelectedItem as SugerenciaItem);
            }
            else if (e.KeyCode == Keys.Escape || (e.KeyCode == Keys.Up && _lista.SelectedIndex == 0))
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                Ocultar();
                _campo.Focus();
            }
        }

        private void Lista_MouseClick(object sender, MouseEventArgs e)
        {
            int indice = _lista.IndexFromPoint(e.Location);
            if (indice >= 0)
                Elegir(_lista.Items[indice] as SugerenciaItem);
        }

        private void Elegir(SugerenciaItem elegida)
        {
            if (elegida == null) return;

            _eligiendo = true;
            try
            {
                Ocultar();
                _alElegir(elegida);
            }
            finally
            {
                _eligiendo = false;
            }

            _campo.Focus();
        }
    }
}
