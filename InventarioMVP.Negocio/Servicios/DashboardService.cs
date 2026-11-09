using InventarioMVP.Datos.Repositorios;
using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Seguridad;

namespace InventarioMVP.Negocio.Servicios
{
    public class DashboardService
    {
        private const int DiasGrafico = 14;

        private readonly IReporteRepositorio _reportes;
        private readonly IActividadRepositorio _actividad;
        private readonly IProductoRepositorio _productos;
        private readonly SesionUsuario _sesion;

        public DashboardService(IReporteRepositorio reportes, IActividadRepositorio actividad,
            IProductoRepositorio productos, SesionUsuario sesion)
        {
            _reportes = reportes;
            _actividad = actividad;
            _productos = productos;
            _sesion = sesion;
        }

        /// <summary>Dashboard completo (administrador) o limitado a las operaciones propias (usuario).</summary>
        public DashboardDatos Obtener(PeriodoIngresos periodo)
        {
            if (!_sesion.EstaAutenticado) throw new AccesoDenegadoException("Debe iniciar sesión.");
            bool general = _sesion.Tiene(Permisos.DashboardGeneral);
            int? usuario = general ? null : _sesion.UsuarioId;

            var hoy = DateTime.Today;
            var inicioGrafico = hoy.AddDays(-(DiasGrafico - 1));
            var puntos = _reportes.VentasPorDia(inicioGrafico, hoy.AddDays(1), usuario).ToDictionary(p => p.Dia.Date, p => p.Total);

            var datos = new DashboardDatos
            {
                EsGeneral = general,
                TotalProductos = _reportes.ContarProductos(),
                ProductosStockBajo = _reportes.ContarProductosStockBajo(),
                TotalClientes = _reportes.ContarClientes(),
                ClientesNuevos = _reportes.ContarClientesNuevos(hoy.AddDays(-30)),
                Ingresos = Ingresos(periodo),
                VentasPorDia = Enumerable.Range(0, DiasGrafico)
                    .Select(i => inicioGrafico.AddDays(i))
                    .Select(d => new PuntoVenta(d, puntos.GetValueOrDefault(d))).ToList(),
                UltimasVentas = _reportes.UltimasVentas(6, usuario).ToList(),
                MasVendidos = _reportes.MasVendidos(5, new DateTime(2000, 1, 1), hoy.AddDays(1), usuario).ToList(),
                StockBajo = _productos.ObtenerConStockBajo(6).ToList(),
                Actividad = general ? _actividad.ObtenerRecientes(8).ToList() : new List<Actividad>()
            };
            return datos;
        }

        /// <summary>Ingresos del período indicado (de todos los usuarios o solo los propios, según el permiso).</summary>
        public decimal Ingresos(PeriodoIngresos periodo)
        {
            if (!_sesion.EstaAutenticado) throw new AccesoDenegadoException("Debe iniciar sesión.");
            int? usuario = _sesion.Tiene(Permisos.DashboardGeneral) ? null : _sesion.UsuarioId;
            var (desde, hasta) = Limites(periodo, DateTime.Today);
            return _reportes.Ingresos(desde, hasta, usuario);
        }

        public static (DateTime Desde, DateTime HastaExclusivo) Limites(PeriodoIngresos periodo, DateTime hoy) => periodo switch
        {
            PeriodoIngresos.Hoy => (hoy, hoy.AddDays(1)),
            PeriodoIngresos.Semana => (hoy.AddDays(-(((int)hoy.DayOfWeek + 6) % 7)), hoy.AddDays(1)), // desde el lunes
            PeriodoIngresos.Mes => (new DateTime(hoy.Year, hoy.Month, 1), hoy.AddDays(1)),
            PeriodoIngresos.Anio => (new DateTime(hoy.Year, 1, 1), hoy.AddDays(1)),
            _ => (new DateTime(2000, 1, 1), hoy.AddDays(1))
        };
    }
}
