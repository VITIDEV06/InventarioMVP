using System.ComponentModel;
using InventarioMVP.Entidades;

namespace InventarioMVP.Vistas
{
    public class UcInventario : UserControl, IInventarioView
    {
        private readonly DataGridView _grid = new();
        private readonly TextBox _txtBuscar = new() { Width = 260, Font = Estilos.Fuente, PlaceholderText = "Buscar por código, nombre o categoría" };
        private readonly TextBox _txtCodigo = new() { Dock = DockStyle.Fill, MaxLength = 30 };
        private readonly TextBox _txtNombre = new() { Dock = DockStyle.Fill, MaxLength = 100 };
        private readonly TextBox _txtDescripcion = new() { Dock = DockStyle.Fill, MaxLength = 255, Multiline = true, Height = 60 };
        private readonly TextBox _txtCategoria = new() { Dock = DockStyle.Fill, MaxLength = 60 };
        private readonly NumericUpDown _numPrecio = new() { Dock = DockStyle.Fill, DecimalPlaces = 2, Maximum = 99999999.99m, ThousandsSeparator = true };
        private readonly NumericUpDown _numStock = new() { Dock = DockStyle.Fill, Maximum = 100000000 };
        private readonly Label _lblModo = new() { AutoSize = true, Font = new Font("Segoe UI Semibold", 11F) };
        private int _idActual;
        private bool _cargando;

        public event EventHandler? CargarSolicitado;
        public event EventHandler? BuscarSolicitado;
        public event EventHandler? NuevoSolicitado;
        public event EventHandler? GuardarSolicitado;
        public event EventHandler? EliminarSolicitado;
        public event EventHandler? SeleccionCambiada;

        public string TextoBusqueda => _txtBuscar.Text;

        public Producto? ProductoSeleccionado =>
            _grid.CurrentRow?.Selected == true ? _grid.CurrentRow.DataBoundItem as Producto : null;

        public UcInventario()
        {
            Dock = DockStyle.Fill;
            Font = Estilos.Fuente;
            BackColor = Estilos.Fondo;

            // --- Barra de búsqueda ---
            var btnBuscar = Estilos.Boton("Buscar", Estilos.Primario, 105, "search");
            var btnTodos = Estilos.Boton("Ver todos", Estilos.Neutro, 120, "list");
            var barra = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(0, 6, 0, 0) };
            barra.Controls.AddRange(new Control[] { _txtBuscar, btnBuscar, btnTodos });
            _txtBuscar.Margin = new Padding(0, 5, 8, 0);

            // --- Grid ---
            Estilos.ConfigurarGrid(_grid);
            _grid.Columns.AddRange(
                Estilos.Columna(nameof(Producto.Codigo), "Código", 70),
                Estilos.Columna(nameof(Producto.Nombre), "Nombre", 120),
                Estilos.Columna(nameof(Producto.Descripcion), "Descripción", 140),
                Estilos.Columna(nameof(Producto.Categoria), "Categoría", 80),
                Estilos.Columna(nameof(Producto.Precio), "Precio", 60, "N2", DataGridViewContentAlignment.MiddleRight),
                Estilos.Columna(nameof(Producto.Stock), "Stock", 50, "N0", DataGridViewContentAlignment.MiddleRight));

            // --- Formulario lateral ---
            var tabla = new TableLayoutPanel
            {
                Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Padding = new Padding(12)
            };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            void Fila(string etiqueta, Control c)
            {
                tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tabla.Controls.Add(Estilos.Etiqueta(etiqueta));
                c.Margin = new Padding(0, 4, 0, 4);
                tabla.Controls.Add(c);
            }
            Fila("Código", _txtCodigo);
            Fila("Nombre", _txtNombre);
            Fila("Descripción", _txtDescripcion);
            Fila("Categoría", _txtCategoria);
            Fila("Precio", _numPrecio);
            Fila("Stock", _numStock);

            var btnNuevo = Estilos.Boton("Nuevo", Estilos.Neutro, 95, "plus");
            var btnGuardar = Estilos.Boton("Guardar", Estilos.Primario, 100, "save");
            var btnEliminar = Estilos.Boton("Eliminar", Estilos.Peligro, 105, "trash");
            var botones = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(12, 8, 0, 0) };
            botones.Controls.AddRange(new Control[] { btnNuevo, btnGuardar, btnEliminar });

            var lateral = new Panel { Dock = DockStyle.Right, Width = 340, BackColor = Color.White };
            lateral.Controls.Add(botones);
            lateral.Controls.Add(tabla);
            lateral.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(12, 12, 0, 0), Controls = { _lblModo } });

            var centro = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 12, 0) };
            centro.Controls.Add(_grid);
            centro.Controls.Add(barra);

            Controls.Add(centro);
            Controls.Add(lateral);
            Padding = new Padding(16);

            MostrarEnFormulario(null);

            // --- Eventos hacia el presentador ---
            Load += (_, _) => CargarSolicitado?.Invoke(this, EventArgs.Empty);
            btnBuscar.Click += (_, _) => BuscarSolicitado?.Invoke(this, EventArgs.Empty);
            btnTodos.Click += (_, _) => { _txtBuscar.Clear(); BuscarSolicitado?.Invoke(this, EventArgs.Empty); };
            _txtBuscar.KeyDown += (_, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                BuscarSolicitado?.Invoke(this, EventArgs.Empty);
            };
            btnNuevo.Click += (_, _) => NuevoSolicitado?.Invoke(this, EventArgs.Empty);
            btnGuardar.Click += (_, _) => GuardarSolicitado?.Invoke(this, EventArgs.Empty);
            btnEliminar.Click += (_, _) => EliminarSolicitado?.Invoke(this, EventArgs.Empty);
            _grid.SelectionChanged += (_, _) =>
            {
                if (!_cargando) SeleccionCambiada?.Invoke(this, EventArgs.Empty);
            };
        }

        public Producto LeerFormulario() => new()
        {
            Id = _idActual,
            Codigo = _txtCodigo.Text,
            Nombre = _txtNombre.Text,
            Descripcion = _txtDescripcion.Text,
            Categoria = _txtCategoria.Text,
            Precio = _numPrecio.Value,
            Stock = (int)_numStock.Value
        };

        public void MostrarProductos(IList<Producto> productos)
        {
            _cargando = true;
            try
            {
                _grid.DataSource = new BindingList<Producto>(productos.ToList());
                _grid.CurrentCell = null;
                _grid.ClearSelection();
            }
            finally { _cargando = false; }
        }

        public void MostrarEnFormulario(Producto? p)
        {
            _idActual = p?.Id ?? 0;
            _txtCodigo.Text = p?.Codigo ?? "";
            _txtNombre.Text = p?.Nombre ?? "";
            _txtDescripcion.Text = p?.Descripcion ?? "";
            _txtCategoria.Text = p?.Categoria ?? "";
            _numPrecio.Value = p?.Precio ?? 0;
            _numStock.Value = p?.Stock ?? 0;
            _lblModo.Text = p == null ? "Nuevo producto" : "Editando producto";
            if (p == null)
            {
                _cargando = true;
                _grid.CurrentCell = null;
                _grid.ClearSelection();
                _cargando = false;
            }
        }

        public void MostrarError(string mensaje) =>
            MessageBox.Show(FindForm(), mensaje, "Inventario", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        public void MostrarInfo(string mensaje) =>
            MessageBox.Show(FindForm(), mensaje, "Inventario", MessageBoxButtons.OK, MessageBoxIcon.Information);

        public bool Confirmar(string mensaje) =>
            MessageBox.Show(FindForm(), mensaje, "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }
}
