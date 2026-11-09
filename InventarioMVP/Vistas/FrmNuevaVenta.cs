using System.ComponentModel;
using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Servicios;

namespace InventarioMVP.Vistas
{
    /// <summary>Ventana modal para registrar una venta: cliente, productos del inventario y total automático.</summary>
    public class FrmNuevaVenta : DialogoBase
    {
        private sealed class LineaPantalla
        {
            public Producto Producto { get; init; } = null!;
            public string Codigo => Producto.Codigo;
            public string Nombre => Producto.Nombre;
            public int Cantidad { get; set; }
            public decimal PrecioUnitario => Producto.Precio;
            public decimal Subtotal => Producto.Precio * Cantidad;
        }

        private readonly Func<int?, string?, IList<LineaVenta>, string?> _registrar;
        private readonly BindingList<LineaPantalla> _lineas = new();
        private readonly RadioButton _rbRegistrado = new() { Text = "Cliente registrado", Checked = true, AutoSize = true, Font = Estilos.Fuente };
        private readonly RadioButton _rbOcasional = new() { Text = "Cliente ocasional", AutoSize = true, Font = Estilos.Fuente };
        private readonly ComboBox _cmbCliente = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill, Font = Estilos.Fuente };
        private readonly TextBox _txtOcasional = new() { Dock = DockStyle.Fill, MaxLength = 120, Font = Estilos.Fuente, PlaceholderText = "Nombre (opcional)", Enabled = false };
        private readonly ComboBox _cmbProducto = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill, Font = Estilos.Fuente, FormattingEnabled = true };
        private readonly NumericUpDown _numCantidad = new() { Minimum = 1, Maximum = 100000, Value = 1, Width = 80, Font = Estilos.Fuente };
        private readonly DataGridView _grid = new();
        private readonly Label _lblTotal = new() { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI Semibold", 18F), ForeColor = Estilos.Exito };

        public FrmNuevaVenta(IList<Producto> productos, IList<Cliente> clientes,
            Func<int?, string?, IList<LineaVenta>, string?> registrar)
            : base("Nueva venta", "Seleccione el cliente y agregue los productos", new Size(900, 680))
        {
            _registrar = registrar;
            BtnAceptar.Text = "Registrar venta";
            BtnAceptar.Width = 160;
            BtnAceptar.BackColor = Estilos.Exito;
            BtnAceptar.Image = Iconos.Obtener("shopping-cart", Iconos.Blanco);

            _cmbCliente.DataSource = clientes.ToList();
            _cmbCliente.DisplayMember = nameof(Cliente.NombreCompleto);
            if (clientes.Count == 0) { _rbOcasional.Checked = true; _rbRegistrado.Enabled = false; }
            _cmbProducto.DataSource = productos.ToList();
            _cmbProducto.Format += (_, e) =>
            {
                if (e.ListItem is Producto p)
                    e.Value = $"{p.Codigo} · {p.Nombre}   (stock {p.Stock}, {p.Precio.ToString("C2", Estilos.Usd)})";
            };

            Estilos.ConfigurarGrid(_grid);
            _grid.Columns.AddRange(
                Estilos.Columna(nameof(LineaPantalla.Codigo), "Código", 60),
                Estilos.Columna(nameof(LineaPantalla.Nombre), "Producto", 150),
                Estilos.Columna(nameof(LineaPantalla.Cantidad), "Cant.", 40, "N0", DataGridViewContentAlignment.MiddleRight),
                Estilos.ColumnaDinero(nameof(LineaPantalla.PrecioUnitario), "Precio", 60),
                Estilos.ColumnaDinero(nameof(LineaPantalla.Subtotal), "Subtotal", 70));
            _grid.DataSource = _lineas;

            var btnAgregar = Estilos.Boton("Agregar", Estilos.Primario, 110, "plus");
            var btnQuitar = Estilos.Boton("Quitar línea", Estilos.Peligro, 130, "trash");

            // Cliente
            var cliente = new TableLayoutPanel { Dock = DockStyle.Top, Height = 78, ColumnCount = 2 };
            cliente.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            cliente.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            cliente.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            cliente.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            cliente.Controls.Add(_rbRegistrado, 0, 0);
            cliente.Controls.Add(_cmbCliente, 1, 0);
            cliente.Controls.Add(_rbOcasional, 0, 1);
            cliente.Controls.Add(_txtOcasional, 1, 1);
            _rbRegistrado.Margin = _rbOcasional.Margin = new Padding(0, 6, 0, 0);

            // Producto + cantidad
            var agregar = new TableLayoutPanel { Dock = DockStyle.Top, Height = 46, ColumnCount = 5 };
            agregar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86));
            agregar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            agregar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            agregar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            agregar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            agregar.Controls.Add(Estilos.Etiqueta("Producto"), 0, 0);
            agregar.Controls.Add(_cmbProducto, 1, 0);
            agregar.Controls.Add(Estilos.Etiqueta("   Cantidad"), 2, 0);
            agregar.Controls.Add(_numCantidad, 3, 0);
            agregar.Controls.Add(btnAgregar, 4, 0);
            _cmbProducto.Margin = new Padding(0, 4, 8, 0);
            _numCantidad.Margin = new Padding(0, 4, 8, 0);

            var totales = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 54, ColumnCount = 2 };
            totales.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            totales.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            totales.Controls.Add(btnQuitar, 0, 0);
            totales.Controls.Add(_lblTotal, 1, 0);
            btnQuitar.Margin = new Padding(0, 8, 0, 0);

            // Orden inverso de apilado: el último agregado queda arriba.
            Cuerpo.Controls.Add(_grid);
            Cuerpo.Controls.Add(totales);
            Cuerpo.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 8 });
            Cuerpo.Controls.Add(agregar);
            Cuerpo.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 6 });
            Cuerpo.Controls.Add(cliente);
            ActualizarTotal();

            _rbRegistrado.CheckedChanged += (_, _) => { _cmbCliente.Enabled = _rbRegistrado.Checked; _txtOcasional.Enabled = !_rbRegistrado.Checked; };
            btnAgregar.Click += (_, _) => Agregar();
            btnQuitar.Click += (_, _) =>
            {
                if (_grid.CurrentRow?.DataBoundItem is LineaPantalla l) { _lineas.Remove(l); ActualizarTotal(); }
            };
        }

        private void Agregar()
        {
            MostrarErrorInterno(string.Empty);
            if (_cmbProducto.SelectedItem is not Producto p) { MostrarErrorInterno("Seleccione un producto."); return; }
            int cantidad = (int)_numCantidad.Value;
            var existente = _lineas.FirstOrDefault(l => l.Producto.Id == p.Id);
            int total = cantidad + (existente?.Cantidad ?? 0);
            if (total > p.Stock)
            {
                MostrarErrorInterno($"Stock insuficiente de \"{p.Nombre}\": disponible {p.Stock}" +
                                    (existente != null ? $", ya agregó {existente.Cantidad}." : "."));
                return;
            }
            if (existente != null)
            {
                existente.Cantidad = total;
                _lineas.ResetBindings();
            }
            else _lineas.Add(new LineaPantalla { Producto = p, Cantidad = cantidad });
            _numCantidad.Value = 1;
            ActualizarTotal();
        }

        private void ActualizarTotal() =>
            _lblTotal.Text = "Total:  " + _lineas.Sum(l => l.Subtotal).ToString("C2", Estilos.Usd);

        protected override string? Aceptar()
        {
            if (_lineas.Count == 0) return "Agregue al menos un producto a la venta.";
            int? clienteId = _rbRegistrado.Checked ? (_cmbCliente.SelectedItem as Cliente)?.Id : null;
            if (_rbRegistrado.Checked && clienteId == null) return "Seleccione un cliente o elija cliente ocasional.";
            var lineas = _lineas.Select(l => new LineaVenta { ProductoId = l.Producto.Id, Cantidad = l.Cantidad }).ToList();
            return _registrar(clienteId, _txtOcasional.Text, lineas);
        }
    }
}
