using System.ComponentModel;
using System.Globalization;
using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Seguridad;
using InventarioMVP.Negocio.Servicios;

namespace InventarioMVP.Vistas
{
    /// <summary>Dashboard principal: saludo, rol, fecha y hora en vivo, tarjetas, gráfico y listados reales de la base de datos.</summary>
    public class UcInicio : VistaBase, IInicioView
    {
        private static readonly CultureInfo Es = new("es-PA");
        private static readonly (string Texto, PeriodoIngresos Valor)[] Periodos =
        {
            ("Hoy", PeriodoIngresos.Hoy), ("Esta semana", PeriodoIngresos.Semana), ("Este mes", PeriodoIngresos.Mes),
            ("Este año", PeriodoIngresos.Anio), ("Todo", PeriodoIngresos.Todo)
        };

        private readonly Usuario _usuario;
        private readonly Label _lblSaludo = new() { AutoSize = true, Font = new Font("Segoe UI Semibold", 20F), ForeColor = Estilos.Menu };
        private readonly Label _lblFecha = new() { AutoSize = true, Font = new Font("Segoe UI", 11F), ForeColor = Estilos.Neutro, Margin = new Padding(12, 6, 0, 0) };
        private readonly Label _lblHora = new() { Dock = DockStyle.Fill, Font = new Font("Segoe UI Light", 30F), ForeColor = Estilos.Menu, TextAlign = ContentAlignment.MiddleRight };
        private readonly System.Windows.Forms.Timer _reloj = new() { Interval = 1000 };

        private readonly TarjetaEstadistica _tProductos = new("Productos", "package", Estilos.Primario);
        private readonly TarjetaEstadistica _tClientes = new("Clientes", "contact", Color.FromArgb(124, 58, 237));
        private readonly TarjetaEstadistica _tDinero;
        private readonly ComboBox _cmbPeriodo = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 118, Font = new Font("Segoe UI", 9F) };

        private readonly GraficoBarras _grafico = new() { Dock = DockStyle.Fill };
        private readonly DataGridView _gridVentas = new();
        private readonly DataGridView _gridVendidos = new();
        private readonly DataGridView _gridStock = new();
        private readonly DataGridView _gridActividad = new();
        private readonly TableLayoutPanel _filaInferior = new() { Dock = DockStyle.Fill, ColumnCount = 3, Margin = new Padding(0) };
        private readonly Panel _panelActividad;
        private bool _cargando;

        public event EventHandler? CargarSolicitado;
        public event EventHandler? PeriodoCambiado;

        public PeriodoIngresos Periodo => _cmbPeriodo.SelectedIndex >= 0 ? Periodos[_cmbPeriodo.SelectedIndex].Valor : PeriodoIngresos.Mes;

