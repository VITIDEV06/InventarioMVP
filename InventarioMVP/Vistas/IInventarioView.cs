using InventarioMVP.Entidades;

namespace InventarioMVP.Vistas
{
    public interface IInventarioView
    {
        event EventHandler CargarSolicitado;
        event EventHandler BuscarSolicitado;
        event EventHandler NuevoSolicitado;
        event EventHandler GuardarSolicitado;
        event EventHandler EliminarSolicitado;
        event EventHandler SeleccionCambiada;

        string TextoBusqueda { get; }
        Producto? ProductoSeleccionado { get; }

        /// <summary>Producto armado con los valores del formulario (Id = 0 si es nuevo).</summary>
        Producto LeerFormulario();
        void MostrarProductos(IList<Producto> productos);
        void MostrarEnFormulario(Producto? producto);
        void MostrarError(string mensaje);
        void MostrarInfo(string mensaje);
        bool Confirmar(string mensaje);
    }
}
