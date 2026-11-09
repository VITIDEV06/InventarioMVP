using InventarioMVP.Datos;
using InventarioMVP.Datos.Repositorios;
using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Seguridad;

namespace InventarioMVP.Negocio.Servicios
{
    public class InventarioService
    {
        private const int CantidadMaxima = 1_000_000;

        private readonly IProductoRepositorio _productos;
        private readonly IInventarioRepositorio _inventario;
        private readonly IActividadRepositorio _actividad;
        private readonly SesionUsuario _sesion;

        public InventarioService(IProductoRepositorio productos, IInventarioRepositorio inventario,
            IActividadRepositorio actividad, SesionUsuario sesion)
        {
            _productos = productos;
            _inventario = inventario;
            _actividad = actividad;
            _sesion = sesion;
        }

        public bool PuedeMover => _sesion.Tiene(Permisos.InventarioMover);

        public IList<Producto> Listar(string texto)
        {
            _sesion.Exigir(Permisos.InventarioVer);
            texto = (texto ?? "").Trim();
            return texto.Length == 0 ? _productos.ObtenerTodos() : _productos.Buscar(texto);
        }

        /// <summary>Registra una entrada o salida de inventario y devuelve el stock resultante.</summary>
        public int RegistrarMovimiento(int productoId, string tipo, int cantidad, string motivo)
        {
            _sesion.Exigir(Permisos.InventarioMover);
            if (tipo != TipoMovimiento.Entrada && tipo != TipoMovimiento.Salida)
                throw new ValidacionException("El tipo de movimiento no es válido.");
            if (cantidad <= 0) throw new ValidacionException("La cantidad debe ser mayor que cero.");
            if (cantidad > CantidadMaxima) throw new ValidacionException($"La cantidad no puede superar {CantidadMaxima:N0}.");
            motivo = Validaciones.Texto(motivo, "El motivo", 200, obligatorio: tipo == TipoMovimiento.Salida);

            try
            {
                int nuevo = _inventario.RegistrarMovimiento(productoId, tipo, cantidad, motivo, _sesion.UsuarioId);
                _actividad.Registrar(_sesion.UsuarioId, "MOVIMIENTO_" + tipo, $"Producto #{productoId}: {cantidad} u. (stock {nuevo})");
                return nuevo;
            }
            catch (ConflictoDatosException ex) { throw new ValidacionException(ex.Message); }
        }

        public IList<MovimientoInventario> Movimientos(DateTime desde, DateTime hasta, int? productoId)
        {
            _sesion.Exigir(Permisos.InventarioVer);
            Validaciones.Fechas(desde, hasta);
            return _inventario.ObtenerMovimientos(desde.Date, hasta.Date.AddDays(1), productoId);
        }
    }
}
