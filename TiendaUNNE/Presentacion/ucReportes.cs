using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace TiendaUNNE
{
    /// <summary>
    /// Sección de Reportes: ventas, productos y recaudación para el rango de fechas
    /// que elige el usuario (Desde/Hasta). Los tres reportes los arma NegocioReporte con
    /// datos de la base: acá solo se elige cuál mostrar y se dibuja el resultado.
    /// </summary>
    public partial class ucReportes : UserControl
    {
        private enum TipoReporte { Ventas, Productos, Recaudacion }

        private static readonly Color ColorAcento = Color.FromArgb(59, 130, 246);
        private static readonly Color ColorAcentoFondo = Color.FromArgb(234, 242, 251);
        private static readonly Color ColorTexto = Color.FromArgb(45, 48, 54);

        private TipoReporte _tipo = TipoReporte.Ventas;
        private Chart _grafico;

        public ucReportes()
        {
            InitializeComponent();
        }

        private void ucReportes_Load(object sender, EventArgs e)
        {
            CrearGrafico();

            dtpHasta.Value = DateTime.Today;
            dtpDesde.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            MostrarReporte();
        }

        private void CrearGrafico()
        {
            _grafico = new Chart { Dock = DockStyle.Fill, BackColor = Color.White };

            var area = new ChartArea("principal") { BackColor = Color.White };
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisX.Interval = 1;
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 8.5f);
            area.AxisX.LineColor = Color.FromArgb(214, 216, 220);
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(235, 236, 240);
            area.AxisY.LabelStyle.Font = new Font("Segoe UI", 8.5f);
            area.AxisY.LineColor = Color.FromArgb(214, 216, 220);
            _grafico.ChartAreas.Add(area);

            _grafico.Series.Add(new Series("datos")
            {
                ChartType = SeriesChartType.Column,
                Color = ColorAcento,
                IsValueShownAsLabel = true,
                Font = new Font("Segoe UI", 8.5f)
            });

            panelGrafico.Controls.Add(_grafico);
            _grafico.BringToFront();   // así el gráfico ocupa el espacio que deja el título
        }

        // -----------------------------------------------------------------
        // Eventos
        // -----------------------------------------------------------------

        private void btnVentas_Click(object sender, EventArgs e) => CambiarTipo(TipoReporte.Ventas);
        private void btnProductos_Click(object sender, EventArgs e) => CambiarTipo(TipoReporte.Productos);
        private void btnRecaudacion_Click(object sender, EventArgs e) => CambiarTipo(TipoReporte.Recaudacion);

        private void btnAplicar_Click(object sender, EventArgs e) => MostrarReporte();

        private void CambiarTipo(TipoReporte tipo)
        {
            _tipo = tipo;
            MostrarReporte();
        }

        // -----------------------------------------------------------------
        // Pintado
        // -----------------------------------------------------------------

        private void MostrarReporte()
        {
            if (_grafico == null) return;

            DateTime desde = dtpDesde.Value.Date;
            DateTime hasta = dtpHasta.Value.Date;

            ReporteVista vista;
            try
            {
                Cursor = Cursors.WaitCursor;
                NegocioReporte.ValidarRango(desde, hasta);

                switch (_tipo)
                {
                    case TipoReporte.Ventas:
                        vista = NegocioReporte.Ventas(desde, hasta);
                        break;
                    case TipoReporte.Productos:
                        vista = NegocioReporte.Productos(desde, hasta);
                        break;
                    default:
                        vista = NegocioReporte.Recaudacion(desde, hasta);
                        break;
                }
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Rango de fechas inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo generar el reporte.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            PintarBoton(btnVentas, _tipo == TipoReporte.Ventas);
            PintarBoton(btnProductos, _tipo == TipoReporte.Productos);
            PintarBoton(btnRecaudacion, _tipo == TipoReporte.Recaudacion);

            lblIndTitulo1.Text = vista.Indicadores[0].Titulo;
            lblIndValor1.Text = vista.Indicadores[0].Valor;
            lblIndTitulo2.Text = vista.Indicadores[1].Titulo;
            lblIndValor2.Text = vista.Indicadores[1].Valor;
            lblIndTitulo3.Text = vista.Indicadores[2].Titulo;
            lblIndValor3.Text = vista.Indicadores[2].Valor;

            lblTituloGrafico.Text = vista.TituloGrafico;
            Series serie = _grafico.Series["datos"];
            serie.Points.Clear();
            for (int i = 0; i < vista.Etiquetas.Count; i++)
                serie.Points.AddXY(vista.Etiquetas[i], vista.Valores[i]);

            dgvDetalle.Rows.Clear();
            dgvDetalle.Columns.Clear();
            for (int i = 0; i < vista.Columnas.Count; i++)
            {
                int indice = dgvDetalle.Columns.Add("col" + i, vista.Columnas[i]);
                if (i > 0)
                    dgvDetalle.Columns[indice].DefaultCellStyle.Alignment =
                        DataGridViewContentAlignment.MiddleRight;
            }
            foreach (string[] fila in vista.Filas)
                dgvDetalle.Rows.Add(fila);

            lblAviso.Text = vista.Descripcion;
        }

        private static void PintarBoton(Button boton, bool activo)
        {
            boton.BackColor = activo ? ColorAcentoFondo : Color.White;
            boton.ForeColor = activo ? ColorAcento : ColorTexto;
            boton.FlatAppearance.BorderColor = activo ? ColorAcento : Color.FromArgb(214, 216, 220);
        }
    }
}
