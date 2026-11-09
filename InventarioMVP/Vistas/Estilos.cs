namespace InventarioMVP.Vistas
{
    /// <summary>Colores y fábrica de controles comunes para mantener un aspecto uniforme.</summary>
    internal static class Estilos
    {
        public static readonly Color Menu = Color.FromArgb(30, 41, 59);
        public static readonly Color MenuActivo = Color.FromArgb(37, 99, 235);
        public static readonly Color MenuHover = Color.FromArgb(51, 65, 85);
        public static readonly Color Fondo = Color.FromArgb(241, 245, 249);
        public static readonly Color Primario = Color.FromArgb(37, 99, 235);
        public static readonly Color Peligro = Color.FromArgb(220, 38, 38);
        public static readonly Color Neutro = Color.FromArgb(100, 116, 139);
        public static readonly Font Fuente = new("Segoe UI", 10F);
        public static readonly Font FuenteTitulo = new("Segoe UI Semibold", 16F);

        public static Button Boton(string texto, Color color, int ancho = 110, string? icono = null)
        {
            var b = new Button
            {
                Text = texto,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = ancho,
                Height = 34,
                Cursor = Cursors.Hand,
                Font = Fuente
            };
            b.FlatAppearance.BorderSize = 0;
            if (icono != null)
            {
                b.Image = Iconos.Obtener(icono, Iconos.Blanco);
                b.ImageAlign = ContentAlignment.MiddleCenter;
                b.TextAlign = ContentAlignment.MiddleCenter;
                b.TextImageRelation = TextImageRelation.ImageBeforeText;
            }
            return b;
        }

        public static Label Etiqueta(string texto) => new()
        {
            Text = texto,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Font = Fuente,
            Margin = new Padding(0, 8, 8, 0)
        };

        public static void ConfigurarGrid(DataGridView g)
        {
            g.Dock = DockStyle.Fill;
            g.BackgroundColor = Color.White;
            g.BorderStyle = BorderStyle.None;
            g.ReadOnly = true;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.AllowUserToResizeRows = false;
            g.MultiSelect = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.RowHeadersVisible = false;
            g.AutoGenerateColumns = false;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersDefaultCellStyle.BackColor = Menu;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10F);
            g.ColumnHeadersHeight = 36;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.DefaultCellStyle.SelectionBackColor = Primario;
            g.DefaultCellStyle.Font = Fuente;
            g.RowTemplate.Height = 28;
        }

        public static DataGridViewTextBoxColumn Columna(string propiedad, string titulo, float peso = 100,
            string? formato = null, DataGridViewContentAlignment? alineacion = null)
        {
            var c = new DataGridViewTextBoxColumn
            {
                DataPropertyName = propiedad,
                HeaderText = titulo,
                FillWeight = peso
            };
            if (formato != null) c.DefaultCellStyle.Format = formato;
            if (alineacion != null) c.DefaultCellStyle.Alignment = alineacion.Value;
            return c;
        }
    }
}
