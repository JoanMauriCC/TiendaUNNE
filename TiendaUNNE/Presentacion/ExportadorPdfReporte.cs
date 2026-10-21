using System;
using System.IO;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

namespace TiendaUNNE
{
    /// <summary>
    /// Arma el PDF de un reporte con lo mismo que muestra la pantalla: encabezado, indicadores,
    /// el gráfico (como imagen) y la tabla de detalle. Es otra forma de mostrar el reporte, así
    /// que no calcula nada: todo llega armado en ReporteVista desde NegocioReporte.
    /// Usa MigraDoc (PDFsharp), que arma el documento por párrafos y tablas y pagina solo.
    /// </summary>
    public static class ExportadorPdfReporte
    {
        private static readonly Color ColorAcento = new Color(59, 130, 246);
        private static readonly Color ColorTexto = new Color(45, 48, 54);
        private static readonly Color ColorGris = new Color(120, 125, 135);
        private static readonly Color ColorFondo = new Color(234, 242, 251);
        private static readonly Color ColorBorde = new Color(214, 216, 220);

        private const string Fuente = "Segoe UI";
        private const double AnchoUtilCm = 17;   // A4 (21 cm) menos 2 cm de margen por lado
        private const string FormatoGenerado = "dd/MM/yyyy HH:mm";

        /// <summary>
        /// Guarda el PDF en la ruta indicada. La imagen del gráfico va en PNG; se escribe en un
        /// archivo temporal porque MigraDoc carga las imágenes desde disco, y se borra al terminar.
        /// </summary>
        public static void Guardar(ReporteVista vista, string tituloGrafico, byte[] imagenGrafico,
                                   string generadoPor, string ruta)
        {
            string archivoImagen = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".png");
            try
            {
                File.WriteAllBytes(archivoImagen, imagenGrafico);

                var renderer = new PdfDocumentRenderer
                {
                    Document = ArmarDocumento(vista, tituloGrafico, archivoImagen, generadoPor)
                };
                renderer.RenderDocument();
                renderer.Save(ruta);
            }
            finally
            {
                if (File.Exists(archivoImagen))
                    File.Delete(archivoImagen);
            }
        }

        private static Document ArmarDocumento(ReporteVista vista, string tituloGrafico,
                                               string archivoImagen, string generadoPor)
        {
            var documento = new Document();
            documento.Info.Title = vista.Titulo;
            documento.Info.Author = "TiendaUNNE";

            Style normal = documento.Styles[StyleNames.Normal];
            normal.Font.Name = Fuente;
            normal.Font.Size = 10;
            normal.Font.Color = ColorTexto;

            Section seccion = documento.AddSection();
            seccion.PageSetup = documento.DefaultPageSetup.Clone();
            seccion.PageSetup.PageFormat = PageFormat.A4;
            seccion.PageSetup.TopMargin = Unit.FromCentimeter(2);
            seccion.PageSetup.BottomMargin = Unit.FromCentimeter(2);
            seccion.PageSetup.LeftMargin = Unit.FromCentimeter(2);
            seccion.PageSetup.RightMargin = Unit.FromCentimeter(2);

            AgregarEncabezado(seccion, vista, generadoPor);
            AgregarIndicadores(seccion, vista);
            AgregarGrafico(seccion, tituloGrafico, archivoImagen);
            AgregarDetalle(seccion, vista);
            AgregarPie(seccion, vista);

            return documento;
        }

        private static void AgregarEncabezado(Section seccion, ReporteVista vista, string generadoPor)
        {
            Paragraph sistema = seccion.AddParagraph("TiendaUNNE");
            sistema.Format.Font.Size = 9;
            sistema.Format.Font.Color = ColorGris;

            Paragraph titulo = seccion.AddParagraph(vista.Titulo);
            titulo.Format.Font.Size = 18;
            titulo.Format.Font.Bold = true;
            titulo.Format.Font.Color = ColorAcento;

            seccion.AddParagraph(vista.Periodo);

            string generado = "Generado el " + DateTime.Now.ToString(FormatoGenerado)
                + (string.IsNullOrWhiteSpace(generadoPor) ? string.Empty : " por " + generadoPor);
            Paragraph linea = seccion.AddParagraph(generado);
            linea.Format.Font.Size = 9;
            linea.Format.Font.Color = ColorGris;
            linea.Format.SpaceAfter = Unit.FromCentimeter(0.5);
        }

