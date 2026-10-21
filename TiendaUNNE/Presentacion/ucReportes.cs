using System;
using System.Drawing;
using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WinForms;
using SkiaSharp;

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

        // Los mismos colores, en el formato que usa LiveCharts (SkiaSharp).
        private static readonly SKColor AcentoGrafico = new SKColor(59, 130, 246);
        private static readonly SKColor TextoGrafico = new SKColor(45, 48, 54);
        private static readonly SKColor GrillaGrafico = new SKColor(235, 236, 240);
        private const float TamanioTextoGrafico = 12f;
        private const string FormatoValorGrafico = "#,##0.##";

        private TipoReporte _tipo = TipoReporte.Ventas;
        private CartesianChart _grafico;
        private Axis _ejeX;

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
            _ejeX = new Axis
            {
                TextSize = TamanioTextoGrafico,
                LabelsPaint = new SolidColorPaint(TextoGrafico),
                MinStep = 1,   // una etiqueta por barra, como antes
                ShowSeparatorLines = false
            };

            _grafico = new CartesianChart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                XAxes = new[] { _ejeX },
                YAxes = new[]
                {
                    new Axis
                    {
                        TextSize = TamanioTextoGrafico,
                        LabelsPaint = new SolidColorPaint(TextoGrafico),
                        SeparatorsPaint = new SolidColorPaint(GrillaGrafico),
                        Labeler = valor => valor.ToString(FormatoValorGrafico),
                        MinLimit = 0,
                        MinStep = 1    // sin divisiones de 0,2 cuando hay pocas ventas
                    }
                }
            };

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
            _ejeX.Labels = vista.Etiquetas.ToArray();
            _grafico.Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Name = vista.TituloGrafico,
                    Values = vista.Valores.ToArray(),
                    Fill = new SolidColorPaint(AcentoGrafico),
                    DataLabelsPaint = new SolidColorPaint(TextoGrafico),
                    DataLabelsSize = TamanioTextoGrafico,
                    DataLabelsPosition = DataLabelsPosition.Top,
                    DataLabelsFormatter = punto => punto.Coordinate.PrimaryValue.ToString(FormatoValorGrafico)
                }
            };

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
