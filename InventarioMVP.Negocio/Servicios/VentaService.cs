using System.Text.RegularExpressions;
using InventarioMVP.Datos;
using InventarioMVP.Datos.Repositorios;
using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Seguridad;

namespace InventarioMVP.Negocio.Servicios
{
    public class VentaService
    {
        private static readonly Regex PatronNumero = new(@"^(?:V-?)?0*(\d{1,9})$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly IVentaRepositorio _ventas;
        private readonly IProductoRepositorio _productos;
        private readonly IClienteRepositorio _clientes;
        private readonly IActividadRepositorio _actividad;
        private readonly SesionUsuario _sesion;

        public VentaService(IVentaRepositorio ventas, IProductoRepositorio productos, IClienteRepositorio clientes,
            IActividadRepositorio actividad, SesionUsuario sesion)
        {
            _ventas = ventas;
            _productos = productos;
            _clientes = clientes;
            _actividad = actividad;
            _sesion = sesion;
        }

        public bool PuedeRegistrar => _sesion.Tiene(Permisos.VentasRegistrar);
        public bool VeTodas => _sesion.Tiene(Permisos.VentasVerTodas);

        /// <summary>Productos con existencias, para armar una venta.</summary>
        public IList<Producto> ProductosParaVenta()
        {
            _sesion.Exigir(Permisos.VentasRegistrar);
            return _productos.ObtenerTodos().Where(p => p.Stock > 0).ToList();
        }

        public IList<Cliente> ClientesParaVenta()
        {
            _sesion.Exigir(Permisos.VentasRegistrar);
            return _clientes.ObtenerTodos();
        }

        /// <summary>
        /// Registra una venta de forma transaccional: descuenta el stock, valida existencias y guarda quién la hizo.
        /// Si clienteId es null se registra a un cliente ocasional. Devuelve la venta registrada.
        /// </summary>
        public Venta Registrar(int? clienteId, string? clienteOcasional, IList<LineaVenta> lineas)
        {
            _sesion.Exigir(Permisos.VentasRegistrar);
            if (lineas == null || lineas.Count == 0) throw new ValidacionException("Agregue al menos un producto a la venta.");

            // Unificar líneas repetidas y validar cantidades
            var unificadas = lineas.GroupBy(l => l.ProductoId)
                .Select(g => new LineaVenta { ProductoId = g.Key, Cantidad = g.Sum(l => l.Cantidad) }).ToList();
            if (unificadas.Any(l => l.ProductoId <= 0 || l.Cantidad <= 0))
                throw new ValidacionException("Todas las cantidades deben ser mayores que cero.");
            if (unificadas.Count > 100) throw new ValidacionException("Una venta no puede tener más de 100 productos distintos.");

            if (clienteId != null && _clientes.ObtenerPorId(clienteId.Value) == null)
                throw new ValidacionException("El cliente seleccionado ya no existe.");
            string? ocasional = null;
            if (clienteId == null)
            {
                ocasional = Validaciones.Texto(clienteOcasional, "El nombre del cliente", 120, obligatorio: false);
                if (ocasional.Length == 0) ocasional = "Cliente ocasional";
            }

            int id;
            try { id = _ventas.Registrar(_sesion.UsuarioId, clienteId, ocasional, unificadas); }
            catch (ConflictoDatosException ex) { throw new ValidacionException(ex.Message); }

            var venta = _ventas.ObtenerConDetalle(id)!;
            _actividad.Registrar(_sesion.UsuarioId, "VENTA", $"{venta.Numero} por {Formato.Dinero(venta.Total)} a {venta.ClienteNombre}");
            return venta;
        }

        /// <summary>
        /// Historial de ventas. Quien no puede ver todas las ventas solo recibe las suyas
        /// (el filtro se aplica aquí, no en la interfaz).
        /// </summary>
        public IList<Venta> Historial(string texto, DateTime desde, DateTime hasta)
        {
            if (!VeTodas && !_sesion.Tiene(Permisos.VentasPropiasVer)) throw new AccesoDenegadoException();
            Validaciones.Fechas(desde, hasta);
            texto = (texto ?? "").Trim();
            int? numero = null;
            var m = PatronNumero.Match(texto);
            if (m.Success) numero = int.Parse(m.Groups[1].Value);

            int? usuario = VeTodas ? null : _sesion.UsuarioId;
            return _ventas.Buscar(texto, numero, desde.Date, hasta.Date.AddDays(1), usuario);
        }

        public Venta ObtenerDetalle(int ventaId)
        {
            if (!VeTodas && !_sesion.Tiene(Permisos.VentasPropiasVer)) throw new AccesoDenegadoException();
            var venta = _ventas.ObtenerConDetalle(ventaId) ?? throw new ValidacionException("La venta no existe.");
            if (!VeTodas && venta.UsuarioId != _sesion.UsuarioId) throw new AccesoDenegadoException();
            return venta;
        }
    }
}
