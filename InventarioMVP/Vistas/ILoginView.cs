using InventarioMVP.Entidades;

namespace InventarioMVP.Vistas
{
    /// <summary>Contrato de la vista de login: solo datos de entrada/salida y eventos.</summary>
    public interface ILoginView
    {
        string Identificador { get; }
        string Password { get; }
        event EventHandler IniciarSesionSolicitado;
        void MostrarError(string mensaje);
        void LimpiarPassword();
        void Ocupado(bool ocupado);
        void CompletarInicioSesion(Usuario usuario);
    }
}
