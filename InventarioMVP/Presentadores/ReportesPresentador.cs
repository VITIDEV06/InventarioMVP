using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Servicios;
using InventarioMVP.Vistas;

namespace InventarioMVP.Presentadores
{
    public class ReportesPresentador
    {
        private readonly IReportesView _vista;
        private readonly ReporteService _reportes;
        private TablaReporte? _actual;

        public ReportesPresentador(IReportesView vista, ReporteService reportes)
        {
            _vista = vista;
            _reportes = reportes;
            _vista.GenerarSolicitado += (_, _) => Ejecutar(Generar);
            _vista.ExcelSolicitado += (_, _) => Ejecutar(() => Exportar(".xlsx", "Libro de Excel"));
            _vista.PdfSolicitado += (_, _) => Ejecutar(() => Exportar(".pdf", "Documento PDF"));
        }

        private void Generar()
        {
            _actual = _reportes.Generar(_vista.Tipo, _vista.Desde, _vista.Hasta);
            _vista.MostrarTabla(_actual);
        }

        private void Exportar(string extension, string descripcion)
        {
            Generar(); // exporta siempre los datos actualizados con los filtros visibles
            var tabla = _actual!;
            if (tabla.Filas.Count == 0) throw new ValidacionException("El reporte no tiene datos para exportar.");

            string sugerido = $"{tabla.Titulo.Replace(' ', '_')}_{DateTime.Now:yyyyMMdd}";
            string? ruta = _vista.PedirRuta(sugerido, extension, descripcion);
            if (ruta == null) return;

            _reportes.Exportar(tabla, ruta, _vista.Desde, _vista.Hasta);
            _vista.MostrarInfo("Reporte exportado: " + ruta);
        }

        private void Ejecutar(Action accion) => Ejecutor.Ejecutar(accion, _vista.MostrarError);
    }
}
