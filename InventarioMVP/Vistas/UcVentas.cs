using System.ComponentModel;
using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Servicios;

namespace InventarioMVP.Vistas
{
    public class UcVentas : VistaBase, IVentasView
    {
        private readonly DataGridView _grid = new();
        private readonly Label _encabezadoTitulo = new() { Dock = DockStyle.Fill, Font = Estilos.FuenteTitulo, ForeColor = Estilos.Menu, TextAlign = ContentAlignment.BottomLeft, Text = "Ventas" };
        private readonly Label _lblResumen = new() { Dock = DockStyle.Bottom, Height = 34, TextAlign = ContentAlignment.MiddleRight, Font = Estilos.FuenteNegrita, ForeColor = Estilos.Menu, Padding = new Padding(0, 0, 8, 0) };
        private readonly TextBox _txtBuscar = new() { Width = 220, Font = Estilos.Fuente, PlaceholderText = "N.º de venta o cliente", Margin = new Padding(0, 5, 10, 0) };
        private readonly DateTimePicker _dtpDesde = Estilos.Fecha(DateTime.Today.AddDays(-30));
        private readonly DateTimePicker _dtpHasta = Estilos.Fecha(DateTime.Today);
        private readonly Button _btnNueva = Estilos.Boton("Nueva venta", Estilos.Exito, 150, "shopping-cart");
        private readonly Panel _encabezado = new() { Dock = DockStyle.Top, Height = 62 };

        public event EventHandler? CargarSolicitado;
        public event EventHandler? BuscarSolicitado;
        public event EventHandler? NuevaVentaSolicitada;
        public event EventHandler? DetalleSolicitado;

        public string TextoBusqueda => _txtBuscar.Text;
        public DateTime Desde => _dtpDesde.Value.Date;
        public DateTime Hasta => _dtpHasta.Value.Date;
        public Venta? VentaSeleccionada => _grid.CurrentRow?.DataBoundItem as Venta;

        public UcVentas()
        {
            Estilos.ConfigurarGrid(_grid);
            _grid.Columns.AddRange(
                Estilos.Columna(nameof(Venta.Numero), "N.º de venta", 60),
                Estilos.Columna(nameof(Venta.Fecha), "Fecha y hora", 80, "dd/MM/yyyy HH:mm"),
                Estilos.Columna(nameof(Venta.ClienteNombre), "Cliente", 120),
                Estilos.Columna(nameof(Venta.UsuarioNombre), "Vendedor", 100),
                Estilos.ColumnaDinero(nameof(Venta.Total), "Total", 70));

            _encabezado.Controls.Add(_encabezadoTitulo);
            _encabezado.Controls.Add(new Label { Text = "Registre ventas y consulte el historial con sus detalles", Dock = DockStyle.Bottom, Height = 24, ForeColor = Estilos.Neutro });

            var btnDetalle = Estilos.Boton("Ver detalle", Estilos.Neutro, 130, "eye");
            var btnBuscar = Estilos.Boton("Buscar", Estilos.Primario, 105, "search");
            var barra = Estilos.Barra(_btnNueva, btnDetalle);
            var filtros = Estilos.Barra(_txtBuscar, Estilos.EtiquetaBarra("Desde"), _dtpDesde, Estilos.EtiquetaBarra("Hasta"), _dtpHasta, btnBuscar);

            var tarjeta = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            tarjeta.Controls.Add(_grid);
            Controls.Add(tarjeta);
            Controls.Add(_lblResumen);
            Controls.Add(new Panel { Dock = DockStyle.Top, Height = 8 });
            Controls.Add(filtros);
            Controls.Add(barra);
            Controls.Add(_encabezado);

            Load += (_, _) => Disparar(CargarSolicitado);
            btnBuscar.Click += (_, _) => Disparar(BuscarSolicitado);
            _txtBuscar.KeyDown += (_, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                Disparar(BuscarSolicitado);
            };
            _btnNueva.Click += (_, _) => Disparar(NuevaVentaSolicitada);
            btnDetalle.Click += (_, _) => Disparar(DetalleSolicitado);
            _grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) Disparar(DetalleSolicitado); };
        }

        public void MostrarVentas(IList<Venta> ventas)
        {
            _grid.DataSource = new BindingList<Venta>(ventas.ToList());
            _lblResumen.Text = ventas.Count == 0
                ? "Sin ventas en el período seleccionado"
                : $"{ventas.Count} venta(s)  ·  Total: {Formato.Dinero(ventas.Sum(v => v.Total))}";
        }

        public void ConfigurarPermisos(bool puedeRegistrar, bool veTodas)
        {
            _btnNueva.Visible = puedeRegistrar;
            _encabezadoTitulo.Text = veTodas ? "Ventas" : "Mis ventas";
        }

        public bool PedirNuevaVenta(IList<Producto> productos, IList<Cliente> clientes,
            Func<int?, string?, IList<LineaVenta>, string?> registrar)
        {
            using var dialogo = new FrmNuevaVenta(productos, clientes, registrar);
            return dialogo.ShowDialog(FindForm()) == DialogResult.OK;
        }

        public void MostrarDetalle(Venta venta)
        {
            using var dialogo = new FrmDetalleVenta(venta);
            dialogo.ShowDialog(FindForm());
        }
    }
}
