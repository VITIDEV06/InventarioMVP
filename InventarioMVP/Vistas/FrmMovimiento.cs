using InventarioMVP.Entidades;

namespace InventarioMVP.Vistas
{
    /// <summary>Ventana modal para registrar una entrada o salida de inventario.</summary>
    public class FrmMovimiento : DialogoBase
    {
        private readonly Func<int, string, string?> _guardar;
        private readonly NumericUpDown _numCantidad = new() { Minimum = 1, Maximum = 1000000, Value = 1, ThousandsSeparator = true };
        private readonly TextBox _txtMotivo = Estilos.Caja(200);

        public FrmMovimiento(Producto producto, string tipo, Func<int, string, string?> guardar)
            : base(tipo == TipoMovimiento.Entrada ? "Registrar entrada" : "Registrar salida",
                   $"{producto.Codigo} · {producto.Nombre}", new Size(460, 360))
        {
            _guardar = guardar;
            bool entrada = tipo == TipoMovimiento.Entrada;
            BtnAceptar.Text = entrada ? "Registrar entrada" : "Registrar salida";
            BtnAceptar.Width = 170;
            BtnAceptar.BackColor = entrada ? Estilos.Exito : Estilos.Aviso;
            BtnAceptar.Image = Iconos.Obtener(entrada ? "arrow-down-to-line" : "arrow-up-from-line", Iconos.Blanco);

            var tabla = Estilos.TablaFormulario(120);
            Estilos.Fila(tabla, "Stock actual", new Label { Text = producto.Stock.ToString("N0"), Font = Estilos.FuenteNegrita, AutoSize = true });
            Estilos.Fila(tabla, "Cantidad", _numCantidad);
            Estilos.Fila(tabla, entrada ? "Motivo (opcional)" : "Motivo", _txtMotivo);
            Cuerpo.Controls.Add(tabla);
            Cuerpo.Controls.Add(new Label
            {
                Text = entrada ? "Ejemplos: compra a proveedor, devolución, ajuste a favor." : "Obligatorio. Ejemplos: merma, daño, ajuste en contra, uso interno.",
                Dock = DockStyle.Top, Height = 40, ForeColor = Estilos.Neutro, Font = new Font("Segoe UI", 9F), Padding = new Padding(0, 10, 0, 0)
            });
            Cuerpo.Controls.SetChildIndex(Cuerpo.Controls[^1], 0);
            Tag = tipo;
        }

        protected override string? Aceptar() => _guardar((int)_numCantidad.Value, _txtMotivo.Text);
    }
}
