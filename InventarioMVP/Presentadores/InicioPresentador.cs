using InventarioMVP.Negocio.Servicios;
using InventarioMVP.Vistas;

namespace InventarioMVP.Presentadores
{
    public class InicioPresentador
    {
        private readonly IInicioView _vista;
        private readonly DashboardService _dashboard;

        public InicioPresentador(IInicioView vista, DashboardService dashboard)
        {
            _vista = vista;
            _dashboard = dashboard;
            _vista.CargarSolicitado += (_, _) =>
                Ejecutor.Ejecutar(() => _vista.MostrarDatos(_dashboard.Obtener(_vista.Periodo)), _vista.MostrarError);
            _vista.PeriodoCambiado += (_, _) =>
                Ejecutor.Ejecutar(() => _vista.MostrarIngresos(_dashboard.Ingresos(_vista.Periodo)), _vista.MostrarError);
        }
    }
}
