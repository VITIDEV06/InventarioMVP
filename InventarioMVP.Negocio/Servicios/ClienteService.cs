using InventarioMVP.Datos;
using InventarioMVP.Datos.Repositorios;
using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Seguridad;

namespace InventarioMVP.Negocio.Servicios
{
    public class ClienteService
    {
        private readonly IClienteRepositorio _clientes;
        private readonly IVentaRepositorio _ventas;
        private readonly IActividadRepositorio _actividad;
        private readonly SesionUsuario _sesion;

        public ClienteService(IClienteRepositorio clientes, IVentaRepositorio ventas, IActividadRepositorio actividad, SesionUsuario sesion)
        {
            _clientes = clientes;
            _ventas = ventas;
            _actividad = actividad;
            _sesion = sesion;
        }

        public bool PuedeGestionar => _sesion.Tiene(Permisos.ClientesGestionar);

        /// <summary>El historial y los totales de compra de los clientes son información de administración.</summary>
        public bool PuedeVerHistorial => _sesion.Tiene(Permisos.VentasVerTodas);

        public IList<Cliente> Listar() => Filtrar(_clientes.ObtenerTodos());

        public IList<Cliente> Buscar(string texto)
        {
            texto = (texto ?? "").Trim();
            return texto.Length == 0 ? Listar() : Filtrar(_clientes.Buscar(texto));
        }

        private IList<Cliente> Filtrar(IList<Cliente> clientes)
        {
            _sesion.Exigir(Permisos.ClientesVer);
            if (!PuedeVerHistorial)
                foreach (var c in clientes) { c.NumCompras = 0; c.TotalComprado = 0; c.UltimaCompra = null; }
            return clientes;
        }

        public void Guardar(Cliente c)
        {
            _sesion.Exigir(Permisos.ClientesGestionar);
            c.NombreCompleto = Validaciones.Texto(c.NombreCompleto, "El nombre", 120);
            c.Telefono = Validaciones.Telefono(c.Telefono);
            c.Correo = Validaciones.Correo(c.Correo, obligatorio: false);
            c.Direccion = Validaciones.Texto(c.Direccion, "La dirección", 255, obligatorio: false);
            c.Observaciones = Validaciones.Texto(c.Observaciones, "Las observaciones", 500, obligatorio: false);
            if (c.Correo.Length > 0 && _clientes.ExisteCorreo(c.Correo, c.Id))
                throw new ValidacionException("Ya existe un cliente con ese correo.");

            bool nuevo = c.Id == 0;
            try
            {
                if (nuevo) c.Id = _clientes.Insertar(c);
                else if (!_clientes.Actualizar(c)) throw new ValidacionException("El cliente ya no existe.");
            }
            catch (ConflictoDatosException ex) { throw new ValidacionException(ex.Message); }

            _actividad.Registrar(_sesion.UsuarioId, nuevo ? "CLIENTE_CREADO" : "CLIENTE_EDITADO", c.NombreCompleto);
        }

        /// <summary>Historial de compras de un cliente (solo quienes pueden ver todas las ventas).</summary>
        public IList<Venta> HistorialCompras(int clienteId)
        {
            _sesion.Exigir(Permisos.ClientesVer);
            _sesion.Exigir(Permisos.VentasVerTodas);
            return _ventas.Buscar("", null, new DateTime(2000, 1, 1), DateTime.Today.AddDays(1), null, clienteId);
        }
    }
}
