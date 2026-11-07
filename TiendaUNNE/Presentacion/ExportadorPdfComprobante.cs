using System;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

namespace TiendaUNNE
{
    /// <summary>
    /// Arma el PDF de un comprobante ya emitido. No calcula nada: todo llega armado en
    /// ComprobanteVista desde NegocioComprobante, y esto solo lo acomoda en una hoja A4.
    /// Usa MigraDoc (PDFsharp), igual que el exportador de reportes.
    /// </summary>
    public static class ExportadorPdfComprobante
    {
        private static readonly Color ColorAcento = new Color(59, 130, 246);
        private static readonly Color ColorTexto = new Color(45, 48, 54);
        private static readonly Color ColorGris = new Color(120, 125, 135);
        private static readonly Color ColorFondo = new Color(234, 242, 251);
        private static readonly Color ColorBorde = new Color(214, 216, 220);

        private const string Fuente = "Segoe UI";
        private const double AnchoUtilCm = 17;   // A4 (21 cm) menos 2 cm de margen por lado

        public static void Guardar(ComprobanteVista v, string ruta)
        {
            var renderer = new PdfDocumentRenderer { Document = ArmarDocumento(v) };
            renderer.RenderDocument();
            renderer.Save(ruta);
        }

        /// <summary>Nombre de archivo sugerido: "Factura-B-0001-00000003.pdf".</summary>
        public static string NombreSugerido(ComprobanteVista v)
        {
            return v.Nombre.Replace(' ', '-') + "-" + v.NumeroFormateado + (v.EsCopia ? "-reimpresion" : string.Empty) + ".pdf";
        }

        private static Document ArmarDocumento(ComprobanteVista v)
        {
            var documento = new Document();
            documento.Info.Title = v.Nombre + " " + v.NumeroFormateado;
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

            AgregarEncabezado(seccion, v);
            AgregarCliente(seccion, v);
            AgregarRenglones(seccion, v);
            AgregarTotales(seccion, v);
            AgregarPagos(seccion, v);
            AgregarPie(seccion, v);

            return documento;
        }

        private static void AgregarEncabezado(Section seccion, ComprobanteVista v)
        {
            Paragraph sistema = seccion.AddParagraph("TiendaUNNE");
            sistema.Format.Font.Size = 9;
            sistema.Format.Font.Color = ColorGris;

            Paragraph titulo = seccion.AddParagraph(v.Nombre.ToUpperInvariant());
            titulo.Format.Font.Size = 20;
            titulo.Format.Font.Bold = true;
            titulo.Format.Font.Color = ColorAcento;

            seccion.AddParagraph("Nº " + v.NumeroFormateado).Format.Font.Size = 13;

            if (v.EsCopia || v.Anulada)
            {
                Paragraph aviso = seccion.AddParagraph(
                    (v.EsCopia ? "REIMPRESIÓN" : string.Empty) +
                    (v.EsCopia && v.Anulada ? "  ·  " : string.Empty) +
                    (v.Anulada ? "VENTA ANULADA" : string.Empty));
                aviso.Format.Font.Bold = true;
                aviso.Format.Font.Size = 11;
                aviso.Format.Font.Color = new Color(180, 35, 24);
            }

            Paragraph fecha = seccion.AddParagraph(
                "Fecha: " + v.FechaHora.ToString("dd/MM/yyyy HH:mm") +
                (string.IsNullOrWhiteSpace(v.Cajero) ? string.Empty : "   ·   Atendió: " + v.Cajero));
            fecha.Format.Font.Size = 9;
            fecha.Format.Font.Color = ColorGris;
            fecha.Format.SpaceAfter = Unit.FromCentimeter(0.4);
        }

        private static void AgregarCliente(Section seccion, ComprobanteVista v)
        {
            Table tabla = seccion.AddTable();
            tabla.Borders.Color = ColorBorde;
            tabla.Borders.Width = 0.5;
            tabla.Shading.Color = ColorFondo;
            tabla.LeftPadding = Unit.FromPoint(6);
            tabla.TopPadding = Unit.FromPoint(4);
            tabla.BottomPadding = Unit.FromPoint(4);
            tabla.AddColumn(Unit.FromCentimeter(AnchoUtilCm));

            Row fila = tabla.AddRow();
            Paragraph rotulo = fila.Cells[0].AddParagraph("CLIENTE");
            rotulo.Format.Font.Size = 8;
            rotulo.Format.Font.Color = ColorGris;

            Paragraph nombre = fila.Cells[0].AddParagraph(v.ClienteNombre);
            nombre.Format.Font.Bold = true;
            nombre.Format.Font.Size = 11;

            Paragraph datos = fila.Cells[0].AddParagraph(v.ClienteDocumento + "   ·   " + v.ClienteCondicionIva);
            datos.Format.Font.Size = 9;
        }

