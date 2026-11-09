using System.Globalization;

namespace InventarioMVP.Negocio.Servicios
{
    /// <summary>Formatos de presentación compartidos (dinero siempre en USD).</summary>
    public static class Formato
    {
        private static readonly CultureInfo Usd = new("en-US");
        private static readonly CultureInfo Es = new("es-PA");

        public static string Dinero(decimal valor) => valor.ToString("C2", Usd);

        public static string Fecha(DateTime fecha) => fecha.ToString("dd/MM/yyyy", Es);

        public static string FechaHora(DateTime fecha) => fecha.ToString("dd/MM/yyyy HH:mm", Es);

        /// <summary>Texto de una celda de reporte (para pantalla y exportaciones).</summary>
        public static string Celda(object? valor) => valor switch
        {
            null => "",
            DateTime f => f.TimeOfDay == TimeSpan.Zero ? Fecha(f) : FechaHora(f),
            decimal d => d.ToString("N2", Usd),
            double d => d.ToString("N2", Usd),
            _ => valor.ToString() ?? ""
        };
    }
}
