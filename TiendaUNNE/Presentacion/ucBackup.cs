using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Sección de copias de seguridad (solo Administrador): lista las copias de la carpeta y
    /// permite crear una, restaurar la elegida o guardar una copia en otro lugar.
    /// Solo muestra y pide confirmaciones: cuándo se puede hacer algo, cómo se nombran los
    /// archivos y qué se valida lo resuelve NegocioBackup. La pantalla nunca maneja rutas:
    /// le pasa a Negocio solo el nombre de la copia elegida.
    /// </summary>
    public partial class ucBackup : UserControl
    {
        private static readonly Color ColorPrevio = Color.FromArgb(255, 243, 205);

        private const int Margen = 12;
        private const int Gap = 16;

        public ucBackup()
        {
            InitializeComponent();
            panelResumen.Resize += (s, e) => ReubicarResumen();
        }

        private void ucBackup_Load(object sender, EventArgs e)
        {
            CargarListado();
            ReubicarResumen();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F5)
            {
                CargarListado();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>Reparte el ancho del panel de resumen en 3 columnas parejas.</summary>
        private void ReubicarResumen()
        {
            int ancho = panelResumen.ClientSize.Width;
            if (ancho <= 0) return;

            int col = Math.Max(120, (ancho - 2 * Margen - 2 * Gap) / 3);

            Ubicar(lblCarpetaTitulo, lblCarpeta, Margen, col);
            Ubicar(lblCantidadTitulo, lblCantidad, Margen + col + Gap, col);
            Ubicar(lblUltimaTitulo, lblUltima, Margen + 2 * (col + Gap), col);
        }

        private static void Ubicar(Label titulo, Label valor, int x, int ancho)
        {
            titulo.Left = x;
            valor.Left = x;
            valor.Width = ancho;
        }

        // -----------------------------------------------------------------
        // Listado
        // -----------------------------------------------------------------

        private void CargarListado()
        {
            string nombreElegido = NombreSeleccionado();

            try
            {
                Cursor = Cursors.WaitCursor;

                DataTable dt = NegocioBackup.Listar();
                dgvBackups.DataSource = dt;
                AplicarFormato();

                lblCarpeta.Text = NegocioBackup.ObtenerCarpeta();
                lblCantidad.Text = dt.Rows.Count.ToString();
                lblUltima.Text = NegocioBackup.TextoUltimaCopia(dt);

                if (nombreElegido == null || !SeleccionarPorNombre(nombreElegido))
                    SeleccionarPorNombre(null);
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Backup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo leer el listado de copias.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            ActualizarBotones();
        }

        private void AplicarFormato()
        {
            SetColumna("Archivo", "Archivo", 220);
            SetColumna("FechaHora", "Fecha y hora", 100);
            SetColumna("Tamano", "Tamaño", 60);
            SetColumna("Tipo", "Tipo", 90);

            if (dgvBackups.Columns.Contains("FechaHora"))
                dgvBackups.Columns["FechaHora"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";

            if (dgvBackups.Columns.Contains("Tamano"))
                dgvBackups.Columns["Tamano"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        private void SetColumna(string columna, string encabezado, float peso)
        {
            if (!dgvBackups.Columns.Contains(columna)) return;
            dgvBackups.Columns[columna].HeaderText = encabezado;
            dgvBackups.Columns[columna].FillWeight = peso;
        }

        /// <summary>Las copias "antes de restaurar" se pintan de amarillo para distinguirlas de las manuales.</summary>
        private void dgvBackups_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow fila = dgvBackups.Rows[e.RowIndex];
            if (fila.Selected) return;

            var drv = fila.DataBoundItem as DataRowView;
            if (drv != null && Convert.ToString(drv["Tipo"]) == NegocioBackup.TipoPrevio)
                e.CellStyle.BackColor = ColorPrevio;
        }

        // -----------------------------------------------------------------
        // Selección
        // -----------------------------------------------------------------

        private DataRowView FilaElegida()
        {
            DataGridViewRow fila = dgvBackups.CurrentRow;
            if (fila == null && dgvBackups.SelectedRows.Count > 0)
                fila = dgvBackups.SelectedRows[0];
            return fila == null ? null : fila.DataBoundItem as DataRowView;
        }

        private string NombreSeleccionado()
        {
            DataRowView fila = FilaElegida();
            return fila == null ? null : Convert.ToString(fila["Archivo"]);
        }

        /// <summary>Selecciona la copia con ese nombre; con null, la primera (la más nueva).</summary>
        private bool SeleccionarPorNombre(string nombre)
        {
            if (dgvBackups.Rows.Count == 0 || dgvBackups.Columns.Count == 0)
                return false;

            foreach (DataGridViewRow fila in dgvBackups.Rows)
            {
                var drv = fila.DataBoundItem as DataRowView;
                if (drv == null) continue;

                if (nombre == null || Convert.ToString(drv["Archivo"]) == nombre)
                {
                    dgvBackups.ClearSelection();
                    fila.Selected = true;
                    dgvBackups.CurrentCell = fila.Cells[0];
                    return true;
                }
            }
            return false;
        }

        private void dgvBackups_SelectionChanged(object sender, EventArgs e)
        {
            ActualizarBotones();
        }

        private void ActualizarBotones()
        {
            string nombre = NombreSeleccionado();
            bool hay = nombre != null;

            tarjetaRestaurar.Enabled = hay;
            tarjetaCopiar.Enabled = hay;
            tarjetaRestaurar.Descripcion = hay ? nombre : "Elegí una copia de la lista";
            tarjetaRestaurar.Invalidate();
        }

        // -----------------------------------------------------------------
        // Acciones
        // -----------------------------------------------------------------

        private void tarjetaCrear_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                string nombre = NegocioBackup.Crear();

                CargarListado();
                SeleccionarPorNombre(nombre);

                MessageBox.Show("Copia creada:\n" + nombre, "Backup",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo crear la copia",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo crear la copia.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void tarjetaRestaurar_Click(object sender, EventArgs e)
        {
            DataRowView fila = FilaElegida();
            if (fila == null) return;

            string nombre = Convert.ToString(fila["Archivo"]);
            string fecha = ((DateTime)fila["FechaHora"]).ToString("dd/MM/yyyy HH:mm");
            string tamanio = Convert.ToString(fila["Tamano"]);

            string confirmacion;
            LlavesRestauracion llaves;
            using (var dialogo = new frmConfirmarRestauracion(nombre, fecha, tamanio))
            {
                if (dialogo.ShowDialog(this) != DialogResult.OK)
                    return;
                confirmacion = dialogo.Confirmacion;
                llaves = dialogo.Llaves;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                NegocioBackup.Restaurar(nombre, confirmacion, llaves);
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo restaurar",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo restaurar la copia.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            // La base cambió por debajo de la aplicación (el usuario con sesión abierta puede ni
            // existir en esa copia): lo único seguro es volver a empezar desde el login.
            MessageBox.Show("La copia se restauró correctamente. La aplicación se va a reiniciar.",
                "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);

            NegocioAutenticacion.CerrarSesion();
            Application.Restart();
        }

        private void tarjetaCopiar_Click(object sender, EventArgs e)
        {
            string nombre = NombreSeleccionado();
            if (nombre == null) return;

            using (var dialogo = new SaveFileDialog())
            {
                dialogo.Title = "Guardar copia en…";
                dialogo.Filter = "Copia de seguridad (*.bak)|*.bak";
                dialogo.FileName = nombre;
                dialogo.OverwritePrompt = true;

                if (dialogo.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    Cursor = Cursors.WaitCursor;
                    NegocioBackup.CopiarA(nombre, dialogo.FileName);

                    MessageBox.Show("La copia se guardó en:\n" + dialogo.FileName, "Backup",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (ReglaNegocioException ex)
                {
                    MessageBox.Show(ex.Message, "No se pudo guardar la copia",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception)
                {
                    MessageBox.Show("No se pudo guardar la copia.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private void tarjetaActualizar_Click(object sender, EventArgs e)
        {
            CargarListado();
        }
    }
}
