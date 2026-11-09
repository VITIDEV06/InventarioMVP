using System.ComponentModel;
using InventarioMVP.Entidades;

namespace InventarioMVP.Vistas
{
    /// <summary>Inventario: existencias, entradas y salidas, e historial de movimientos.</summary>
    public class UcInventario : VistaBase, IInventarioView
    {
        private readonly DataGridView _gridProductos = new();
        private readonly DataGridView _gridMovimientos = new();
        private readonly TextBox _txtBuscar = new() { Width = 190, Font = Estilos.Fuente, PlaceholderText = "Buscar producto", Margin = new Padding(0, 5, 8, 0) };
        private readonly Button _btnEntrada = Estilos.Boton("Entrada", Estilos.Exito, 120, "arrow-down-to-line");
        private readonly Button _btnSalida = Estilos.Boton("Salida", Estilos.Aviso, 120, "arrow-up-from-line");
        private readonly DateTimePicker _dtpDesde = Estilos.Fecha(DateTime.Today.AddDays(-30));
        private readonly DateTimePicker _dtpHasta = Estilos.Fecha(DateTime.Today);
        private readonly CheckBox _chkSolo = new() { Text = "Solo el producto seleccionado", AutoSize = true, Margin = new Padding(0, 8, 14, 0), Font = Estilos.Fuente };

        public event EventHandler? CargarSolicitado;
        public event EventHandler? BuscarSolicitado;
        public event EventHandler? EntradaSolicitada;
        public event EventHandler? SalidaSolicitada;
        public event EventHandler? MovimientosFiltrados;

        public string TextoBusqueda => _txtBuscar.Text;
        public Producto? ProductoSeleccionado => _gridProductos.CurrentRow?.DataBoundItem as Producto;
        public DateTime Desde => _dtpDesde.Value.Date;
        public DateTime Hasta => _dtpHasta.Value.Date;
        public bool SoloSeleccionado => _chkSolo.Checked;

