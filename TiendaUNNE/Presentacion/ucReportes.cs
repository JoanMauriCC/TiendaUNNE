using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        private static readonly SKColor[] ColoresTorta =
        {
            new SKColor(59, 130, 246), new SKColor(16, 185, 129), new SKColor(245, 158, 11),
            new SKColor(239, 68, 68), new SKColor(139, 92, 246), new SKColor(20, 184, 166)
        };
        private const float TamanioTextoGrafico = 12f;
        private const double AnchoBaston = 4;
        private const string FormatoValorGrafico = "#,##0.##";
        private const string FormatoPorcentaje = "0.#";

        /// <summary>Con más puntos que esto los números se pisan: el valor se ve pasando el mouse.</summary>
        private const int MaximoPuntosConValor = 12;

        /// <summary>Nombres más largos que esto (productos, medios de pago) se inclinan para no pisarse.</summary>
        private const int LargoEtiquetaHorizontal = 8;
        private const double RotacionEtiquetas = 30;

        private TipoReporte _tipo = TipoReporte.Ventas;
        private ReporteVista _vista;
        private bool _cargandoTipos;

        private CartesianChart _grafico;   // barras, bastones, línea y escalonado
        private PieChart _torta;
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

            _torta = new PieChart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Visible = false,
                LegendPosition = LegendPosition.Right,
                LegendTextPaint = new SolidColorPaint(TextoGrafico),
                LegendTextSize = TamanioTextoGrafico
            };

            // Así los gráficos ocupan el espacio que deja el título.
            panelGrafico.Controls.Add(_grafico);
            panelGrafico.Controls.Add(_torta);
            _grafico.BringToFront();
            _torta.BringToFront();
        }

        // -----------------------------------------------------------------
        // Eventos
        // -----------------------------------------------------------------

        private void btnVentas_Click(object sender, EventArgs e) => CambiarTipo(TipoReporte.Ventas);
        private void btnProductos_Click(object sender, EventArgs e) => CambiarTipo(TipoReporte.Productos);
        private void btnRecaudacion_Click(object sender, EventArgs e) => CambiarTipo(TipoReporte.Recaudacion);

        private void btnAplicar_Click(object sender, EventArgs e) => MostrarReporte();

        private void cboTipoGrafico_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_cargandoTipos)
                DibujarGrafico();
        }

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

            _vista = vista;
            CargarTiposGrafico();
            DibujarGrafico();

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

        // -----------------------------------------------------------------
        // Gráfico
        // -----------------------------------------------------------------

        /// <summary>
        /// Ofrece los gráficos que NegocioReporte permite para este reporte. Si el que estaba
        /// elegido sigue permitido se mantiene; si no, se vuelve al primero (barras).
        /// </summary>
        private void CargarTiposGrafico()
        {
            TipoGrafico? anterior = cboTipoGrafico.SelectedValue as TipoGrafico?;

            _cargandoTipos = true;
            cboTipoGrafico.DisplayMember = "Value";
            cboTipoGrafico.ValueMember = "Key";
            cboTipoGrafico.DataSource = _vista.TiposGrafico
                .Select(t => new KeyValuePair<TipoGrafico, string>(t, NegocioReporte.NombreGrafico(t)))
                .ToList();
            cboTipoGrafico.SelectedValue = anterior.HasValue && _vista.TiposGrafico.Contains(anterior.Value)
                ? anterior.Value
                : _vista.TiposGrafico[0];
            _cargandoTipos = false;
        }

        private void DibujarGrafico()
        {
            if (_vista == null || !(cboTipoGrafico.SelectedValue is TipoGrafico)) return;

            var tipo = (TipoGrafico)cboTipoGrafico.SelectedValue;

            lblTituloGrafico.Text = tipo == TipoGrafico.Escalonado ? _vista.TituloAcumulado : _vista.TituloGrafico;
            _grafico.Visible = tipo != TipoGrafico.Torta;
            _torta.Visible = tipo == TipoGrafico.Torta;

            if (tipo == TipoGrafico.Torta)
            {
                _torta.Series = CrearPorciones();
                return;
            }

            _ejeX.Labels = _vista.Etiquetas.ToArray();
            _ejeX.LabelsRotation = _vista.Etiquetas.Any(e => e.Length > LargoEtiquetaHorizontal) ? RotacionEtiquetas : 0;
            _grafico.Series = new[] { CrearSerie(tipo) };
        }

        private ISeries CrearSerie(TipoGrafico tipo)
        {
            switch (tipo)
            {
                case TipoGrafico.Linea:
                    return new LineSeries<double>
                    {
                        Values = _vista.Valores.ToArray(),
                        Stroke = new SolidColorPaint(AcentoGrafico, 3),
                        Fill = null,
                        GeometrySize = 8,
                        GeometryStroke = new SolidColorPaint(AcentoGrafico, 3),
                        LineSmoothness = 0,
                        DataLabelsPaint = PinturaValores(),
                        DataLabelsSize = TamanioTextoGrafico,
                        DataLabelsPosition = DataLabelsPosition.Top,
                        DataLabelsFormatter = punto => punto.Coordinate.PrimaryValue.ToString(FormatoValorGrafico)
                    };

                case TipoGrafico.Escalonado:
                    return new StepLineSeries<double>
                    {
                        Values = _vista.ValoresAcumulados.ToArray(),
                        Stroke = new SolidColorPaint(AcentoGrafico, 3),
                        Fill = null,
                        GeometrySize = 8,
                        GeometryStroke = new SolidColorPaint(AcentoGrafico, 3),
                        DataLabelsPaint = PinturaValores(),
                        DataLabelsSize = TamanioTextoGrafico,
                        DataLabelsPosition = DataLabelsPosition.Top,
                        DataLabelsFormatter = punto => punto.Coordinate.PrimaryValue.ToString(FormatoValorGrafico)
                    };

                default:   // barras y bastones: los bastones son barras finitas
                    return new ColumnSeries<double>
                    {
                        Values = _vista.Valores.ToArray(),
                        Fill = new SolidColorPaint(AcentoGrafico),
                        MaxBarWidth = tipo == TipoGrafico.Bastones ? AnchoBaston : double.MaxValue,
                        DataLabelsPaint = PinturaValores(),
                        DataLabelsSize = TamanioTextoGrafico,
                        DataLabelsPosition = DataLabelsPosition.Top,
                        DataLabelsFormatter = punto => punto.Coordinate.PrimaryValue.ToString(FormatoValorGrafico)
                    };
            }
        }

        /// <summary>Pintura de los números sobre cada punto, o ninguna si son demasiados.</summary>
        private SolidColorPaint PinturaValores()
        {
            return _vista.Valores.Count <= MaximoPuntosConValor ? new SolidColorPaint(TextoGrafico) : null;
        }

        /// <summary>Una porción por etiqueta, con su porcentaje del total escrito adentro.</summary>
        private ISeries[] CrearPorciones()
        {
            var porciones = new ISeries[_vista.Valores.Count];
            for (int i = 0; i < porciones.Length; i++)
            {
                string porcentaje = _vista.Porcentajes[i].ToString(FormatoPorcentaje) + " %";
                porciones[i] = new PieSeries<double>
                {
                    Name = _vista.Etiquetas[i],
                    Values = new[] { _vista.Valores[i] },
                    Fill = new SolidColorPaint(ColoresTorta[i % ColoresTorta.Length]),
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = TamanioTextoGrafico,
                    DataLabelsPosition = PolarLabelsPosition.Middle,
                    DataLabelsFormatter = punto => porcentaje
                };
            }
            return porciones;
        }

        private static void PintarBoton(Button boton, bool activo)
        {
            boton.BackColor = activo ? ColorAcentoFondo : Color.White;
            boton.ForeColor = activo ? ColorAcento : ColorTexto;
            boton.FlatAppearance.BorderColor = activo ? ColorAcento : Color.FromArgb(214, 216, 220);
        }
    }
}
