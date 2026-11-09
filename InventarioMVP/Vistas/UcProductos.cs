using System.ComponentModel;
using InventarioMVP.Entidades;

namespace InventarioMVP.Vistas
{
    public class UcProductos : VistaBase, IProductosView
    {
        private readonly DataGridView _grid = new();
        private readonly FlowLayoutPanel _barra;
        private readonly TextBox _txtBuscar = new() { Width = 280, Font = Estilos.Fuente, PlaceholderText = "Buscar por código, nombre o categoría", Margin = new Padding(0, 5, 8, 0) };
        private readonly Button _btnNuevo = Estilos.Boton("Nuevo producto", Estilos.Primario, 150, "plus");
        private readonly Button _btnEditar = Estilos.Boton("Editar", Estilos.Neutro, 100, "pencil");
        private readonly Button _btnEliminar = Estilos.Boton("Eliminar", Estilos.Peligro, 110, "trash");

        public event EventHandler? CargarSolicitado;
        public event EventHandler? BuscarSolicitado;
        public event EventHandler? NuevoSolicitado;
        public event EventHandler? EditarSolicitado;
        public event EventHandler? EliminarSolicitado;

        public string TextoBusqueda => _txtBuscar.Text;
        public Producto? ProductoSeleccionado => _grid.CurrentRow?.DataBoundItem as Producto;

        public UcProductos()
        {
            Estilos.ConfigurarGrid(_grid);
            _grid.Columns.AddRange(
                Estilos.Columna(nameof(Producto.Codigo), "Código", 70),
                Estilos.Columna(nameof(Producto.Nombre), "Nombre", 120),
                Estilos.Columna(nameof(Producto.Descripcion), "Descripción", 150),
                Estilos.Columna(nameof(Producto.Categoria), "Categoría", 80),
                Estilos.ColumnaDinero(nameof(Producto.Precio), "Precio (USD)", 70),
                Estilos.Columna(nameof(Producto.Stock), "Stock", 50, "N0", DataGridViewContentAlignment.MiddleRight),
                Estilos.Columna(nameof(Producto.StockMinimo), "Mínimo", 50, "N0", DataGridViewContentAlignment.MiddleRight),
                Estilos.Columna(nameof(Producto.Estado), "Estado", 70));

            var btnBuscar = Estilos.Boton("Buscar", Estilos.Primario, 105, "search");
            var btnTodos = Estilos.Boton("Ver todos", Estilos.Neutro, 120, "list");
            _barra = Estilos.Barra(_btnNuevo, _btnEditar, _btnEliminar);
            var busqueda = Estilos.Barra(_txtBuscar, btnBuscar, btnTodos);

            var tarjeta = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            tarjeta.Controls.Add(_grid);
            Controls.Add(tarjeta);
            Controls.Add(new Panel { Dock = DockStyle.Top, Height = 8 });
            Controls.Add(busqueda);
            Controls.Add(_barra);
            Controls.Add(Encabezado("Productos", "Catálogo de productos: registre, edite, elimine y busque"));

            Load += (_, _) => Disparar(CargarSolicitado);
            btnBuscar.Click += (_, _) => Disparar(BuscarSolicitado);
            btnTodos.Click += (_, _) => { _txtBuscar.Clear(); Disparar(BuscarSolicitado); };
            _txtBuscar.KeyDown += (_, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                Disparar(BuscarSolicitado);
            };
            _btnNuevo.Click += (_, _) => Disparar(NuevoSolicitado);
            _btnEditar.Click += (_, _) => Disparar(EditarSolicitado);
            _btnEliminar.Click += (_, _) => Disparar(EliminarSolicitado);
            _grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0 && _btnEditar.Visible) Disparar(EditarSolicitado); };
            _grid.CellFormatting += (_, e) =>
            {
                if (e.RowIndex < 0 || _grid.Rows[e.RowIndex].DataBoundItem is not Producto p) return;
                if (_grid.Columns[e.ColumnIndex].DataPropertyName != nameof(Producto.Estado)) return;
                e.CellStyle!.ForeColor = p.Stock == 0 ? Estilos.Peligro : p.StockBajo ? Estilos.Aviso : Estilos.Exito;
                e.CellStyle.Font = Estilos.FuenteNegrita;
            };
        }

        public void MostrarProductos(IList<Producto> productos) => _grid.DataSource = new BindingList<Producto>(productos.ToList());

        public void ConfigurarPermisos(bool puedeGestionar)
        {
            _btnNuevo.Visible = _btnEditar.Visible = _btnEliminar.Visible = _barra.Visible = puedeGestionar;
        }

        public bool EditarProducto(Producto? existente, int stockMinimoDefecto, Func<Producto, string?> guardar)
        {
            using var dialogo = new FrmProductoEditor(existente, stockMinimoDefecto, guardar);
            return dialogo.ShowDialog(FindForm()) == DialogResult.OK;
        }
    }
}
