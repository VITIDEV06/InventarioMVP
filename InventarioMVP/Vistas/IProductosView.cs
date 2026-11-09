using InventarioMVP.Entidades;

namespace InventarioMVP.Vistas
{
    public interface IProductosView
    {
        event EventHandler CargarSolicitado;
        event EventHandler BuscarSolicitado;
        event EventHandler NuevoSolicitado;
        event EventHandler EditarSolicitado;
        event EventHandler EliminarSolicitado;

        string TextoBusqueda { get; }
        Producto? ProductoSeleccionado { get; }

        void MostrarProductos(IList<Producto> productos);
        void ConfigurarPermisos(bool puedeGestionar);

        /// <summary>Abre la ventana modal de alta/edición. Devuelve true si se guardó.</summary>
        bool EditarProducto(Producto? existente, int stockMinimoDefecto, Func<Producto, string?> guardar);

        void MostrarError(string mensaje);
        void MostrarInfo(string mensaje);
        bool Confirmar(string mensaje);
    }
}
