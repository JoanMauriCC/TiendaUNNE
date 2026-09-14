using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace TiendaUNNE
{
    /// <summary>
    /// Sección de Reportes: ventas, productos y recaudación por período.
    /// Por ahora es solo la vista: muestra datos de ejemplo fijos y no consulta la
    /// base de datos. Cuando se conecte, los datos tienen que venir de la capa Negocio.
    /// </summary>
    public partial class ucReportes : UserControl
    {
        private enum TipoReporte { Ventas, Productos, Recaudacion }

        private const string PeriodoHoy = "Hoy";
        private const string PeriodoMes = "Este mes";

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

            cboPeriodo.Items.AddRange(new object[] { PeriodoHoy, PeriodoMes });
            cboPeriodo.SelectedItem = PeriodoMes;   // dispara MostrarReporte
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

        private void cboPeriodo_SelectedIndexChanged(object sender, EventArgs e) => MostrarReporte();

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

            bool esHoy = (cboPeriodo.SelectedItem as string) == PeriodoHoy;
            ReporteEjemplo r = ObtenerEjemplo(_tipo, esHoy);

            PintarBoton(btnVentas, _tipo == TipoReporte.Ventas);
            PintarBoton(btnProductos, _tipo == TipoReporte.Productos);
            PintarBoton(btnRecaudacion, _tipo == TipoReporte.Recaudacion);

            lblIndTitulo1.Text = r.Indicadores[0, 0];
            lblIndValor1.Text = r.Indicadores[0, 1];
            lblIndTitulo2.Text = r.Indicadores[1, 0];
            lblIndValor2.Text = r.Indicadores[1, 1];
            lblIndTitulo3.Text = r.Indicadores[2, 0];
            lblIndValor3.Text = r.Indicadores[2, 1];

            lblTituloGrafico.Text = r.TituloGrafico;
            Series serie = _grafico.Series["datos"];
            serie.Points.Clear();
            for (int i = 0; i < r.Etiquetas.Length; i++)
                serie.Points.AddXY(r.Etiquetas[i], r.Valores[i]);

            dgvDetalle.Rows.Clear();
            dgvDetalle.Columns.Clear();
            for (int i = 0; i < r.Columnas.Length; i++)
            {
                int indice = dgvDetalle.Columns.Add("col" + i, r.Columnas[i]);
                if (i > 0)
                    dgvDetalle.Columns[indice].DefaultCellStyle.Alignment =
                        DataGridViewContentAlignment.MiddleRight;
            }
            foreach (string[] fila in r.Filas)
                dgvDetalle.Rows.Add(fila);
        }

        private static void PintarBoton(Button boton, bool activo)
        {
            boton.BackColor = activo ? ColorAcentoFondo : Color.White;
            boton.ForeColor = activo ? ColorAcento : ColorTexto;
            boton.FlatAppearance.BorderColor = activo ? ColorAcento : Color.FromArgb(214, 216, 220);
        }

        // -----------------------------------------------------------------
        // Datos de ejemplo (se reemplazan cuando la vista se conecte a Negocio)
        // -----------------------------------------------------------------

        private sealed class ReporteEjemplo
        {
            public string[,] Indicadores;
            public string TituloGrafico;
            public string[] Etiquetas;
            public double[] Valores;
            public string[] Columnas;
            public string[][] Filas;
        }

        private static ReporteEjemplo ObtenerEjemplo(TipoReporte tipo, bool esHoy)
        {
            switch (tipo)
            {
                case TipoReporte.Productos:
                    return new ReporteEjemplo
                    {
                        Indicadores = esHoy
                            ? new[,] { { "Unidades vendidas", "41" }, { "Productos distintos", "12" }, { "Con stock bajo", "3" } }
                            : new[,] { { "Unidades vendidas", "1.032" }, { "Productos distintos", "38" }, { "Con stock bajo", "3" } },
                        TituloGrafico = "Más vendidos (unidades)",
                        Etiquetas = new[] { "Remera", "Gorra", "Medias", "Buzo", "Short" },
                        Valores = esHoy ? new double[] { 14, 9, 6, 7, 5 } : new double[] { 310, 204, 181, 140, 97 },
                        Columnas = new[] { "Producto con stock bajo", "Stock" },
                        Filas = new[]
                        {
                            new[] { "Buzo canguro", "2" },
                            new[] { "Campera", "1" },
                            new[] { "Gorra", "4" }
                        }
                    };

                case TipoReporte.Recaudacion:
                    return esHoy
                        ? new ReporteEjemplo
                        {
                            Indicadores = new[,] { { "Recaudado", "$ 412.300" }, { "En efectivo", "$ 186.000" }, { "Otros medios", "$ 226.300" } },
                            TituloGrafico = "Por medio de pago (miles de $)",
                            Etiquetas = new[] { "Efectivo", "Débito", "Crédito", "Transf." },
                            Valores = new double[] { 186, 121, 64, 41 },
                            Columnas = new[] { "Medio de pago", "Pagos", "Importe" },
                            Filas = new[]
                            {
                                new[] { "Efectivo", "9", "$ 186.000" },
                                new[] { "Tarjeta de débito", "5", "$ 121.000" },
                                new[] { "Tarjeta de crédito", "3", "$ 64.300" },
                                new[] { "Transferencia", "1", "$ 41.000" }
                            }
                        }
                        : new ReporteEjemplo
                        {
                            Indicadores = new[,] { { "Recaudado", "$ 9.874.500" }, { "En efectivo", "$ 4.120.000" }, { "Otros medios", "$ 5.754.500" } },
                            TituloGrafico = "Recaudación mensual (millones de $)",
                            Etiquetas = new[] { "Jun", "Jul", "Ago", "Sep" },
                            Valores = new double[] { 7.9, 8.6, 9.1, 9.9 },
                            Columnas = new[] { "Mes", "Ventas", "Total" },
                            Filas = new[]
                            {
                                new[] { "Septiembre", "426", "$ 9.874.500" },
                                new[] { "Agosto", "398", "$ 9.102.000" },
                                new[] { "Julio", "371", "$ 8.640.300" },
                                new[] { "Junio", "344", "$ 7.905.200" }
                            }
                        };

                default:
                    return esHoy
                        ? new ReporteEjemplo
                        {
                            Indicadores = new[,] { { "Ventas", "18" }, { "Total vendido", "$ 412.300" }, { "Ticket promedio", "$ 22.905" } },
                            TituloGrafico = "Ventas por hora",
                            Etiquetas = new[] { "9h", "11h", "13h", "15h", "17h", "19h" },
                            Valores = new double[] { 2, 5, 3, 1, 4, 3 },
                            Columnas = new[] { "Venta", "Hora", "Total" },
                            Filas = new[]
                            {
                                new[] { "Nº 118", "19:42", "$ 31.200" },
                                new[] { "Nº 117", "19:10", "$ 12.500" },
                                new[] { "Nº 116", "18:55", "$ 47.800" }
                            }
                        }
                        : new ReporteEjemplo
                        {
                            Indicadores = new[,] { { "Ventas", "426" }, { "Total vendido", "$ 9.874.500" }, { "Ticket promedio", "$ 23.180" } },
                            TituloGrafico = "Ventas por semana",
                            Etiquetas = new[] { "Sem 1", "Sem 2", "Sem 3", "Sem 4" },
                            Valores = new double[] { 98, 112, 121, 95 },
                            Columnas = new[] { "Período", "Ventas", "Total" },
                            Filas = new[]
                            {
                                new[] { "Semana 4", "95", "$ 2.205.000" },
                                new[] { "Semana 3", "121", "$ 2.810.400" },
                                new[] { "Semana 2", "112", "$ 2.598.000" },
                                new[] { "Semana 1", "98", "$ 2.260.100" }
                            }
                        };
            }
        }
    }
}
