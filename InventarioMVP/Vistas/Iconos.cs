using System.Reflection;
using System.Runtime.InteropServices;
using Svg;

namespace InventarioMVP.Vistas
{
    /// <summary>
    /// Iconos Lucide (outline). Los SVG originales están incrustados en Resources/Icons;
    /// aquí se rasterizan al color y tamaño pedidos, ajustados al DPI del sistema.
    /// </summary>
    internal static class Iconos
    {
        public const int Tamano = 20;
        private const string Grosor = "1.75";

        public static readonly Color Normal = Color.FromArgb(148, 163, 184);   // gris neutro (sobre menú oscuro)
        public static readonly Color Sutil = Color.FromArgb(100, 116, 139);    // gris neutro (sobre fondo claro)
        public static readonly Color Blanco = Color.White;                     // sobre botones/elemento activo

        private static readonly Dictionary<string, Bitmap> Cache = new();
        private static readonly Assembly Ensamblado = typeof(Iconos).Assembly;

        [DllImport("user32.dll")]
        private static extern uint GetDpiForSystem();

        /// <summary>Factor de escala del sistema (1.0 = 96 DPI).</summary>
        public static double Escala => GetDpiForSystem() / 96.0;

        public static Bitmap Obtener(string nombre, Color color, int tamano = Tamano)
        {
            int px = (int)Math.Round(tamano * GetDpiForSystem() / 96.0);
            string clave = $"{nombre}|{color.ToArgb()}|{px}";
            if (Cache.TryGetValue(clave, out var existente)) return existente;

            string recurso = Ensamblado.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("." + nombre + ".svg", StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"Icono no encontrado: {nombre}");

            string svg;
            using (var s = Ensamblado.GetManifestResourceStream(recurso)!)
            using (var r = new StreamReader(s))
                svg = r.ReadToEnd();

            string hex = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            svg = svg.Replace("currentColor", hex).Replace("stroke-width=\"2\"", $"stroke-width=\"{Grosor}\"");

            var doc = SvgDocument.FromSvg<SvgDocument>(svg);
            var bmp = doc.Draw(px, px);
            Cache[clave] = bmp;
            return bmp;
        }
    }
}
