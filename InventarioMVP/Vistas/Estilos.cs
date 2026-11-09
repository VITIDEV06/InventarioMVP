using System.Globalization;

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
        public static readonly Color Exito = Color.FromArgb(22, 163, 74);
        public static readonly Color Aviso = Color.FromArgb(217, 119, 6);
        public static readonly Color Neutro = Color.FromArgb(100, 116, 139);
        public static readonly Color Borde = Color.FromArgb(226, 232, 240);
        public static readonly Font Fuente = new("Segoe UI", 10F);
        public static readonly Font FuenteNegrita = new("Segoe UI Semibold", 10F);
        public static readonly Font FuenteTitulo = new("Segoe UI Semibold", 16F);
        public static readonly CultureInfo Usd = new("en-US");

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
                Font = Fuente,
                Margin = new Padding(0, 0, 8, 6)
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

        public static TextBox Caja(int maxLength = 100) => new() { Dock = DockStyle.Fill, MaxLength = maxLength, Font = Fuente };

        /// <summary>Tabla de dos columnas (etiqueta, campo) para formularios.</summary>
        public static TableLayoutPanel TablaFormulario(int anchoEtiqueta = 120)
        {
            var t = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Padding = new Padding(0) };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, anchoEtiqueta));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            return t;
        }

        public static void Fila(TableLayoutPanel tabla, string etiqueta, Control campo)
        {
            tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tabla.Controls.Add(Etiqueta(etiqueta));
            campo.Margin = new Padding(0, 4, 0, 4);
            campo.Dock = DockStyle.Fill;
            tabla.Controls.Add(campo);
        }

        /// <summary>Etiqueta para barras de herramientas (alineada arriba, junto a botones y selectores).</summary>
        public static Label EtiquetaBarra(string texto) => new()
        {
            Text = texto, AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Left, Font = Fuente, Margin = new Padding(0, 9, 6, 0)
        };

        /// <summary>
        /// Barra horizontal de controles (botones, filtros) de una sola fila. Cada pantalla reparte sus
        /// controles en varias barras para que quepan incluso con la ventana en su tamaño mínimo.
        /// </summary>
        public static FlowLayoutPanel Barra(params Control[] controles)
        {
            var f = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 46, Padding = new Padding(0, 4, 0, 0), WrapContents = false };
            foreach (var c in controles) if (c is TextBox or ComboBox) c.Margin = new Padding(0, 5, 8, 0);
            f.Controls.AddRange(controles);
            return f;
        }

        /// <summary>Tarjeta blanca con título que envuelve un contenido (típicamente una tabla).</summary>
        public static Panel Titulado(string titulo, Control contenido)
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(12, 8, 12, 12), Margin = new Padding(0, 0, 12, 12) };
            contenido.Dock = DockStyle.Fill;
            p.Controls.Add(contenido);
            p.Controls.Add(new Label
            {
                Text = titulo, Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI Semibold", 11F), ForeColor = Menu
            });
            return p;
        }

        public static void ConfigurarGrid(DataGridView g)
        {
            g.Dock = DockStyle.Fill;
            g.BackgroundColor = Color.White;
            g.BorderStyle = BorderStyle.None;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.GridColor = Borde;
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
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Menu;
            g.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10F);
            g.ColumnHeadersHeight = 36;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            g.DefaultCellStyle.SelectionForeColor = Menu;
            g.DefaultCellStyle.Font = Fuente;
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            g.RowTemplate.Height = 30;
        }

        public static DataGridViewTextBoxColumn Columna(string propiedad, string titulo, float peso = 100,
            string? formato = null, DataGridViewContentAlignment? alineacion = null)
        {
            var c = new DataGridViewTextBoxColumn { DataPropertyName = propiedad, HeaderText = titulo, FillWeight = peso };
            if (formato != null) c.DefaultCellStyle.Format = formato;
            if (alineacion != null) c.DefaultCellStyle.Alignment = alineacion.Value;
            return c;
        }

        public static DataGridViewTextBoxColumn ColumnaDinero(string propiedad, string titulo, float peso = 80)
        {
            var c = Columna(propiedad, titulo, peso, "C2", DataGridViewContentAlignment.MiddleRight);
            c.DefaultCellStyle.FormatProvider = Usd;
            return c;
        }

        public static DateTimePicker Fecha(DateTime valor) => new()
        {
            Format = DateTimePickerFormat.Short, Value = valor, Width = 120, Font = Fuente, Margin = new Padding(0, 4, 10, 0)
        };
    }
}
