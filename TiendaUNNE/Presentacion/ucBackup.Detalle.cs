using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Panel de abajo de la pantalla de Backup: los datos de la copia que está elegida en la
    /// lista de arriba. Solo muestra lo que arma NegocioBackup.ObtenerDetalle y pide, si la
    /// persona lo quiere, la verificación de integridad.
    /// </summary>
    public partial class ucBackup
    {
        private static readonly Color ColorGrisDetalle = Color.FromArgb(120, 125, 135);
        private static readonly Color ColorVerde = Color.FromArgb(21, 127, 75);
        private static readonly Color ColorRojo = Color.FromArgb(180, 35, 24);

        private Label lblDetalleTitulo;
        private Label lblSinSeleccion;
        private TableLayoutPanel tablaDetalle;
        private Button btnVerificar;
        private Label lblVerificacion;

        private Label vArchivo, vTipo, vCreada, vTamano, vBase, vServidor, vUsuarioSql, vVersion, vIntegridad, vUbicacion;

        // Leer el encabezado es una consulta a SQL Server: se recuerda por copia mientras no se actualice el listado.
        private readonly Dictionary<string, DetalleBackup> _detalles = new Dictionary<string, DetalleBackup>();
        private string _detalleMostrado;

        private void ConstruirDetalle()
        {
            panelDetalle.SuspendLayout();

            lblDetalleTitulo = new Label
            {
                Text = "Detalle de la copia seleccionada",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(12, 10)
            };

            lblSinSeleccion = new Label
            {
                Text = "Elegí una copia de la lista para ver sus datos.",
                ForeColor = ColorGrisDetalle,
                AutoSize = true,
                Location = new Point(12, 42)
            };

            tablaDetalle = new TableLayoutPanel
            {
                Location = new Point(12, 38),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ColumnCount = 3,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Visible = false
            };
            for (int i = 0; i < 3; i++)
                tablaDetalle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

            vArchivo = Dato(0, 0, "Archivo");
            vTipo = Dato(1, 0, "Tipo");
            vCreada = Dato(2, 0, "Creada");
            vTamano = Dato(0, 1, "Tamaño");
            vBase = Dato(1, 1, "Base de datos");
            vServidor = Dato(2, 1, "Servidor");
            vUsuarioSql = Dato(0, 2, "Hecha por (usuario de SQL Server)");
            vVersion = Dato(1, 2, "Versión");
            vIntegridad = Dato(2, 2, "Control de integridad");
            vUbicacion = Dato(0, 3, "Ubicación", 3);

            btnVerificar = new Button
            {
                Text = "Verificar integridad ahora",
                AutoSize = true,
                Padding = new Padding(6, 1, 6, 1),
                Visible = false,
                UseVisualStyleBackColor = true
            };
            btnVerificar.Click += btnVerificar_Click;

            lblVerificacion = new Label { AutoSize = true, Visible = false };

            panelDetalle.Controls.Add(lblDetalleTitulo);
            panelDetalle.Controls.Add(lblSinSeleccion);
            panelDetalle.Controls.Add(tablaDetalle);
            panelDetalle.Controls.Add(btnVerificar);
            panelDetalle.Controls.Add(lblVerificacion);

            panelDetalle.Resize += (s, e) => AcomodarDetalle();
            tablaDetalle.SizeChanged += (s, e) => AcomodarDetalle();

            panelDetalle.ResumeLayout(false);
            panelDetalle.PerformLayout();
            AcomodarDetalle();
        }

        /// <summary>Crea una celda de la tabla: un rótulo gris chico y debajo el valor en negrita.</summary>
        private Label Dato(int columna, int fila, string rotulo, int ancho = 1)
        {
            var celda = new Panel { Dock = DockStyle.Fill, Height = 46, Margin = new Padding(0, 0, 8, 4) };

            celda.Controls.Add(new Label
            {
                Text = rotulo,
                ForeColor = ColorGrisDetalle,
                AutoSize = true,
                Location = new Point(0, 0),
                UseMnemonic = false
            });

            var valor = new Label
            {
                AutoEllipsis = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(0, 20),
                Size = new Size(200, 22),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                UseMnemonic = false
            };
            celda.Controls.Add(valor);

            tablaDetalle.Controls.Add(celda, columna, fila);
            if (ancho > 1)
                tablaDetalle.SetColumnSpan(celda, ancho);

            return valor;
        }

        /// <summary>El botón de verificar queda debajo de la tabla, sea cual sea su altura.</summary>
        private void AcomodarDetalle()
        {
            if (tablaDetalle == null) return;

            tablaDetalle.Width = Math.Max(300, panelDetalle.ClientSize.Width - 24);

            int y = tablaDetalle.Bottom + 6;
            btnVerificar.Location = new Point(12, y);
            lblVerificacion.Location = new Point(btnVerificar.Right + 12, y + 6);
        }

        // -----------------------------------------------------------------
        // Mostrar la copia elegida
        // -----------------------------------------------------------------

        private void MostrarDetalle()
        {
            string nombre = NombreSeleccionado();

            if (nombre == null)
            {
                _detalleMostrado = null;
                lblSinSeleccion.Visible = true;
                tablaDetalle.Visible = false;
                btnVerificar.Visible = false;
                lblVerificacion.Visible = false;
                return;
            }

            if (nombre == _detalleMostrado)
                return;   // el listado se refrescó pero la elección no cambió

            DetalleBackup d;
            if (!_detalles.TryGetValue(nombre, out d))
            {
                try
                {
                    Cursor = Cursors.WaitCursor;
                    d = NegocioBackup.ObtenerDetalle(nombre);
                    _detalles[nombre] = d;
                }
                catch (ReglaNegocioException)
                {
                    // La copia ya no está o no se puede acceder: se muestra como si no hubiera elección.
                    _detalleMostrado = null;
                    lblSinSeleccion.Visible = true;
                    tablaDetalle.Visible = false;
                    btnVerificar.Visible = false;
                    lblVerificacion.Visible = false;
                    return;
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }

            _detalleMostrado = nombre;

            const string NoDisponible = "No se pudo leer";

            vArchivo.Text = d.Archivo;
            vTipo.Text = d.Tipo;
            vCreada.Text = d.FechaHora.ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture);
            vTamano.Text = d.Tamano;
            vUbicacion.Text = d.Ubicacion;

            vBase.Text = d.EncabezadoLeido ? d.BaseDeDatos : NoDisponible;
            vServidor.Text = d.EncabezadoLeido ? d.Servidor : NoDisponible;
            vUsuarioSql.Text = d.EncabezadoLeido ? d.UsuarioSql : NoDisponible;
            vVersion.Text = d.EncabezadoLeido ? d.VersionSql : NoDisponible;
            vIntegridad.Text = !d.EncabezadoLeido
                ? NoDisponible
                : d.ConChecksum ? "Con checksum (se puede verificar)" : "Sin checksum";

            lblVerificacion.Text = string.Empty;
            lblVerificacion.Visible = true;
            btnVerificar.Visible = true;
            btnVerificar.Enabled = true;

            lblSinSeleccion.Visible = false;
            tablaDetalle.Visible = true;
            AcomodarDetalle();
        }

        private void btnVerificar_Click(object sender, EventArgs e)
        {
            string nombre = NombreSeleccionado();
            if (nombre == null) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                btnVerificar.Enabled = false;
                lblVerificacion.ForeColor = ColorGrisDetalle;
                lblVerificacion.Text = "Verificando…";
                lblVerificacion.Refresh();

                NegocioBackup.VerificarIntegridad(nombre);

                lblVerificacion.ForeColor = ColorVerde;
                lblVerificacion.Text = "✔ La copia está íntegra y se puede restaurar.";
            }
            catch (ReglaNegocioException ex)
            {
                lblVerificacion.ForeColor = ColorRojo;
                lblVerificacion.Text = "✖ " + ex.Message;
            }
            catch (Exception)
            {
                lblVerificacion.ForeColor = ColorRojo;
                lblVerificacion.Text = "✖ No se pudo verificar la copia.";
            }
            finally
            {
                btnVerificar.Enabled = true;
                Cursor = Cursors.Default;
                AcomodarDetalle();
            }
        }

        /// <summary>Al actualizar el listado se olvida lo recordado: las copias pudieron cambiar.</summary>
        private void OlvidarDetalles()
        {
            _detalles.Clear();
            _detalleMostrado = null;
        }
    }
}
