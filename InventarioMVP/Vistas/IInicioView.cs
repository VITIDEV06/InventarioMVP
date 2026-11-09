using InventarioMVP.Entidades;

namespace InventarioMVP.Vistas
{
    public interface IInicioView
    {
        event EventHandler CargarSolicitado;
        event EventHandler PeriodoCambiado;

        PeriodoIngresos Periodo { get; }

        void MostrarDatos(DashboardDatos datos);
        void MostrarIngresos(decimal total);
        void MostrarError(string mensaje);
    }
}
