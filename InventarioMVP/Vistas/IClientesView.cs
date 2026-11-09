using InventarioMVP.Entidades;

namespace InventarioMVP.Vistas
{
    public interface IClientesView
    {
        event EventHandler CargarSolicitado;
        event EventHandler BuscarSolicitado;
        event EventHandler NuevoSolicitado;
        event EventHandler EditarSolicitado;
        event EventHandler HistorialSolicitado;

        string TextoBusqueda { get; }
        Cliente? ClienteSeleccionado { get; }

        void MostrarClientes(IList<Cliente> clientes);
        void ConfigurarPermisos(bool puedeGestionar, bool puedeVerHistorial);

        /// <summary>Abre la ventana modal de alta/edición. Devuelve true si se guardó.</summary>
        bool EditarCliente(Cliente? existente, Func<Cliente, string?> guardar);

        void MostrarHistorial(Cliente cliente, IList<Venta> ventas);
        void MostrarError(string mensaje);
        void MostrarInfo(string mensaje);
    }
}
