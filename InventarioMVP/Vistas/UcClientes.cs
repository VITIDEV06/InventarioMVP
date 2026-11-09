using System.ComponentModel;
using InventarioMVP.Entidades;

namespace InventarioMVP.Vistas
{
    public class UcClientes : VistaBase, IClientesView
    {
        private readonly DataGridView _grid = new();
        private readonly FlowLayoutPanel _barra;
        private readonly TextBox _txtBuscar = new() { Width = 280, Font = Estilos.Fuente, PlaceholderText = "Buscar por nombre, teléfono o correo", Margin = new Padding(0, 5, 8, 0) };
        private readonly Button _btnNuevo = Estilos.Boton("Nuevo cliente", Estilos.Primario, 150, "plus");
        private readonly Button _btnEditar = Estilos.Boton("Editar", Estilos.Neutro, 100, "pencil");
        private readonly Button _btnHistorial = Estilos.Boton("Historial", Estilos.Neutro, 120, "clipboard-list");

        public event EventHandler? CargarSolicitado;
        public event EventHandler? BuscarSolicitado;
        public event EventHandler? NuevoSolicitado;
        public event EventHandler? EditarSolicitado;
        public event EventHandler? HistorialSolicitado;

        public string TextoBusqueda => _txtBuscar.Text;
        public Cliente? ClienteSeleccionado => _grid.CurrentRow?.DataBoundItem as Cliente;

        public UcClientes()
        {
            Estilos.ConfigurarGrid(_grid);
            _grid.Columns.AddRange(
                Estilos.Columna(nameof(Cliente.NombreCompleto), "Nombre", 120),
                Estilos.Columna(nameof(Cliente.Telefono), "Teléfono", 70),
                Estilos.Columna(nameof(Cliente.Correo), "Correo", 110),
                Estilos.Columna(nameof(Cliente.NumCompras), "Compras", 50, "N0", DataGridViewContentAlignment.MiddleRight),
                Estilos.ColumnaDinero(nameof(Cliente.TotalComprado), "Total comprado", 75),
                Estilos.Columna(nameof(Cliente.UltimaCompra), "Última compra", 75, "dd/MM/yyyy HH:mm"),
                Estilos.Columna(nameof(Cliente.FechaRegistro), "Registro", 60, "dd/MM/yyyy"));

            var btnBuscar = Estilos.Boton("Buscar", Estilos.Primario, 105, "search");
            var btnTodos = Estilos.Boton("Ver todos", Estilos.Neutro, 120, "list");
            _barra = Estilos.Barra(_btnNuevo, _btnEditar, _btnHistorial);
            var busqueda = Estilos.Barra(_txtBuscar, btnBuscar, btnTodos);

            var tarjeta = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            tarjeta.Controls.Add(_grid);
            Controls.Add(tarjeta);
            Controls.Add(new Panel { Dock = DockStyle.Top, Height = 8 });
            Controls.Add(busqueda);
            Controls.Add(_barra);
            Controls.Add(Encabezado("Clientes", "Registre y consulte clientes y su historial de compras"));

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
            _btnHistorial.Click += (_, _) => Disparar(HistorialSolicitado);
            _grid.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex < 0) return;
                if (_btnEditar.Visible) Disparar(EditarSolicitado);
                else if (_btnHistorial.Visible) Disparar(HistorialSolicitado);
            };
        }

        public void MostrarClientes(IList<Cliente> clientes) => _grid.DataSource = new BindingList<Cliente>(clientes.ToList());

        public void ConfigurarPermisos(bool puedeGestionar, bool puedeVerHistorial)
        {
            _btnNuevo.Visible = _btnEditar.Visible = puedeGestionar;
            _btnHistorial.Visible = puedeVerHistorial;
            _barra.Visible = puedeGestionar || puedeVerHistorial;
            // Los totales de compra son información restringida: sin permiso no se muestran
            foreach (var nombre in new[] { nameof(Cliente.NumCompras), nameof(Cliente.TotalComprado), nameof(Cliente.UltimaCompra) })
                _grid.Columns.Cast<DataGridViewColumn>().First(c => c.DataPropertyName == nombre).Visible = puedeVerHistorial;
        }

        public bool EditarCliente(Cliente? existente, Func<Cliente, string?> guardar)
        {
            using var dialogo = new FrmClienteEditor(existente, guardar);
            return dialogo.ShowDialog(FindForm()) == DialogResult.OK;
        }

        public void MostrarHistorial(Cliente cliente, IList<Venta> ventas)
        {
            using var dialogo = new FrmHistorialCliente(cliente, ventas);
            dialogo.ShowDialog(FindForm());
        }
    }
}
