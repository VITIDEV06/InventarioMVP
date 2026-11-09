using InventarioMVP.Datos;
using InventarioMVP.Datos.Repositorios;
using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Seguridad;

namespace InventarioMVP.Negocio.Servicios
{
    public class ProductoService
    {
        private readonly IProductoRepositorio _productos;
        private readonly IActividadRepositorio _actividad;
        private readonly SesionUsuario _sesion;

        public ProductoService(IProductoRepositorio productos, IActividadRepositorio actividad, SesionUsuario sesion)
        {
            _productos = productos;
            _actividad = actividad;
            _sesion = sesion;
        }

        public bool PuedeGestionar => _sesion.Tiene(Permisos.ProductosGestionar);

        public IList<Producto> Listar()
        {
            _sesion.Exigir(Permisos.ProductosVer);
            return _productos.ObtenerTodos();
        }

        public IList<Producto> Buscar(string texto)
        {
            _sesion.Exigir(Permisos.ProductosVer);
            texto = (texto ?? "").Trim();
            return texto.Length == 0 ? _productos.ObtenerTodos() : _productos.Buscar(texto);
        }

        /// <summary>
        /// Inserta si Id == 0 (con su stock inicial, que genera un movimiento de entrada);
        /// de lo contrario actualiza los datos. El stock de un producto existente solo cambia mediante movimientos.
        /// </summary>
        public void Guardar(Producto p)
        {
            _sesion.Exigir(Permisos.ProductosGestionar);
            p.Codigo = Validaciones.Texto(p.Codigo, "El código", 30);
            p.Nombre = Validaciones.Texto(p.Nombre, "El nombre", 100);
            p.Descripcion = Validaciones.Texto(p.Descripcion, "La descripción", 255, obligatorio: false);
            p.Categoria = Validaciones.Texto(p.Categoria, "La categoría", 60);
            if (p.Precio < 0 || p.Precio > 99999999.99m)
                throw new ValidacionException("El precio debe estar entre 0 y 99,999,999.99.");
            if (p.StockMinimo < 0) throw new ValidacionException("El stock mínimo no puede ser negativo.");
            if (p.Id == 0 && p.Stock < 0) throw new ValidacionException("El stock no puede ser negativo.");
            if (_productos.ExisteCodigo(p.Codigo, p.Id))
                throw new ValidacionException("Ya existe un producto con ese código (puede haber sido dado de baja).");

            bool nuevo = p.Id == 0;
            try
            {
                if (nuevo) p.Id = _productos.Insertar(p, _sesion.UsuarioId);
                else if (!_productos.Actualizar(p)) throw new ValidacionException("El producto ya no existe.");
            }
            catch (ConflictoDatosException ex) { throw new ValidacionException(ex.Message); }

            _actividad.Registrar(_sesion.UsuarioId, nuevo ? "PRODUCTO_CREADO" : "PRODUCTO_EDITADO", $"{p.Codigo} - {p.Nombre}");
        }

        /// <summary>Baja lógica: el producto deja de listarse pero se conservan sus ventas y movimientos.</summary>
        public void Eliminar(int id)
        {
            _sesion.Exigir(Permisos.ProductosGestionar);
            if (id <= 0) throw new ValidacionException("Seleccione un producto.");
            var producto = _productos.ObtenerPorId(id) ?? throw new ValidacionException("El producto ya no existe.");
            if (!_productos.DarDeBaja(id)) throw new ValidacionException("El producto ya no existe.");
            _actividad.Registrar(_sesion.UsuarioId, "PRODUCTO_ELIMINADO", $"{producto.Codigo} - {producto.Nombre}");
        }
    }
}
