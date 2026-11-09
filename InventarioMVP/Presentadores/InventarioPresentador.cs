using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Servicios;
using InventarioMVP.Vistas;

namespace InventarioMVP.Presentadores
{
    public class InventarioPresentador
    {
        private readonly IInventarioView _vista;
        private readonly InventarioService _inventario;

        public InventarioPresentador(IInventarioView vista, InventarioService inventario)
        {
            _vista = vista;
            _inventario = inventario;

            _vista.CargarSolicitado += (_, _) => Ejecutar(() =>
            {
                _vista.ConfigurarPermisos(_inventario.PuedeMover);
                RecargarProductos();
                RecargarMovimientos();
            });
            _vista.BuscarSolicitado += (_, _) => Ejecutar(RecargarProductos);
            _vista.MovimientosFiltrados += (_, _) => Ejecutar(RecargarMovimientos);
            _vista.EntradaSolicitada += (_, _) => Ejecutar(() => Mover(TipoMovimiento.Entrada));
            _vista.SalidaSolicitada += (_, _) => Ejecutar(() => Mover(TipoMovimiento.Salida));
        }

        private void RecargarProductos() => _vista.MostrarProductos(_inventario.Listar(_vista.TextoBusqueda));

        private void RecargarMovimientos() =>
            _vista.MostrarMovimientos(_inventario.Movimientos(_vista.Desde, _vista.Hasta,
                _vista.SoloSeleccionado ? _vista.ProductoSeleccionado?.Id : null));

        private void Mover(string tipo)
        {
            var producto = _vista.ProductoSeleccionado
                ?? throw new ValidacionException("Seleccione un producto de la tabla para registrar el movimiento.");
            bool guardado = _vista.PedirMovimiento(producto, tipo,
                (cantidad, motivo) => Ejecutor.Intentar(() => _inventario.RegistrarMovimiento(producto.Id, tipo, cantidad, motivo)));
            if (!guardado) return;
            RecargarProductos();
            RecargarMovimientos();
            _vista.MostrarInfo(tipo == TipoMovimiento.Entrada ? "Entrada de inventario registrada." : "Salida de inventario registrada.");
        }

        private void Ejecutar(Action accion) => Ejecutor.Ejecutar(accion, _vista.MostrarError);
    }
}
