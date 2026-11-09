using InventarioMVP.Entidades;

namespace InventarioMVP.Vistas
{
    public interface IUsuariosView
    {
        event EventHandler CargarSolicitado;
        event EventHandler RegistrarSolicitado;

        string NombreUsuario { get; }
        string Correo { get; }
        string NombreCompleto { get; }
        string Password { get; }
        string ConfirmarPassword { get; }

        void MostrarUsuarios(IList<Usuario> usuarios);
        void LimpiarFormulario();
        void MostrarError(string mensaje);
        void MostrarInfo(string mensaje);
    }
}
