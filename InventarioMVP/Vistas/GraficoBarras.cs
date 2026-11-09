using System.Drawing.Drawing2D;

namespace InventarioMVP.Vistas
{
    /// <summary>Gráfico de barras sencillo (GDI+) para mostrar valores monetarios por día.</summary>
    public class GraficoBarras : Control
    {
        private readonly ToolTip _tip = new();
        private IList<(string Etiqueta, decimal Valor)> _datos = Array.Empty<(string, decimal)>();
        private int _barraActiva = -1;

        public GraficoBarras()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 8.5F);
            MinimumSize = new Size(260, 160);
        }

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public IList<(string Etiqueta, decimal Valor)> Datos
        {
            get => _datos;
            set { _datos = value; _barraActiva = -1; Invalidate(); }
        }

        private Rectangle AreaGrafico => new(60, 12, Math.Max(10, Width - 60 - 14), Math.Max(10, Height - 12 - 30));

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);
            var area = AreaGrafico;
            decimal maximo = _datos.Count == 0 ? 0 : _datos.Max(d => d.Valor);
            decimal tope = EscalaAgradable(maximo);

            using var lineaFondo = new Pen(Estilos.Borde);
            using var pincelTexto = new SolidBrush(Estilos.Neutro);
            var formatoEje = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
            for (int i = 0; i <= 4; i++)
            {
                float y = area.Bottom - area.Height * i / 4f;
                g.DrawLine(lineaFondo, area.Left, y, area.Right, y);
                g.DrawString((tope * i / 4).ToString("$#,##0", Estilos.Usd), Font, pincelTexto, new RectangleF(0, y - 10, area.Left - 6, 20), formatoEje);
            }

            if (_datos.Count == 0 || maximo == 0)
            {
                var centrado = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("Sin ventas en este período", new Font("Segoe UI", 10F), pincelTexto, area, centrado);
                if (_datos.Count == 0) return;
            }

            float ranura = area.Width / (float)_datos.Count;
            float ancho = Math.Min(ranura * 0.62f, 46f);
            var formatoEtiqueta = new StringFormat { Alignment = StringAlignment.Center };
            for (int i = 0; i < _datos.Count; i++)
            {
                float x = area.Left + ranura * i + (ranura - ancho) / 2;
                float alto = tope == 0 ? 0 : (float)(_datos[i].Valor / tope) * area.Height;
                if (alto > 0)
                {
                    using var pincel = new SolidBrush(i == _barraActiva ? Color.FromArgb(29, 78, 216) : Estilos.Primario);
                    var barra = new RectangleF(x, area.Bottom - alto, ancho, alto);
                    using var ruta = Redondeado(barra, 4);
                    g.FillPath(pincel, ruta);
                }
                g.DrawString(_datos[i].Etiqueta, Font, pincelTexto, new RectangleF(area.Left + ranura * i - 6, area.Bottom + 6, ranura + 12, 18), formatoEtiqueta);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var area = AreaGrafico;
            int indice = _datos.Count == 0 || !area.Contains(e.Location)
                ? -1 : Math.Min(_datos.Count - 1, (int)((e.X - area.Left) / (area.Width / (float)_datos.Count)));
            if (indice == _barraActiva) return;
            _barraActiva = indice;
            _tip.SetToolTip(this, indice < 0 ? "" : $"{_datos[indice].Etiqueta}: {_datos[indice].Valor.ToString("C2", Estilos.Usd)}");
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_barraActiva != -1) { _barraActiva = -1; Invalidate(); }
        }

        /// <summary>Redondea el máximo a 1, 2, 2.5, 5 o 10 por una potencia de 10.</summary>
        private static decimal EscalaAgradable(decimal maximo)
        {
            if (maximo <= 0) return 100;
            decimal potencia = (decimal)Math.Pow(10, Math.Floor(Math.Log10((double)maximo)));
            foreach (var factor in new[] { 1m, 2m, 2.5m, 5m, 10m })
                if (maximo <= factor * potencia) return factor * potencia;
            return 10 * potencia;
        }

        private static GraphicsPath Redondeado(RectangleF r, float radio)
        {
            var p = new GraphicsPath();
            float d = Math.Min(radio * 2, Math.Min(r.Width, r.Height));
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddLine(r.Right, r.Bottom, r.X, r.Bottom);
            p.CloseFigure();
            return p;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _tip.Dispose();
            base.Dispose(disposing);
        }
    }
}
