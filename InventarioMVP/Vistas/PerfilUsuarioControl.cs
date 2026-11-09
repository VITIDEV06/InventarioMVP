namespace InventarioMVP.Vistas
{
    /// <summary>Perfil discreto para la barra superior: icono de persona en un círculo suave, nombre y rol.</summary>
    internal class PerfilUsuarioControl : Control
    {
        private const int Circulo = 34;
        private const int Separacion = 10;
        private readonly Bitmap _icono = Iconos.Obtener("user-round", Iconos.Sutil);
        private readonly Font _fuenteRol = new("Segoe UI Semibold", 8F);
        private readonly string _nombre;
        private readonly string _rol;

        public PerfilUsuarioControl(string nombre, string rol)
        {
            _nombre = nombre;
            _rol = rol;
            Font = Estilos.Fuente;
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw, true);
            // Ancho en píxeles "a 96 DPI": el formulario (DpiUnaware) lo escala de forma uniforme.
            int texto = Math.Max(TextRenderer.MeasureText(nombre, Font).Width, TextRenderer.MeasureText(rol, _fuenteRol).Width);
            Width = Circulo + Separacion + (int)Math.Ceiling(texto / Iconos.Escala) + 8;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int d = (int)Math.Round(Circulo * DeviceDpi / 96.0);
            int y = (Height - d) / 2;
            using (var relleno = new SolidBrush(Color.FromArgb(226, 232, 240)))
                g.FillEllipse(relleno, 0, y, d - 1, d - 1);
            g.DrawImage(_icono, (d - _icono.Width) / 2, y + (d - _icono.Height) / 2, _icono.Width, _icono.Height);

            int x = d + (int)Math.Round(Separacion * DeviceDpi / 96.0);
            const TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis;
            TextRenderer.DrawText(g, _nombre, Font, new Rectangle(x, y - 1, Width - x, d / 2 + 2), Estilos.Menu, flags | TextFormatFlags.Bottom);
            TextRenderer.DrawText(g, _rol, _fuenteRol, new Rectangle(x, y + d / 2 + 1, Width - x, d / 2), Estilos.Primario, flags | TextFormatFlags.Top);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _fuenteRol.Dispose();
            base.Dispose(disposing);
        }
    }
}
