using InventarioMVP.Datos.Repositorios;
using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Reportes;
using InventarioMVP.Negocio.Seguridad;

namespace InventarioMVP.Negocio.Servicios
{
    public enum TipoReporte
    {
        Ventas, IngresosDia, IngresosSemana, IngresosMes, IngresosAnio,
        MasVendidos, MenosVendidos, InventarioDisponible, StockBajo, Movimientos, VentasPorUsuario
    }

    public class ReporteService
    {
        private readonly IReporteRepositorio _reportes;
        private readonly IActividadRepositorio _actividad;
        private readonly SesionUsuario _sesion;
        private readonly ExportadorExcel _excel;
        private readonly ExportadorPdf _pdf;

        public ReporteService(IReporteRepositorio reportes, IActividadRepositorio actividad, SesionUsuario sesion,
            ExportadorExcel excel, ExportadorPdf pdf)
        {
            _reportes = reportes;
            _actividad = actividad;
            _sesion = sesion;
            _excel = excel;
            _pdf = pdf;
        }

        public TablaReporte Generar(TipoReporte tipo, DateTime desde, DateTime hasta)
        {
            _sesion.Exigir(Permisos.ReportesVer);
            Validaciones.Fechas(desde, hasta);
            DateTime d = desde.Date, h = hasta.Date.AddDays(1);
            return tipo switch
            {
                TipoReporte.Ventas => _reportes.Ventas(d, h),
                TipoReporte.IngresosDia => _reportes.Ingresos("DIA", d, h),
                TipoReporte.IngresosSemana => _reportes.Ingresos("SEMANA", d, h),
                TipoReporte.IngresosMes => _reportes.Ingresos("MES", d, h),
                TipoReporte.IngresosAnio => _reportes.Ingresos("ANIO", d, h),
                TipoReporte.MasVendidos => _reportes.ProductosVendidos(true, d, h),
                TipoReporte.MenosVendidos => _reportes.ProductosVendidos(false, d, h),
                TipoReporte.InventarioDisponible => _reportes.InventarioDisponible(),
                TipoReporte.StockBajo => _reportes.StockBajo(),
                TipoReporte.Movimientos => _reportes.Movimientos(d, h),
                TipoReporte.VentasPorUsuario => _reportes.VentasPorUsuario(d, h),
                _ => throw new ValidacionException("Reporte no válido.")
            };
        }

        /// <summary>Exporta el reporte a PDF o Excel según la extensión del archivo (.pdf o .xlsx).</summary>
        public void Exportar(TablaReporte tabla, string rutaArchivo, DateTime desde, DateTime hasta)
        {
            _sesion.Exigir(Permisos.ReportesVer);
            if (tabla.Filas.Count == 0) throw new ValidacionException("El reporte no tiene datos para exportar.");
            IExportadorReporte exportador = Path.GetExtension(rutaArchivo).ToLowerInvariant() switch
            {
                ".pdf" => _pdf,
                ".xlsx" => _excel,
                _ => throw new ValidacionException("Formato no admitido. Use .pdf o .xlsx.")
            };
            string subtitulo = $"Período: {Formato.Fecha(desde)} al {Formato.Fecha(hasta)}  ·  Generado: {Formato.FechaHora(DateTime.Now)}  ·  Por: {_sesion.Usuario.NombreCompleto}";
            try { exportador.Exportar(tabla, rutaArchivo, subtitulo); }
            catch (IOException) { throw new ValidacionException("No se pudo escribir el archivo. Verifique que no esté abierto en otro programa."); }
            _actividad.Registrar(_sesion.UsuarioId, "REPORTE_EXPORTADO", $"{tabla.Titulo} ({exportador.Extension})");
        }
    }
}
