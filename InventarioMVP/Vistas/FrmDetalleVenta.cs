using System.ComponentModel;
using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Servicios;

namespace InventarioMVP.Vistas
{
    /// <summary>Detalle de solo lectura de una venta.</summary>
    public class FrmDetalleVenta : DialogoBase
    {
        public FrmDetalleVenta(Venta venta)
            : base("Venta " + venta.Numero,
                   $"{Formato.FechaHora(venta.Fecha)} · Cliente: {venta.ClienteNombre} · Vendedor: {venta.UsuarioNombre}",
                   new Size(720, 480))
        {
            BtnAceptar.Visible = false;
            BtnCancelar.Text = "Cerrar";

            var grid = new DataGridView();
            Estilos.ConfigurarGrid(grid);
            grid.Columns.AddRange(
                Estilos.Columna(nameof(DetalleVenta.ProductoCodigo), "Código", 60),
                Estilos.Columna(nameof(DetalleVenta.ProductoNombre), "Producto", 150),
                Estilos.Columna(nameof(DetalleVenta.Cantidad), "Cant.", 40, "N0", DataGridViewContentAlignment.MiddleRight),
                Estilos.ColumnaDinero(nameof(DetalleVenta.PrecioUnitario), "Precio", 60),
                Estilos.ColumnaDinero(nameof(DetalleVenta.Subtotal), "Subtotal", 70));
            grid.DataSource = new BindingList<DetalleVenta>(venta.Detalles);
            grid.ClearSelection();

            var total = new Label
            {
                Text = "Total:  " + Formato.Dinero(venta.Total), Dock = DockStyle.Bottom, Height = 52,
                TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI Semibold", 18F), ForeColor = Estilos.Exito
            };
            Cuerpo.Controls.Add(grid);
            Cuerpo.Controls.Add(total);
        }

        protected override string? Aceptar() => null;
    }
}
