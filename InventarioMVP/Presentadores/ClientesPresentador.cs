using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Servicios;
using InventarioMVP.Vistas;

namespace InventarioMVP.Presentadores
{
    public class ClientesPresentador
    {
        private readonly IClientesView _vista;
        private readonly ClienteService _clientes;

        public ClientesPresentador(IClientesView vista, ClienteService clientes)
        {
            _vista = vista;
            _clientes = clientes;

            _vista.CargarSolicitado += (_, _) => Ejecutar(() =>
            {
                _vista.ConfigurarPermisos(_clientes.PuedeGestionar, _clientes.PuedeVerHistorial);
                Recargar();
            });
            _vista.BuscarSolicitado += (_, _) => Ejecutar(Recargar);
            _vista.NuevoSolicitado += (_, _) => Ejecutar(Nuevo);
            _vista.EditarSolicitado += (_, _) => Ejecutar(Editar);
            _vista.HistorialSolicitado += (_, _) => Ejecutar(Historial);
        }

        private void Recargar() => _vista.MostrarClientes(_clientes.Buscar(_vista.TextoBusqueda));

        private void Nuevo()
        {
            if (_vista.EditarCliente(null, Guardar))
            {
                Recargar();
                _vista.MostrarInfo("Cliente registrado correctamente.");
            }
        }

        private void Editar()
        {
            var c = _vista.ClienteSeleccionado ?? throw new ValidacionException("Seleccione un cliente de la tabla para editarlo.");
            var copia = new Cliente
            {
                Id = c.Id, NombreCompleto = c.NombreCompleto, Telefono = c.Telefono, Correo = c.Correo,
                Direccion = c.Direccion, Observaciones = c.Observaciones, FechaRegistro = c.FechaRegistro
            };
            if (_vista.EditarCliente(copia, Guardar))
            {
                Recargar();
                _vista.MostrarInfo("Cliente actualizado correctamente.");
            }
        }

        private void Historial()
        {
            var c = _vista.ClienteSeleccionado ?? throw new ValidacionException("Seleccione un cliente de la tabla.");
            _vista.MostrarHistorial(c, _clientes.HistorialCompras(c.Id));
        }

        private string? Guardar(Cliente c) => Ejecutor.Intentar(() => _clientes.Guardar(c));

        private void Ejecutar(Action accion) => Ejecutor.Ejecutar(accion, _vista.MostrarError);
    }
}