        private static void AgregarRenglones(Section seccion, ComprobanteVista v)
        {
            Paragraph titulo = seccion.AddParagraph("Detalle");
            titulo.Format.Font.Size = 12;
            titulo.Format.Font.Bold = true;
            titulo.Format.SpaceBefore = Unit.FromCentimeter(0.6);
            titulo.Format.SpaceAfter = Unit.FromCentimeter(0.2);

            Table tabla = seccion.AddTable();
            tabla.Borders.Color = ColorBorde;
            tabla.Borders.Width = 0.5;
            tabla.LeftPadding = Unit.FromPoint(4);
            tabla.RightPadding = Unit.FromPoint(4);
            tabla.TopPadding = Unit.FromPoint(2);
            tabla.BottomPadding = Unit.FromPoint(2);

            tabla.AddColumn(Unit.FromCentimeter(8.5));
            tabla.AddColumn(Unit.FromCentimeter(2.3));
            tabla.AddColumn(Unit.FromCentimeter(3.1));
            tabla.AddColumn(Unit.FromCentimeter(3.1));

            string[] titulos = { "Descripción", "Cant.", "Precio unit.", "Subtotal" };
            Row encabezado = tabla.AddRow();
            encabezado.HeadingFormat = true;
            encabezado.Shading.Color = ColorFondo;
            encabezado.Format.Font.Bold = true;
            for (int i = 0; i < titulos.Length; i++)
            {
                Paragraph p = encabezado.Cells[i].AddParagraph(titulos[i]);
                if (i > 0) p.Format.Alignment = ParagraphAlignment.Right;
            }

            foreach (ComprobanteRenglon r in v.Renglones)
            {
                Row fila = tabla.AddRow();
                fila.Cells[0].AddParagraph(r.Descripcion);
                Derecha(fila.Cells[1], r.Cantidad.ToString(NegocioVenta.FormatoCantidadTicket));
                Derecha(fila.Cells[2], r.PrecioUnitario.ToString(NegocioVenta.FormatoImporte));
                Derecha(fila.Cells[3], r.Subtotal.ToString(NegocioVenta.FormatoImporte));
            }
        }

        private static void AgregarTotales(Section seccion, ComprobanteVista v)
        {
            Table tabla = seccion.AddTable();
            tabla.LeftPadding = Unit.FromPoint(4);
            tabla.RightPadding = Unit.FromPoint(4);
            tabla.TopPadding = Unit.FromPoint(2);
            tabla.BottomPadding = Unit.FromPoint(2);
            tabla.AddColumn(Unit.FromCentimeter(AnchoUtilCm - 6));
            tabla.AddColumn(Unit.FromCentimeter(3));
            tabla.AddColumn(Unit.FromCentimeter(3));

            tabla.Rows.LeftIndent = 0;
            seccion.AddParagraph().Format.SpaceAfter = Unit.FromPoint(2);

            if (v.DiscriminaIva)
            {
                Linea(tabla, "Importe neto gravado", v.ImporteNeto, false);
                Linea(tabla, "IVA " + NegocioComprobante.AlicuotaIva.ToString("0.##") + " %", v.ImporteIva, false);
            }

            Row total = Linea(tabla, "TOTAL", v.Total, true);
            total.Shading.Color = ColorFondo;
        }

        private static Row Linea(Table tabla, string rotulo, decimal importe, bool destacada)
        {
            Row fila = tabla.AddRow();
            fila.Cells[1].MergeRight = 0;

            Paragraph r = fila.Cells[1].AddParagraph(rotulo);
            r.Format.Alignment = ParagraphAlignment.Right;

            Paragraph valor = fila.Cells[2].AddParagraph("$ " + importe.ToString(NegocioVenta.FormatoImporte));
            valor.Format.Alignment = ParagraphAlignment.Right;

            if (destacada)
            {
                r.Format.Font.Bold = true;
                valor.Format.Font.Bold = true;
                valor.Format.Font.Size = 13;
            }
            return fila;
        }

        private static void AgregarPagos(Section seccion, ComprobanteVista v)
        {
            if (v.Pagos.Count == 0) return;

            Paragraph titulo = seccion.AddParagraph("Pagos");
            titulo.Format.Font.Size = 12;
            titulo.Format.Font.Bold = true;
            titulo.Format.SpaceBefore = Unit.FromCentimeter(0.6);
            titulo.Format.SpaceAfter = Unit.FromCentimeter(0.2);

            foreach (ComprobantePago p in v.Pagos)
                seccion.AddParagraph(p.Medio + ":  $ " + p.Importe.ToString(NegocioVenta.FormatoImporte));

            if (v.Vuelto > 0)
            {
                Paragraph vuelto = seccion.AddParagraph("Vuelto:  $ " + v.Vuelto.ToString(NegocioVenta.FormatoImporte));
                vuelto.Format.Font.Bold = true;
            }
        }

        private static void AgregarPie(Section seccion, ComprobanteVista v)
        {
            Paragraph nota = seccion.AddParagraph(v.Codigo == NegocioComprobante.Ticket
                ? "Ticket interno. No es válido como factura."
                : "Comprobante interno de TiendaUNNE. No incluye CAE de ARCA (AFIP).");
            nota.Format.Font.Size = 8;
            nota.Format.Font.Color = ColorGris;
            nota.Format.SpaceBefore = Unit.FromCentimeter(0.8);

            Paragraph pie = seccion.Footers.Primary.AddParagraph();
            pie.Format.Font.Size = 8;
            pie.Format.Font.Color = ColorGris;
            pie.Format.Alignment = ParagraphAlignment.Center;
            pie.AddText("TiendaUNNE · " + v.Nombre + " " + v.NumeroFormateado);
        }

        private static void Derecha(Cell celda, string texto)
        {
            celda.AddParagraph(texto).Format.Alignment = ParagraphAlignment.Right;
        }
    }
}