        public UcInventario()
        {
            Estilos.ConfigurarGrid(_gridProductos);
            _gridProductos.Columns.AddRange(
                Estilos.Columna(nameof(Producto.Codigo), "Código", 60),
                Estilos.Columna(nameof(Producto.Nombre), "Producto", 130),
                Estilos.Columna(nameof(Producto.Categoria), "Categoría", 80),
                Estilos.ColumnaDinero(nameof(Producto.Precio), "Precio", 60),
                Estilos.Columna(nameof(Producto.Stock), "Stock", 50, "N0", DataGridViewContentAlignment.MiddleRight),
                Estilos.Columna(nameof(Producto.StockMinimo), "Mínimo", 50, "N0", DataGridViewContentAlignment.MiddleRight),
                Estilos.Columna(nameof(Producto.Estado), "Estado", 70));

            Estilos.ConfigurarGrid(_gridMovimientos);
            _gridMovimientos.Columns.AddRange(
                Estilos.Columna(nameof(MovimientoInventario.Fecha), "Fecha", 80, "dd/MM/yyyy HH:mm"),
                Estilos.Columna(nameof(MovimientoInventario.ProductoCodigo), "Código", 50),
                Estilos.Columna(nameof(MovimientoInventario.ProductoNombre), "Producto", 100),
                Estilos.Columna(nameof(MovimientoInventario.Tipo), "Tipo", 55),
                Estilos.Columna(nameof(MovimientoInventario.Cantidad), "Cant.", 40, "N0", DataGridViewContentAlignment.MiddleRight),
                Estilos.Columna(nameof(MovimientoInventario.StockAnterior), "Anterior", 45, "N0", DataGridViewContentAlignment.MiddleRight),
                Estilos.Columna(nameof(MovimientoInventario.StockNuevo), "Nuevo", 45, "N0", DataGridViewContentAlignment.MiddleRight),
                Estilos.Columna(nameof(MovimientoInventario.Motivo), "Motivo", 110),
                Estilos.Columna(nameof(MovimientoInventario.UsuarioNombre), "Usuario", 75));

            var btnBuscar = Estilos.Boton("Buscar", Estilos.Primario, 105, "search");
            var btnTodos = Estilos.Boton("Ver todos", Estilos.Neutro, 120, "list");
            var barra = Estilos.Barra(_txtBuscar, btnBuscar, btnTodos, new Panel { Width = 12 }, _btnEntrada, _btnSalida);

            // Historial con filtros
            var btnFiltrar = Estilos.Boton("Filtrar", Estilos.Primario, 105, "search");
            var filtros = Estilos.Barra(Estilos.EtiquetaBarra("Desde"), _dtpDesde, Estilos.EtiquetaBarra("Hasta"), _dtpHasta, _chkSolo, btnFiltrar);
            var contenidoHistorial = new Panel();
            contenidoHistorial.Controls.Add(_gridMovimientos);
            contenidoHistorial.Controls.Add(filtros);

            var partes = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
            partes.RowStyles.Add(new RowStyle(SizeType.Percent, 48));
            partes.RowStyles.Add(new RowStyle(SizeType.Percent, 52));
            partes.Controls.Add(Estilos.Titulado("Existencias", _gridProductos), 0, 0);
            partes.Controls.Add(Estilos.Titulado("Historial de movimientos", contenidoHistorial), 0, 1);

            Controls.Add(partes);
            Controls.Add(barra);
            Controls.Add(Encabezado("Inventario", "Consulte las existencias y registre entradas y salidas de mercancía"));

            Load += (_, _) => Disparar(CargarSolicitado);
            btnBuscar.Click += (_, _) => Disparar(BuscarSolicitado);
            btnTodos.Click += (_, _) => { _txtBuscar.Clear(); Disparar(BuscarSolicitado); };
            _txtBuscar.KeyDown += (_, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                Disparar(BuscarSolicitado);
            };
            _btnEntrada.Click += (_, _) => Disparar(EntradaSolicitada);
            _btnSalida.Click += (_, _) => Disparar(SalidaSolicitada);
            btnFiltrar.Click += (_, _) => Disparar(MovimientosFiltrados);
            _chkSolo.CheckedChanged += (_, _) => Disparar(MovimientosFiltrados);
            _gridProductos.SelectionChanged += (_, _) => { if (_chkSolo.Checked && _gridProductos.CurrentRow != null) Disparar(MovimientosFiltrados); };

            _gridProductos.CellFormatting += (_, e) =>
            {
                if (e.RowIndex < 0 || _gridProductos.Rows[e.RowIndex].DataBoundItem is not Producto p) return;
                if (_gridProductos.Columns[e.ColumnIndex].DataPropertyName != nameof(Producto.Estado)) return;
                e.CellStyle!.ForeColor = p.Stock == 0 ? Estilos.Peligro : p.StockBajo ? Estilos.Aviso : Estilos.Exito;
                e.CellStyle.Font = Estilos.FuenteNegrita;
            };
            _gridMovimientos.CellFormatting += (_, e) =>
            {
                if (e.RowIndex < 0 || _gridMovimientos.Rows[e.RowIndex].DataBoundItem is not MovimientoInventario m) return;
                if (_gridMovimientos.Columns[e.ColumnIndex].DataPropertyName != nameof(MovimientoInventario.Tipo)) return;
                e.CellStyle!.ForeColor = m.Tipo == TipoMovimiento.Entrada ? Estilos.Exito : m.Tipo == TipoMovimiento.Venta ? Estilos.Primario : Estilos.Aviso;
                e.CellStyle.Font = Estilos.FuenteNegrita;
            };
        }

        public void MostrarProductos(IList<Producto> productos)
        {
            int? seleccionado = ProductoSeleccionado?.Id;
            _gridProductos.DataSource = new BindingList<Producto>(productos.ToList());
            if (seleccionado == null) return;
            foreach (DataGridViewRow fila in _gridProductos.Rows)
                if (fila.DataBoundItem is Producto p && p.Id == seleccionado) { fila.Selected = true; _gridProductos.CurrentCell = fila.Cells[0]; break; }
        }

        public void MostrarMovimientos(IList<MovimientoInventario> movimientos) =>
            _gridMovimientos.DataSource = new BindingList<MovimientoInventario>(movimientos.ToList());

        public void ConfigurarPermisos(bool puedeMover) => _btnEntrada.Visible = _btnSalida.Visible = puedeMover;

        public bool PedirMovimiento(Producto producto, string tipo, Func<int, string, string?> guardar)
        {
            using var dialogo = new FrmMovimiento(producto, tipo, guardar);
            return dialogo.ShowDialog(FindForm()) == DialogResult.OK;
        }
    }
}
