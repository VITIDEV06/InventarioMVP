using InventarioMVP.Entidades;

namespace InventarioMVP.Vistas
{
    /// <summary>Ventana modal para registrar o editar un producto.</summary>
    public class FrmProductoEditor : DialogoBase
    {
        private readonly Producto? _existente;
        private readonly Func<Producto, string?> _guardar;
        private readonly TextBox _txtCodigo = Estilos.Caja(30);
        private readonly TextBox _txtNombre = Estilos.Caja(100);
        private readonly TextBox _txtDescripcion = new() { MaxLength = 255, Multiline = true, Height = 64, ScrollBars = ScrollBars.Vertical };
        private readonly TextBox _txtCategoria = Estilos.Caja(60);
        private readonly NumericUpDown _numPrecio = new() { DecimalPlaces = 2, Maximum = 99999999.99m, ThousandsSeparator = true };
        private readonly NumericUpDown _numStock = new() { Maximum = 100000000, ThousandsSeparator = true };
        private readonly NumericUpDown _numMinimo = new() { Maximum = 1000000 };

        public FrmProductoEditor(Producto? existente, int stockMinimoDefecto, Func<Producto, string?> guardar)
            : base(existente == null ? "Nuevo producto" : "Editar producto",
                   existente == null ? "Complete los datos del producto" : "Modifique los datos del producto",
                   new Size(480, 540))
        {
            _existente = existente;
            _guardar = guardar;

            var tabla = Estilos.TablaFormulario(130);
            Estilos.Fila(tabla, "Código", _txtCodigo);
            Estilos.Fila(tabla, "Nombre", _txtNombre);
            Estilos.Fila(tabla, "Descripción", _txtDescripcion);
            Estilos.Fila(tabla, "Categoría", _txtCategoria);
            Estilos.Fila(tabla, "Precio (USD)", _numPrecio);
            Estilos.Fila(tabla, existente == null ? "Stock inicial" : "Stock actual", _numStock);
            Estilos.Fila(tabla, "Stock mínimo", _numMinimo);
            Cuerpo.Controls.Add(tabla);

            if (existente == null)
            {
                _numMinimo.Value = stockMinimoDefecto;
            }
            else
            {
                _txtCodigo.Text = existente.Codigo;
                _txtNombre.Text = existente.Nombre;
                _txtDescripcion.Text = existente.Descripcion;
                _txtCategoria.Text = existente.Categoria;
                _numPrecio.Value = existente.Precio;
                _numStock.Value = existente.Stock;
                _numMinimo.Value = existente.StockMinimo;
                // El stock de un producto existente solo cambia mediante movimientos (módulo Inventario)
                _numStock.Enabled = false;
                Cuerpo.Controls.Add(new Label
                {
                    Text = "El stock se modifica con entradas y salidas desde el módulo Inventario.",
                    Dock = DockStyle.Top, Height = 40, ForeColor = Estilos.Neutro, Font = new Font("Segoe UI", 9F), Padding = new Padding(0, 10, 0, 0)
                });
                Cuerpo.Controls.SetChildIndex(Cuerpo.Controls[^1], 0);
            }
        }

        protected override string? Aceptar() => _guardar(new Producto
        {
            Id = _existente?.Id ?? 0,
            Codigo = _txtCodigo.Text,
            Nombre = _txtNombre.Text,
            Descripcion = _txtDescripcion.Text,
            Categoria = _txtCategoria.Text,
            Precio = _numPrecio.Value,
            Stock = (int)_numStock.Value,
            StockMinimo = (int)_numMinimo.Value
        });
    }
}
