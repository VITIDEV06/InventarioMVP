using ClosedXML.Excel;
using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Servicios;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InventarioMVP.Negocio.Reportes
{
    /// <summary>Exporta una tabla de reporte a un archivo.</summary>
    public interface IExportadorReporte
    {
        string Extension { get; }
        void Exportar(TablaReporte tabla, string rutaArchivo, string subtitulo);
    }

    public class ExportadorExcel : IExportadorReporte
    {
        public string Extension => ".xlsx";

        public void Exportar(TablaReporte tabla, string rutaArchivo, string subtitulo)
        {
            using var libro = new XLWorkbook();
            string nombreHoja = new string(tabla.Titulo.Where(c => !"[]:*?/\\".Contains(c)).ToArray());
            var hoja = libro.Worksheets.Add(nombreHoja.Length > 31 ? nombreHoja[..31] : nombreHoja);

            hoja.Cell(1, 1).Value = tabla.Titulo;
            hoja.Cell(1, 1).Style.Font.Bold = true;
            hoja.Cell(1, 1).Style.Font.FontSize = 14;
            hoja.Cell(2, 1).Value = subtitulo;
            hoja.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;

            const int filaEncabezado = 4;
            for (int c = 0; c < tabla.Columnas.Count; c++)
            {
                var celda = hoja.Cell(filaEncabezado, c + 1);
                celda.Value = tabla.Columnas[c];
                celda.Style.Font.Bold = true;
                celda.Style.Font.FontColor = XLColor.White;
                celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E293B");
            }

            for (int f = 0; f < tabla.Filas.Count; f++)
                for (int c = 0; c < tabla.Columnas.Count; c++)
                {
                    var celda = hoja.Cell(filaEncabezado + 1 + f, c + 1);
                    switch (tabla.Filas[f][c])
                    {
                        case null: break;
                        case DateTime d: celda.Value = d; celda.Style.DateFormat.Format = d.TimeOfDay == TimeSpan.Zero ? "dd/MM/yyyy" : "dd/MM/yyyy HH:mm"; break;
                        case decimal m: celda.Value = m; celda.Style.NumberFormat.Format = "#,##0.00"; break;
                        case double db: celda.Value = db; celda.Style.NumberFormat.Format = "#,##0.00"; break;
                        case long l: celda.Value = l; break;
                        case int i: celda.Value = i; break;
                        default: celda.Value = Convert.ToString(tabla.Filas[f][c]) ?? ""; break;
                    }
                }

            hoja.Columns().AdjustToContents();
            hoja.SheetView.FreezeRows(filaEncabezado);
            libro.SaveAs(rutaArchivo);
        }
    }

    public class ExportadorPdf : IExportadorReporte
    {
        static ExportadorPdf() => QuestPDF.Settings.License = LicenseType.Community;

        public string Extension => ".pdf";

        public void Exportar(TablaReporte tabla, string rutaArchivo, string subtitulo)
        {
            Document.Create(documento =>
            {
                documento.Page(pagina =>
                {
                    pagina.Size(PageSizes.A4.Landscape());
                    pagina.Margin(28);
                    pagina.DefaultTextStyle(t => t.FontSize(9));

                    pagina.Header().Column(col =>
                    {
                        col.Item().Text(tabla.Titulo).FontSize(16).Bold().FontColor("#1E293B");
                        col.Item().Text(subtitulo).FontSize(9).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingBottom(8);
                    });

                    pagina.Content().Table(t =>
                    {
                        t.ColumnsDefinition(c => { foreach (var _ in tabla.Columnas) c.RelativeColumn(); });
                        t.Header(h =>
                        {
                            foreach (var columna in tabla.Columnas)
                                h.Cell().Background("#1E293B").Padding(4).Text(columna).Bold().FontColor(Colors.White);
                        });
                        for (int f = 0; f < tabla.Filas.Count; f++)
                            foreach (var valor in tabla.Filas[f])
                            {
                                var fondo = f % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                                t.Cell().Background(fondo).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(3).Text(Formato.Celda(valor));
                            }
                    });

                    pagina.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ").FontSize(8);
                        x.CurrentPageNumber().FontSize(8);
                        x.Span(" de ").FontSize(8);
                        x.TotalPages().FontSize(8);
                    });
                });
            }).GeneratePdf(rutaArchivo);
        }
    }
}
