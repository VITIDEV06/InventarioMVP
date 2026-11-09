using InventarioMVP.Entidades;

namespace InventarioMVP.Vistas
{
    public interface IVentasView
    {
        event EventHandler CargarSolicitado;
        event EventHandler BuscarSolicitado;
        event EventHandler NuevaVentaSolicitada;
        event EventHandler DetalleSolicitado;

        string TextoBusqueda { get; }
        DateTime Desde { get; }
        DateTime Hasta { get; }
        Venta? VentaSeleccionada { get; }

        void MostrarVentas(IList<Venta> ventas);
        void ConfigurarPermisos(bool puedeRegistrar, bool veTodas);

        /// <summary>Abre la ventana modal de nueva venta. Devuelve true si se registró.</summary>
        bool PedirNuevaVenta(IList<Producto> productos, IList<Cliente> clientes,
            Func<int?, string?, IList<LineaVenta>, string?> registrar);

        void MostrarDetalle(Venta venta);
        void MostrarError(string mensaje);
        void MostrarInfo(string mensaje);
    }
}
