using InventarioMVP.Negocio.Servicios;
using InventarioMVP.Vistas;

namespace InventarioMVP.Presentadores
{
    public class InventarioPresentador
    {
        private readonly IInventarioView _vista;
        private readonly ProductoService _productos;

        public InventarioPresentador(IInventarioView vista, ProductoService productos)
        {
            _vista = vista;
            _productos = productos;

            _vista.CargarSolicitado += (_, _) => Ejecutar(Cargar);
            _vista.BuscarSolicitado += (_, _) => Ejecutar(Buscar);
            _vista.NuevoSolicitado += (_, _) => _vista.MostrarEnFormulario(null);
            _vista.GuardarSolicitado += (_, _) => Ejecutar(Guardar);
            _vista.EliminarSolicitado += (_, _) => Ejecutar(Eliminar);
            _vista.SeleccionCambiada += (_, _) =>
            {
                var p = _vista.ProductoSeleccionado;
                if (p != null) _vista.MostrarEnFormulario(p);
            };
        }

        private void Cargar() => _vista.MostrarProductos(_productos.Listar());

        private void Buscar() => _vista.MostrarProductos(_productos.Buscar(_vista.TextoBusqueda));

        private void Guardar()
        {
            var producto = _vista.LeerFormulario();
            bool esNuevo = producto.Id == 0;
            _productos.Guardar(producto);
            Buscar();
            _vista.MostrarEnFormulario(null);
            _vista.MostrarInfo(esNuevo ? "Producto registrado correctamente." : "Producto actualizado correctamente.");
        }

        private void Eliminar()
        {
            var seleccionado = _vista.ProductoSeleccionado;
            if (seleccionado == null)
                throw new ValidacionException("Seleccione un producto de la tabla para eliminarlo.");
            if (!_vista.Confirmar($"¿Eliminar el producto \"{seleccionado.Nombre}\"? Esta acción no se puede deshacer."))
                return;
            _productos.Eliminar(seleccionado.Id);
            Buscar();
            _vista.MostrarEnFormulario(null);
            _vista.MostrarInfo("Producto eliminado.");
        }

        private void Ejecutar(Action accion)
        {
            try { accion(); }
            catch (ValidacionException ex) { _vista.MostrarError(ex.Message); }
            catch (Exception ex) { _vista.MostrarError("Error de base de datos: " + ex.Message); }
        }
    }
}