        public UcInicio(Usuario usuario)
        {
            _usuario = usuario;
            Padding = new Padding(0);
            bool admin = usuario.EsAdministrador;
            _tDinero = new TarjetaEstadistica(admin ? "Dinero ganado" : "Mis ventas", "trending-up", Estilos.Exito);

            _cmbPeriodo.Items.AddRange(Periodos.Select(p => (object)p.Texto).ToArray());
            _cmbPeriodo.SelectedIndex = 2;
            _tDinero.AgregarSuperiorDerecha(_cmbPeriodo);
            _tDinero.Detalle = "Ingresos por ventas (USD)";

            ConfigurarGrid(_gridVentas,
                Estilos.Columna(nameof(Venta.Numero), "Número", 22), Estilos.Columna(nameof(Venta.ClienteNombre), "Cliente", 40),
                Estilos.ColumnaDinero(nameof(Venta.Total), "Total", 28));
            ConfigurarGrid(_gridVendidos,
                Estilos.Columna(nameof(ProductoVendido.Nombre), "Producto", 55),
                Estilos.Columna(nameof(ProductoVendido.Unidades), "Unid.", 16, "N0", DataGridViewContentAlignment.MiddleRight),
                Estilos.ColumnaDinero(nameof(ProductoVendido.Ingresos), "Ingresos", 29));
            ConfigurarGrid(_gridStock,
                Estilos.Columna(nameof(Producto.Nombre), "Producto", 55),
                Estilos.Columna(nameof(Producto.Stock), "Stock", 22, "N0", DataGridViewContentAlignment.MiddleRight),
                Estilos.Columna(nameof(Producto.StockMinimo), "Mínimo", 23, "N0", DataGridViewContentAlignment.MiddleRight));
            ConfigurarGrid(_gridActividad,
                Estilos.Columna(nameof(Actividad.Fecha), "Fecha", 26, "dd/MM HH:mm"),
                Estilos.Columna(nameof(Actividad.UsuarioNombre), "Usuario", 26),
                Estilos.Columna(nameof(Actividad.Detalle), "Detalle", 48));

            // --- Encabezado: saludo, rol y fecha + reloj ---
            var etiquetaRol = new Label
            {
                Text = "  " + Roles.Nombre(usuario.RolId) + "  ", AutoSize = true, BackColor = Color.FromArgb(219, 234, 254),
                ForeColor = Estilos.Primario, Font = new Font("Segoe UI Semibold", 9F), Margin = new Padding(0, 8, 0, 0), Padding = new Padding(2, 3, 2, 3)
            };
            var filaRol = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 34, WrapContents = false };
            filaRol.Controls.Add(etiquetaRol);
            filaRol.Controls.Add(_lblFecha);
            var izquierda = new Panel { Dock = DockStyle.Fill };
            izquierda.Controls.Add(filaRol);
            izquierda.Controls.Add(_lblSaludo);
            _lblSaludo.Dock = DockStyle.Top;
            _lblSaludo.AutoSize = false;
            _lblSaludo.Height = 44;

