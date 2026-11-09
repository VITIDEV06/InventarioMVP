using InventarioMVP.Entidades;

namespace InventarioMVP.Vistas
{
    /// <summary>Inventario: existencias por producto y registro de entradas/salidas con su historial.</summary>
    public interface IInventarioView
    {
        event EventHandler CargarSolicitado;
        event EventHandler BuscarSolicitado;
        event EventHandler EntradaSolicitada;
        event EventHandler SalidaSolicitada;
        event EventHandler MovimientosFiltrados;

        string TextoBusqueda { get; }
        Producto? ProductoSeleccionado { get; }
        DateTime Desde { get; }
        DateTime Hasta { get; }
        bool SoloSeleccionado { get; }

        void MostrarProductos(IList<Producto> productos);
        void MostrarMovimientos(IList<MovimientoInventario> movimientos);
        void ConfigurarPermisos(bool puedeMover);

        /// <summary>Abre la ventana modal de entrada/salida. Devuelve true si se registró.</summary>
        bool PedirMovimiento(Producto producto, string tipo, Func<int, string, string?> guardar);

        void MostrarError(string mensaje);
        void MostrarInfo(string mensaje);
    }
}
