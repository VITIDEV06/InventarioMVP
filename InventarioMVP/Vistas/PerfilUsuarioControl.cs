namespace InventarioMVP.Vistas
{
    /// <summary>Perfil discreto para la barra superior: icono de persona en un círculo suave + nombre del usuario.</summary>
    internal class PerfilUsuarioControl : Control
    {
        private const int Circulo = 32;
        private const int Separacion = 10;
        private readonly Bitmap _icono = Iconos.Obtener("user-round", Iconos.Sutil);
        private readonly string _nombre;

        public PerfilUsuarioControl(string nombre)
        {
            _nombre = nombre;
            Font = Estilos.Fuente;
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw, true);
            // Ancho en píxeles "a 96 DPI": el formulario (AutoScaleMode.Dpi) lo escalará después.
            int texto = (int)Math.Ceiling(TextRenderer.MeasureText(nombre, Font).Width / Iconos.Escala);
            Width = Circulo + Separacion + texto + 8;
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

            var area = new Rectangle(d + (int)Math.Round(Separacion * DeviceDpi / 96.0), 0, Width, Height);
            TextRenderer.DrawText(g, _nombre, Font, area, Estilos.Menu,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);
        }
    }
}
