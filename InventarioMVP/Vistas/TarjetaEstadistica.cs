namespace InventarioMVP.Vistas
{
    /// <summary>Tarjeta de estadística: icono en círculo, título, valor grande y detalle.</summary>
    public class TarjetaEstadistica : Panel
    {
        private readonly Color _acento;
        private readonly Bitmap _icono;
        private readonly Label _titulo = new() { AutoSize = false, Location = new Point(76, 18), Size = new Size(160, 22), ForeColor = Estilos.Neutro, Font = new Font("Segoe UI Semibold", 9F) };
        private readonly Label _valor = new() { AutoSize = false, Location = new Point(20, 62), Height = 40, Font = new Font("Segoe UI Semibold", 22F), ForeColor = Estilos.Menu, Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
        private readonly Label _detalle = new() { AutoSize = false, Location = new Point(20, 106), Height = 22, ForeColor = Estilos.Neutro, Font = new Font("Segoe UI", 9F), Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };

        public TarjetaEstadistica(string titulo, string icono, Color acento)
        {
            _acento = acento;
            _icono = Iconos.Obtener(icono, acento, 24);
            BackColor = Color.White;
            Dock = DockStyle.Fill;
            Margin = new Padding(0, 0, 14, 0);
            MinimumSize = new Size(220, 138);
            DoubleBuffered = true;
            _titulo.Text = titulo.ToUpperInvariant();
            _valor.Width = 260;
            _detalle.Width = 260;
            Controls.Add(_titulo);
            Controls.Add(_valor);
            Controls.Add(_detalle);
            Resize += (_, _) => { _valor.Width = Width - 40; _detalle.Width = Width - 40; };
        }

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string Valor { set => _valor.Text = value; }

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string Detalle { set => _detalle.Text = value; }

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Color ColorDetalle { set => _detalle.ForeColor = value; }

        /// <summary>Coloca un control (por ejemplo un selector de período) en la esquina superior derecha.</summary>
        public void AgregarSuperiorDerecha(Control control)
        {
            control.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            control.Location = new Point(Width - control.Width - 14, 16);
            Controls.Add(control);
            control.BringToFront();
            _titulo.Width = 110;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var barra = new SolidBrush(_acento)) g.FillRectangle(barra, 0, 0, 4, Height);
            using (var fondo = new SolidBrush(Color.FromArgb(28, _acento))) g.FillEllipse(fondo, 20, 12, 44, 44);
            g.DrawImage(_icono, 20 + (44 - _icono.Width) / 2, 12 + (44 - _icono.Height) / 2, _icono.Width, _icono.Height);
            using var borde = new Pen(Estilos.Borde);
            g.DrawRectangle(borde, 0, 0, Width - 1, Height - 1);
        }
    }
}