            var encabezado = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Margin = new Padding(0) };
            encabezado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
            encabezado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
            encabezado.Controls.Add(izquierda, 0, 0);
            encabezado.Controls.Add(_lblHora, 1, 0);

            // --- Tarjetas ---
            var tarjetas = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, Margin = new Padding(0) };
            for (int i = 0; i < 3; i++) tarjetas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            tarjetas.Controls.Add(_tProductos, 0, 0);
            tarjetas.Controls.Add(_tClientes, 1, 0);
            tarjetas.Controls.Add(_tDinero, 2, 0);
            _tDinero.Margin = new Padding(0);

            // --- Gráfico y últimas ventas ---
            var medio = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Margin = new Padding(0) };
            medio.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            medio.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            medio.Controls.Add(Estilos.Titulado(admin ? "Ventas de los últimos 14 días" : "Mis ventas de los últimos 14 días", _grafico), 0, 0);
            medio.Controls.Add(Estilos.Titulado(admin ? "Últimas ventas" : "Mis últimas ventas", _gridVentas), 1, 0);

            // --- Listados inferiores ---
            _filaInferior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            _filaInferior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            _filaInferior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            _panelActividad = Estilos.Titulado("Actividad reciente del sistema", _gridActividad);
            _filaInferior.Controls.Add(Estilos.Titulado("Productos más vendidos", _gridVendidos), 0, 0);
            _filaInferior.Controls.Add(Estilos.Titulado("Productos con poco inventario", _gridStock), 1, 0);
            if (admin) _filaInferior.Controls.Add(_panelActividad, 2, 0);
            else
            {
                _filaInferior.ColumnStyles[0].Width = 50;
                _filaInferior.ColumnStyles[1].Width = 50;
                _filaInferior.ColumnStyles[2].Width = 0;
            }

            var rejilla = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(20, 14, 8, 6) };
            rejilla.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
            rejilla.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
            rejilla.RowStyles.Add(new RowStyle(SizeType.Percent, 52));
            rejilla.RowStyles.Add(new RowStyle(SizeType.Percent, 48));
            rejilla.Controls.Add(encabezado, 0, 0);
            rejilla.Controls.Add(tarjetas, 0, 1);
            rejilla.Controls.Add(medio, 0, 2);
            rejilla.Controls.Add(_filaInferior, 0, 3);

            // Con pantallas pequeñas aparece la barra de desplazamiento en lugar de comprimir el contenido
            var desplazable = new Panel { Dock = DockStyle.Fill, AutoScroll = true, AutoScrollMinSize = new Size(940, 800) };
            desplazable.Controls.Add(rejilla);
            Controls.Add(desplazable);

            ActualizarReloj();
            _reloj.Tick += (_, _) => ActualizarReloj();
            _reloj.Start();

            Load += (_, _) => Disparar(CargarSolicitado);
            _cmbPeriodo.SelectedIndexChanged += (_, _) => { if (!_cargando) Disparar(PeriodoCambiado); };
            _gridStock.CellFormatting += (_, e) =>
            {
                if (e.RowIndex >= 0 && _gridStock.Rows[e.RowIndex].DataBoundItem is Producto p && p.Stock == 0)
                    e.CellStyle!.ForeColor = Estilos.Peligro;
            };
        }

        private static void ConfigurarGrid(DataGridView g, params DataGridViewColumn[] columnas)
        {
            Estilos.ConfigurarGrid(g);
            g.Columns.AddRange(columnas);
            g.ColumnHeadersHeight = 30;
            g.RowTemplate.Height = 28;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F);
            g.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            g.ScrollBars = ScrollBars.Vertical;
            g.TabStop = false;
            g.SelectionChanged += (_, _) => g.ClearSelection();
        }

        public void MostrarDatos(DashboardDatos d)
        {
            _cargando = true;
            _tProductos.Valor = d.TotalProductos.ToString("N0");
            _tProductos.Detalle = d.ProductosStockBajo == 0 ? "Sin productos con stock bajo" : $"{d.ProductosStockBajo} con stock bajo";
            _tProductos.ColorDetalle = d.ProductosStockBajo == 0 ? Estilos.Neutro : Estilos.Peligro;
            _tClientes.Valor = d.TotalClientes.ToString("N0");
            _tClientes.Detalle = d.ClientesNuevos == 0 ? "Sin clientes nuevos (30 días)" : $"+{d.ClientesNuevos} nuevos en 30 días";
            _tClientes.ColorDetalle = d.ClientesNuevos == 0 ? Estilos.Neutro : Estilos.Exito;
            _cargando = false;
            MostrarIngresos(d.Ingresos);

            _grafico.Datos = d.VentasPorDia.Select(p => (p.Dia.ToString("dd/MM", Es), p.Total)).ToList();
            _gridVentas.DataSource = new BindingList<Venta>(d.UltimasVentas);
            _gridVendidos.DataSource = new BindingList<ProductoVendido>(d.MasVendidos);
            _gridStock.DataSource = new BindingList<Producto>(d.StockBajo);
            _gridActividad.DataSource = new BindingList<Actividad>(d.Actividad);
            foreach (var g in new[] { _gridVentas, _gridVendidos, _gridStock, _gridActividad }) g.ClearSelection();
        }

        public void MostrarIngresos(decimal total) => _tDinero.Valor = total.ToString("C2", Estilos.Usd);

        private void ActualizarReloj()
        {
            var ahora = DateTime.Now;
            string saludo = ahora.Hour < 12 ? "Buenos días" : ahora.Hour < 19 ? "Buenas tardes" : "Buenas noches";
            _lblSaludo.Text = $"{saludo}, {_usuario.NombreCompleto}";
            _lblHora.Text = ahora.ToString("HH:mm:ss");
            var fecha = ahora.ToString("dddd, d 'de' MMMM 'de' yyyy", Es);
            _lblFecha.Text = char.ToUpper(fecha[0]) + fecha[1..];
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _reloj.Dispose();
            base.Dispose(disposing);
        }
    }
}
