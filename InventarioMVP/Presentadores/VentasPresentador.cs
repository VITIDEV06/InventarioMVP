using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Servicios;
using InventarioMVP.Vistas;

namespace InventarioMVP.Presentadores
{
    public class VentasPresentador
    {
        private readonly IVentasView _vista;
        private readonly VentaService _ventas;

        public VentasPresentador(IVentasView vista, VentaService ventas)
        {
            _vista = vista;
            _ventas = ventas;

            _vista.CargarSolicitado += (_, _) => Ejecutar(() =>
            {
                _vista.ConfigurarPermisos(_ventas.PuedeRegistrar, _ventas.VeTodas);
                Recargar();
            });
            _vista.BuscarSolicitado += (_, _) => Ejecutar(Recargar);
            _vista.NuevaVentaSolicitada += (_, _) => Ejecutar(NuevaVenta);
            _vista.DetalleSolicitado += (_, _) => Ejecutar(Detalle);
        }

        private void Recargar() =>
            _vista.MostrarVentas(_ventas.Historial(_vista.TextoBusqueda, _vista.Desde, _vista.Hasta));

        private void NuevaVenta()
        {
            var productos = _ventas.ProductosParaVenta();
            if (productos.Count == 0) throw new ValidacionException("No hay productos con existencias para vender.");
            var clientes = _ventas.ClientesParaVenta();

            Venta? registrada = null;
            bool guardada = _vista.PedirNuevaVenta(productos, clientes, (clienteId, ocasional, lineas) =>
                Ejecutor.Intentar(() => registrada = _ventas.Registrar(clienteId, ocasional, lineas)));
            if (!guardada || registrada == null) return;

            Recargar();
            _vista.MostrarInfo($"Venta {registrada.Numero} registrada por {Formato.Dinero(registrada.Total)}.");
        }

        private void Detalle()
        {
            var seleccionada = _vista.VentaSeleccionada ?? throw new ValidacionException("Seleccione una venta de la tabla.");
            _vista.MostrarDetalle(_ventas.ObtenerDetalle(seleccionada.Id));
        }

        private void Ejecutar(Action accion) => Ejecutor.Ejecutar(accion, _vista.MostrarError);
    }
}
