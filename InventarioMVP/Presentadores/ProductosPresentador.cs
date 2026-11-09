using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Servicios;
using InventarioMVP.Vistas;

namespace InventarioMVP.Presentadores
{
    public class ProductosPresentador
    {
        private readonly IProductosView _vista;
        private readonly ProductoService _productos;
        private readonly ConfiguracionService _config;

        public ProductosPresentador(IProductosView vista, ProductoService productos, ConfiguracionService config)
        {
            _vista = vista;
            _productos = productos;
            _config = config;

            _vista.CargarSolicitado += (_, _) => Ejecutar(() =>
            {
                _vista.ConfigurarPermisos(_productos.PuedeGestionar);
                Recargar();
            });
            _vista.BuscarSolicitado += (_, _) => Ejecutar(Recargar);
            _vista.NuevoSolicitado += (_, _) => Ejecutar(Nuevo);
            _vista.EditarSolicitado += (_, _) => Ejecutar(Editar);
            _vista.EliminarSolicitado += (_, _) => Ejecutar(Eliminar);
        }

        private void Recargar() => _vista.MostrarProductos(_productos.Buscar(_vista.TextoBusqueda));

        private void Nuevo()
        {
            if (_vista.EditarProducto(null, _config.StockMinimoDefecto(), Guardar))
            {
                Recargar();
                _vista.MostrarInfo("Producto registrado correctamente.");
            }
        }

        private void Editar()
        {
            var p = _vista.ProductoSeleccionado ?? throw new ValidacionException("Seleccione un producto de la tabla para editarlo.");
            var copia = new Producto
            {
                Id = p.Id, Codigo = p.Codigo, Nombre = p.Nombre, Descripcion = p.Descripcion,
                Categoria = p.Categoria, Precio = p.Precio, Stock = p.Stock, StockMinimo = p.StockMinimo
            };
            if (_vista.EditarProducto(copia, _config.StockMinimoDefecto(), Guardar))
            {
                Recargar();
                _vista.MostrarInfo("Producto actualizado correctamente.");
            }
        }

        private void Eliminar()
        {
            var p = _vista.ProductoSeleccionado ?? throw new ValidacionException("Seleccione un producto de la tabla para eliminarlo.");
            if (!_vista.Confirmar($"¿Eliminar el producto \"{p.Nombre}\"?\n\nDejará de mostrarse, pero se conservará su historial de ventas y movimientos."))
                return;
            _productos.Eliminar(p.Id);
            Recargar();
            _vista.MostrarInfo("Producto eliminado.");
        }

        private string? Guardar(Producto p) => Ejecutor.Intentar(() => _productos.Guardar(p));

        private void Ejecutar(Action accion) => Ejecutor.Ejecutar(accion, _vista.MostrarError);
    }
}
