using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Servicios;

namespace InventarioMVP.Vistas
{
    public interface IReportesView
    {
        event EventHandler GenerarSolicitado;
        event EventHandler ExcelSolicitado;
        event EventHandler PdfSolicitado;

        TipoReporte Tipo { get; }
        DateTime Desde { get; }
        DateTime Hasta { get; }

        void MostrarTabla(TablaReporte tabla);

        /// <summary>Muestra el cuadro de guardar archivo. Devuelve la ruta elegida o null si se canceló.</summary>
        string? PedirRuta(string nombreSugerido, string extension, string descripcion);

        void MostrarError(string mensaje);
        void MostrarInfo(string mensaje);
    }
}
