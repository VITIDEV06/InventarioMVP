using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Servicios;

namespace InventarioMVP.Vistas
{
    /// <summary>Reportes del negocio con filtro de fechas y exportación a Excel o PDF.</summary>
    public class UcReportes : VistaBase, IReportesView
    {
        private static readonly (string Texto, TipoReporte Tipo, bool UsaFechas)[] Opciones =
        {
            ("Reporte de ventas", TipoReporte.Ventas, true),
            ("Ingresos por día", TipoReporte.IngresosDia, true),
            ("Ingresos por semana", TipoReporte.IngresosSemana, true),
            ("Ingresos por mes", TipoReporte.IngresosMes, true),
            ("Ingresos por año", TipoReporte.IngresosAnio, true),
            ("Productos más vendidos", TipoReporte.MasVendidos, true),
            ("Productos menos vendidos", TipoReporte.MenosVendidos, true),
            ("Inventario disponible", TipoReporte.InventarioDisponible, false),
            ("Productos con stock bajo", TipoReporte.StockBajo, false),
            ("Historial de movimientos", TipoReporte.Movimientos, true),
            ("Rendimiento de ventas por usuario", TipoReporte.VentasPorUsuario, true)
        };

        private readonly ComboBox _cmbReporte = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 290, Font = Estilos.Fuente, Margin = new Padding(0, 4, 12, 0) };
        private readonly DateTimePicker _dtpDesde = Estilos.Fecha(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
        private readonly DateTimePicker _dtpHasta = Estilos.Fecha(DateTime.Today);
        private readonly DataGridView _grid = new();
        private readonly Label _lblResumen = new() { Dock = DockStyle.Bottom, Height = 32, TextAlign = ContentAlignment.MiddleRight, Font = Estilos.FuenteNegrita, ForeColor = Estilos.Menu, Padding = new Padding(0, 0, 8, 0) };

        public event EventHandler? GenerarSolicitado;
        public event EventHandler? ExcelSolicitado;
        public event EventHandler? PdfSolicitado;

        public TipoReporte Tipo => Opciones[Math.Max(0, _cmbReporte.SelectedIndex)].Tipo;
        public DateTime Desde => _dtpDesde.Value.Date;
        public DateTime Hasta => _dtpHasta.Value.Date;

        public UcReportes()
        {
            Estilos.ConfigurarGrid(_grid);
            _grid.AllowUserToResizeColumns = true;
            foreach (var o in Opciones) _cmbReporte.Items.Add(o.Texto);
            _cmbReporte.SelectedIndex = 0;

            var btnGenerar = Estilos.Boton("Generar", Estilos.Primario, 120, "chart-no-axes-combined");
            var btnExcel = Estilos.Boton("Excel", Estilos.Exito, 110, "file-spreadsheet");
            var btnPdf = Estilos.Boton("PDF", Estilos.Peligro, 100, "file-text");
            var barra = Estilos.Barra(_cmbReporte, Estilos.EtiquetaBarra("Desde"), _dtpDesde, Estilos.EtiquetaBarra("Hasta"), _dtpHasta);
            var acciones = Estilos.Barra(btnGenerar, btnExcel, btnPdf);

            var tarjeta = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            tarjeta.Controls.Add(_grid);
            Controls.Add(tarjeta);
            Controls.Add(_lblResumen);
            Controls.Add(new Panel { Dock = DockStyle.Top, Height = 8 });
            Controls.Add(acciones);
            Controls.Add(barra);
            Controls.Add(Encabezado("Reportes", "Consulte y exporte la información del negocio"));

            _cmbReporte.SelectedIndexChanged += (_, _) =>
            {
                _dtpDesde.Enabled = _dtpHasta.Enabled = Opciones[_cmbReporte.SelectedIndex].UsaFechas;
                Disparar(GenerarSolicitado);
            };
            Load += (_, _) => Disparar(GenerarSolicitado);
            btnGenerar.Click += (_, _) => Disparar(GenerarSolicitado);
            btnExcel.Click += (_, _) => Disparar(ExcelSolicitado);
            btnPdf.Click += (_, _) => Disparar(PdfSolicitado);
        }

        public void MostrarTabla(TablaReporte tabla)
        {
            _grid.Columns.Clear();
            _grid.Rows.Clear();
            for (int c = 0; c < tabla.Columnas.Count; c++)
            {
                bool numerica = tabla.Filas.Select(f => f[c]).FirstOrDefault(v => v != null) is decimal or int or long or double;
                var columna = new DataGridViewTextBoxColumn { HeaderText = tabla.Columnas[c], SortMode = DataGridViewColumnSortMode.NotSortable };
                if (numerica) columna.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                _grid.Columns.Add(columna);
            }
            foreach (var fila in tabla.Filas)
                _grid.Rows.Add(fila.Select(Formato.Celda).Cast<object>().ToArray());
            _grid.ClearSelection();
            _lblResumen.Text = tabla.Filas.Count == 0 ? "Sin datos para el período seleccionado" : $"{tabla.Titulo}  ·  {tabla.Filas.Count} fila(s)";
        }

        public string? PedirRuta(string nombreSugerido, string extension, string descripcion)
        {
            using var dialogo = new SaveFileDialog
            {
                Title = "Exportar reporte",
                FileName = nombreSugerido,
                DefaultExt = extension.TrimStart('.'),
                Filter = $"{descripcion} (*{extension})|*{extension}",
                AddExtension = true,
                OverwritePrompt = true
            };
            return dialogo.ShowDialog(FindForm()) == DialogResult.OK ? dialogo.FileName : null;
        }
    }
}
