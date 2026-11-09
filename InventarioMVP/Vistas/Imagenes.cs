using System.Drawing.Drawing2D;
using System.Reflection;

namespace InventarioMVP.Vistas
{
    /// <summary>Logo e icono de la aplicación, leídos de los recursos incrustados (Resources/Images).</summary>
    internal static class Imagenes
    {
        private static readonly Assembly Ensamblado = typeof(Imagenes).Assembly;
        private static readonly Dictionary<int, Bitmap> CacheLogo = new();
        private static Icon? _icono;

        /// <summary>Icono para la barra de título y la barra de tareas de cada ventana.</summary>
        public static Icon Icono => _icono ??= new Icon(Abrir(".app.ico"));

        /// <summary>Logo cuadrado reescalado con alta calidad a px x px (con transparencia).</summary>
        public static Bitmap Logo(int px)
        {
            if (CacheLogo.TryGetValue(px, out var existente)) return existente;

            using var original = new Bitmap(Abrir(".logo.png"));
            var destino = new Bitmap(px, px, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(destino))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(original, 0, 0, px, px);
            }
            return CacheLogo[px] = destino;
        }

        private static Stream Abrir(string sufijo)
        {
            string nombre = Ensamblado.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith(sufijo, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException("Recurso no encontrado: " + sufijo);
            return Ensamblado.GetManifestResourceStream(nombre)!;
        }
    }
}
