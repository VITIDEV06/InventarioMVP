using System.ComponentModel;
using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Servicios;

namespace InventarioMVP.Vistas
{
    /// <summary>Historial de compras de un cliente, con cuánto ha comprado y su última compra.</summary>
    public class FrmHistorialCliente : DialogoBase
    {
        public FrmHistorialCliente(Cliente cliente, IList<Venta> ventas)
            : base("Historial de compras", cliente.NombreCompleto, new Size(720, 500))
        {
            BtnAceptar.Visible = false;
            BtnCancelar.Text = "Cerrar";

            var grid = new DataGridView();
            Estilos.ConfigurarGrid(grid);
            grid.Columns.AddRange(
                Estilos.Columna(nameof(Venta.Numero), "N.º de venta", 60),
                Estilos.Columna(nameof(Venta.Fecha), "Fecha y hora", 80, "dd/MM/yyyy HH:mm"),
                Estilos.Columna(nameof(Venta.UsuarioNombre), "Vendedor", 100),
                Estilos.ColumnaDinero(nameof(Venta.Total), "Total", 70));
            grid.DataSource = new BindingList<Venta>(ventas.ToList());
            grid.ClearSelection();

            decimal total = ventas.Sum(v => v.Total);
            string ultima = ventas.Count == 0 ? "—" : Formato.FechaHora(ventas.Max(v => v.Fecha));
            var resumen = new Label
            {
                Text = $"Compras: {ventas.Count}     ·     Total comprado: {Formato.Dinero(total)}     ·     Última compra: {ultima}",
                Dock = DockStyle.Top, Height = 40, Font = Estilos.FuenteNegrita, ForeColor = Estilos.Menu, TextAlign = ContentAlignment.MiddleLeft
            };
            Cuerpo.Controls.Add(grid);
            Cuerpo.Controls.Add(resumen);
        }

        protected override string? Aceptar() => null;
    }
}
