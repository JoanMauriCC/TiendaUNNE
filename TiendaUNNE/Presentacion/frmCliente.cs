using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Ventana para elegir el cliente de la venta desde la Caja: se busca por DNI, CUIT, apellido o
    /// nombre; si no está, se carga ahí mismo. También permite volver a "consumidor final".
    /// Solo muestra y recoge datos: qué es válido y cómo se guarda lo resuelve NegocioCliente.
    /// </summary>
    public class frmCliente : Form
    {
        private static readonly Color Gris = Color.FromArgb(120, 125, 135);
        private static readonly Color Acento = Color.FromArgb(47, 111, 237);

        private const int AlturaBusqueda = 256;
        private const int AlturaNuevo = 262;
        private const int AlturaBotones = 58;

        private readonly TextBox txtBuscar = new TextBox();
        private readonly ListBox lstResultados = new ListBox();
        private readonly Label lblEstado = new Label();
        private readonly Button btnNuevo = new Button();
        private readonly Panel panelNuevo = new Panel();
        private readonly Button btnUsar = new Button();
        private readonly Button btnConsumidor = new Button();
        private readonly Button btnCancelar = new Button();

        private readonly TextBox txtDni = new TextBox();
        private readonly TextBox txtNombre = new TextBox();
        private readonly TextBox txtApellido = new TextBox();
        private readonly TextBox txtEmail = new TextBox();
        private readonly TextBox txtTelefono = new TextBox();
        private readonly TextBox txtCuit = new TextBox();
        private readonly TextBox txtRazonSocial = new TextBox();
        private readonly ComboBox cboIva = new ComboBox();

        private bool _modoNuevo;

        /// <summary>El cliente elegido o recién cargado. Null si se eligió consumidor final.</summary>
        public ClienteItem ClienteElegido { get; private set; }

        public frmCliente()
        {
            Text = "Cliente de la venta";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Font = new Font("Segoe UI", 9F);
            KeyPreview = true;

            ConstruirBusqueda();
            ConstruirNuevo();
            ConstruirBotones();

            MostrarModoNuevo(false);
        }

        // -----------------------------------------------------------------
        // Armado de la ventana
        // -----------------------------------------------------------------

        private void ConstruirBusqueda()
        {
            Controls.Add(new Label
            {
                Text = "Buscar cliente por DNI, CUIT, apellido o nombre",
                AutoSize = true,
                Location = new Point(16, 16)
            });

            txtBuscar.Font = new Font("Segoe UI", 11F);
            txtBuscar.Location = new Point(16, 38);
            txtBuscar.Size = new Size(508, 27);
            txtBuscar.MaxLength = 100;
            txtBuscar.TextChanged += (s, e) => Buscar();
            txtBuscar.KeyDown += txtBuscar_KeyDown;
            Controls.Add(txtBuscar);

            lstResultados.Location = new Point(16, 74);
            lstResultados.Size = new Size(508, 140);
            lstResultados.Font = new Font("Segoe UI", 10F);
            lstResultados.IntegralHeight = false;
            lstResultados.SelectedIndexChanged += (s, e) => ActualizarBotones();
            lstResultados.DoubleClick += (s, e) => UsarSeleccionado();
            Controls.Add(lstResultados);

            lblEstado.Location = new Point(16, 224);
            lblEstado.Size = new Size(330, 22);
            lblEstado.ForeColor = Gris;
            lblEstado.Text = "Escribí para buscar.";
            Controls.Add(lblEstado);

            btnNuevo.Text = "+ Cliente nuevo";
            btnNuevo.Location = new Point(384, 220);
            btnNuevo.Size = new Size(140, 28);
            btnNuevo.Click += (s, e) => MostrarModoNuevo(true);
            Controls.Add(btnNuevo);
        }

        private void ConstruirNuevo()
        {
            panelNuevo.Location = new Point(0, AlturaBusqueda);
            panelNuevo.Size = new Size(540, AlturaNuevo);
            panelNuevo.Visible = false;
            Controls.Add(panelNuevo);

            panelNuevo.Controls.Add(new Label
            {
                Text = "CLIENTE NUEVO",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Gris,
                AutoSize = true,
                Location = new Point(16, 4)
            });

            Campo("DNI *", txtDni, 16, 24, 8);
            txtDni.KeyPress += (s, e) => e.Handled = !Validaciones.EsCaracterDniValido(e.KeyChar);

            cboIva.DropDownStyle = ComboBoxStyle.DropDownList;
            cboIva.Items.AddRange(NegocioCliente.CondicionesIva);
            cboIva.SelectedItem = NegocioCliente.ConsumidorFinal;
            Etiqueta("Condición frente al IVA", 280, 24);
            cboIva.Location = new Point(280, 44);
            cboIva.Size = new Size(244, 23);
            panelNuevo.Controls.Add(cboIva);

            Campo("Nombre *", txtNombre, 16, 80, NegocioCliente.LargoMaximoNombre);
            txtNombre.KeyPress += (s, e) => e.Handled = !Validaciones.EsCaracterNombrePersonaValido(e.KeyChar);
            Campo("Apellido *", txtApellido, 280, 80, NegocioCliente.LargoMaximoNombre);
            txtApellido.KeyPress += (s, e) => e.Handled = !Validaciones.EsCaracterNombrePersonaValido(e.KeyChar);

            Campo("Email", txtEmail, 16, 136, NegocioCliente.LargoMaximoEmail);
            txtEmail.KeyPress += (s, e) => e.Handled = !Validaciones.EsCaracterEmailValido(e.KeyChar);
            Campo("Teléfono", txtTelefono, 280, 136, NegocioCliente.LargoMaximoTelefono);
            txtTelefono.KeyPress += (s, e) => e.Handled = !Validaciones.EsCaracterTelefonoValido(e.KeyChar);

            Campo("CUIT (solo números)", txtCuit, 16, 192, NegocioCliente.LargoCuit);
            txtCuit.KeyPress += (s, e) => e.Handled = !Validaciones.EsCaracterNumericoValido(e.KeyChar);
            Campo("Razón social", txtRazonSocial, 280, 192, NegocioCliente.LargoMaximoRazonSocial);

            panelNuevo.Controls.Add(new Label
            {
                Text = "* Obligatorio. Para facturar con Factura A hace falta el CUIT.",
                ForeColor = Gris,
                AutoSize = true,
                Location = new Point(16, 240)
            });
        }

        private void ConstruirBotones()
        {
            var barra = new Panel { Dock = DockStyle.Bottom, Height = AlturaBotones };
            barra.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(223, 227, 234) });

            btnUsar.Location = new Point(16, 12);
            btnUsar.Size = new Size(170, 34);
            btnUsar.FlatStyle = FlatStyle.Flat;
            btnUsar.FlatAppearance.BorderSize = 0;
            btnUsar.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnUsar.Click += (s, e) => { if (_modoNuevo) GuardarNuevo(); else UsarSeleccionado(); };
            btnUsar.EnabledChanged += (s, e) => PintarBotonUsar();
            barra.Controls.Add(btnUsar);

            btnConsumidor.Text = "Consumidor final";
            btnConsumidor.Location = new Point(194, 12);
            btnConsumidor.Size = new Size(150, 34);
            btnConsumidor.Click += (s, e) => { ClienteElegido = null; DialogResult = DialogResult.OK; Close(); };
            barra.Controls.Add(btnConsumidor);

            btnCancelar.Text = "Cancelar";
            btnCancelar.Location = new Point(424, 12);
            btnCancelar.Size = new Size(100, 34);
            btnCancelar.DialogResult = DialogResult.Cancel;
            barra.Controls.Add(btnCancelar);

            CancelButton = btnCancelar;
            Controls.Add(barra);
        }

        private void Etiqueta(string texto, int x, int y)
        {
            panelNuevo.Controls.Add(new Label { Text = texto, AutoSize = true, Location = new Point(x, y) });
        }

        private void Campo(string rotulo, TextBox caja, int x, int y, int maximo)
        {
            Etiqueta(rotulo, x, y);
            caja.Location = new Point(x, y + 20);
            caja.Size = new Size(244, 23);
            caja.MaxLength = maximo;
            panelNuevo.Controls.Add(caja);
        }

        // -----------------------------------------------------------------
        // Comportamiento
        // -----------------------------------------------------------------

        private void MostrarModoNuevo(bool nuevo)
        {
            _modoNuevo = nuevo;

            panelNuevo.Visible = nuevo;
            lstResultados.Enabled = !nuevo;
            txtBuscar.Enabled = !nuevo;
            btnNuevo.Visible = !nuevo;

            int alto = AlturaBusqueda + (nuevo ? AlturaNuevo : 0) + AlturaBotones;
            ClientSize = new Size(540, alto);

            if (nuevo)
            {
                // Si lo buscado parece un DNI o un CUIT, se adelanta en el formulario.
                string texto = txtBuscar.Text.Trim();
                if (Validaciones.EsDniValido(texto)) txtDni.Text = texto;
                else if (texto.Length == NegocioCliente.LargoCuit && NegocioCliente.EsCuitValido(texto))
                    txtCuit.Text = texto;

                (txtDni.Text.Length == 0 ? txtDni : txtNombre).Focus();
            }
            else
            {
                txtBuscar.Focus();
            }

            btnUsar.Text = nuevo ? "Guardar y usar" : "Usar este cliente";
            ActualizarBotones();
        }

        private void Buscar()
        {
            string texto = txtBuscar.Text.Trim();
            lstResultados.Items.Clear();

            if (texto.Length == 0)
            {
                lblEstado.Text = "Escribí para buscar.";
                ActualizarBotones();
                return;
            }

            try
            {
                List<ClienteItem> clientes = NegocioCliente.Buscar(texto);
                foreach (ClienteItem c in clientes)
                    lstResultados.Items.Add(c);

                lblEstado.Text = clientes.Count == 0
                    ? "No hay clientes con eso. Podés cargarlo."
                    : clientes.Count + (clientes.Count == 1 ? " cliente" : " clientes");

                if (clientes.Count > 0)
                    lstResultados.SelectedIndex = 0;
            }
            catch (Exception)
            {
                lblEstado.Text = "No se pudo buscar.";
            }

            ActualizarBotones();
        }

        private void ActualizarBotones()
        {
            btnUsar.Enabled = _modoNuevo || lstResultados.SelectedItem != null;
            PintarBotonUsar();
        }

        // Un botón plano deshabilitado no se ve "apagado" por sí solo: se le cambia el color.
        private void PintarBotonUsar()
        {
            btnUsar.BackColor = btnUsar.Enabled ? Acento : Color.FromArgb(214, 216, 220);
            btnUsar.ForeColor = btnUsar.Enabled ? Color.White : Color.FromArgb(120, 125, 135);
        }

        private void txtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && lstResultados.Items.Count > 0)
            {
                lstResultados.Focus();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                if (lstResultados.SelectedItem != null)
                    UsarSeleccionado();
                else if (Validaciones.EsDniValido(txtBuscar.Text.Trim()))
                    MostrarModoNuevo(true);
            }
        }

        private void UsarSeleccionado()
        {
            var cliente = lstResultados.SelectedItem as ClienteItem;
            if (cliente == null) return;

            ClienteElegido = cliente;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void GuardarNuevo()
        {
            var modelo = new ClienteEditModel
            {
                Dni = txtDni.Text,
                Nombre = txtNombre.Text,
                Apellido = txtApellido.Text,
                Email = txtEmail.Text,
                Telefono = txtTelefono.Text,
                Cuit = txtCuit.Text,
                RazonSocial = txtRazonSocial.Text,
                CondicionIva = Convert.ToString(cboIva.SelectedItem)
            };

            try
            {
                Cursor = Cursors.WaitCursor;

                bool creado;
                ClienteElegido = NegocioCliente.ObtenerOCrear(modelo, SesionActual.Usuario.IdUsuario, out creado);

                if (!creado)
                    MessageBox.Show("Ese DNI ya estaba cargado como cliente: se usa el que existe.",
                        "Cliente", MessageBoxButtons.OK, MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo guardar el cliente",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo guardar el cliente.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // Ctrl+Enter guarda desde cualquier campo del formulario nuevo.
            if (_modoNuevo && e.Control && e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                GuardarNuevo();
                return;
            }
            base.OnKeyDown(e);
        }
    }
}
