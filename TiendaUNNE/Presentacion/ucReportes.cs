using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace TiendaUNNE
{
    /// <summary>
    /// Sección de Reportes: ventas, productos y recaudación para el rango de fechas
    /// que elige el usuario (Desde/Hasta). Ventas y Productos ya salen de la base a través
    /// de NegocioReporte; Recaudación todavía muestra datos de ejemplo fijos.
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

                // Recaudación todavía no consulta la base: un rango de un solo día muestra el
                // ejemplo con detalle fino (por hora); uno más amplio, el agregado.
                switch (_tipo)
                {
                    case TipoReporte.Ventas:
                        vista = NegocioReporte.Ventas(desde, hasta);
                        break;
                    case TipoReporte.Productos:
                        vista = NegocioReporte.Productos(desde, hasta);
                        break;
                    default:
                        vista = Convertir(ObtenerEjemplo(_tipo, desde == hasta), desde, hasta);
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

        private static ReporteVista Convertir(ReporteEjemplo e, DateTime desde, DateTime hasta)
        {
            var vista = new ReporteVista
            {
                TituloGrafico = e.TituloGrafico,
                Descripcion = string.Format(
                    "Datos de ejemplo del {0:dd/MM/yyyy} al {1:dd/MM/yyyy}: este reporte todavía no está conectado a la base de datos.",
                    desde, hasta)
            };

            for (int i = 0; i < 3; i++)
                vista.Indicadores.Add(new IndicadorReporte(e.Indicadores[i, 0], e.Indicadores[i, 1]));

            vista.Etiquetas.AddRange(e.Etiquetas);
            vista.Valores.AddRange(e.Valores);
            vista.Columnas.AddRange(e.Columnas);
            vista.Filas.AddRange(e.Filas);
            return vista;
        }

        private static ReporteEjemplo ObtenerEjemplo(TipoReporte tipo, bool esHoy)
        {
            switch (tipo)
            {
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
