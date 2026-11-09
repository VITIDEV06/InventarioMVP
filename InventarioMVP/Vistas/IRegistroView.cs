namespace InventarioMVP.Vistas
{
    public interface IRegistroView
    {
        string NombreCompleto { get; }
        string Correo { get; }
        string NombreUsuario { get; }
        string Password { get; }
        string Confirmacion { get; }

        event EventHandler RegistrarSolicitado;
        void MostrarError(string mensaje);
    }
}