        /// <summary>Los tres números de arriba de la pantalla, uno al lado del otro.</summary>
        private static void AgregarIndicadores(Section seccion, ReporteVista vista)
        {
            Table tabla = seccion.AddTable();
            tabla.Borders.Color = ColorBorde;
            tabla.Borders.Width = 0.5;
            tabla.Shading.Color = ColorFondo;

            double ancho = AnchoUtilCm / vista.Indicadores.Count;
            foreach (IndicadorReporte _ in vista.Indicadores)
                tabla.AddColumn(Unit.FromCentimeter(ancho));

            Row titulos = tabla.AddRow();
            Row valores = tabla.AddRow();
            for (int i = 0; i < vista.Indicadores.Count; i++)
            {
                Paragraph t = titulos.Cells[i].AddParagraph(vista.Indicadores[i].Titulo);
                t.Format.Font.Size = 9;
                t.Format.Font.Color = ColorGris;
                t.Format.SpaceBefore = Unit.FromPoint(4);
                t.Format.LeftIndent = Unit.FromPoint(4);

                Paragraph v = valores.Cells[i].AddParagraph(vista.Indicadores[i].Valor);
                v.Format.Font.Size = 14;
                v.Format.Font.Bold = true;
                v.Format.SpaceAfter = Unit.FromPoint(4);
                v.Format.LeftIndent = Unit.FromPoint(4);
            }
            titulos.Borders.Bottom.Visible = false;
            valores.Borders.Top.Visible = false;
        }

        private static void AgregarGrafico(Section seccion, string tituloGrafico, string archivoImagen)
        {
            Paragraph titulo = seccion.AddParagraph(tituloGrafico);
            titulo.Format.Font.Size = 12;
            titulo.Format.Font.Bold = true;
            titulo.Format.SpaceBefore = Unit.FromCentimeter(0.6);
            titulo.Format.SpaceAfter = Unit.FromCentimeter(0.2);

            MigraDoc.DocumentObjectModel.Shapes.Image imagen = seccion.AddImage(archivoImagen);
            imagen.Width = Unit.FromCentimeter(AnchoUtilCm);
            imagen.LockAspectRatio = true;
        }

        /// <summary>La misma tabla de la pantalla; si no entra en una hoja, repite el encabezado.</summary>
        private static void AgregarDetalle(Section seccion, ReporteVista vista)
        {
            Paragraph titulo = seccion.AddParagraph("Detalle");
            titulo.Format.Font.Size = 12;
            titulo.Format.Font.Bold = true;
            titulo.Format.SpaceBefore = Unit.FromCentimeter(0.6);
            titulo.Format.SpaceAfter = Unit.FromCentimeter(0.2);

            if (vista.Filas.Count == 0)
            {
                Paragraph vacio = seccion.AddParagraph("Sin datos en este período.");
                vacio.Format.Font.Color = ColorGris;
                return;
            }

            Table tabla = seccion.AddTable();
            tabla.Borders.Color = ColorBorde;
            tabla.Borders.Width = 0.5;
            tabla.LeftPadding = Unit.FromPoint(4);
            tabla.RightPadding = Unit.FromPoint(4);
            tabla.TopPadding = Unit.FromPoint(2);
            tabla.BottomPadding = Unit.FromPoint(2);

            double ancho = AnchoUtilCm / vista.Columnas.Count;
            foreach (string _ in vista.Columnas)
                tabla.AddColumn(Unit.FromCentimeter(ancho));

            Row encabezado = tabla.AddRow();
            encabezado.HeadingFormat = true;   // se repite arriba de cada hoja
            encabezado.Shading.Color = ColorFondo;
            encabezado.Format.Font.Bold = true;
            for (int i = 0; i < vista.Columnas.Count; i++)
                encabezado.Cells[i].AddParagraph(vista.Columnas[i]);

            foreach (string[] fila in vista.Filas)
            {
                Row renglon = tabla.AddRow();
                for (int i = 0; i < fila.Length; i++)
                {
                    Paragraph celda = renglon.Cells[i].AddParagraph(fila[i]);
                    if (i > 0)   // números a la derecha, como en la pantalla
                        celda.Format.Alignment = ParagraphAlignment.Right;
                }
            }
        }

        private static void AgregarPie(Section seccion, ReporteVista vista)
        {
            if (!string.IsNullOrWhiteSpace(vista.Descripcion))
            {
                Paragraph nota = seccion.AddParagraph(vista.Descripcion);
                nota.Format.Font.Size = 9;
                nota.Format.Font.Color = ColorGris;
                nota.Format.SpaceBefore = Unit.FromCentimeter(0.4);
            }

            Paragraph pie = seccion.Footers.Primary.AddParagraph();
            pie.Format.Font.Size = 8;
            pie.Format.Font.Color = ColorGris;
            pie.Format.Alignment = ParagraphAlignment.Center;
            pie.AddText("TiendaUNNE · " + vista.Titulo + " · Página ");
            pie.AddPageField();
            pie.AddText(" de ");
            pie.AddNumPagesField();
        }
    }
}
