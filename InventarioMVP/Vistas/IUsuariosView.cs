using InventarioMVP.Entidades;

namespace InventarioMVP.Vistas
{
    /// <summary>Datos capturados en la ventana de alta/edición de usuarios.</summary>
    public record DatosUsuario(string NombreCompleto, string Correo, string NombreUsuario, string Password,
        int RolId, bool Activo, IList<string> Permisos);

    public interface IUsuariosView
    {
        event EventHandler CargarSolicitado;
        event EventHandler NuevoSolicitado;
        event EventHandler EditarSolicitado;
        event EventHandler EstadoSolicitado;
        event EventHandler PasswordSolicitada;

        Usuario? UsuarioSeleccionado { get; }

        void MostrarUsuarios(IList<Usuario> usuarios);

        /// <summary>Abre la ventana modal de alta (existente = null) o edición. Devuelve true si se guardó.</summary>
        bool EditarUsuario(Usuario? existente, IList<Permiso> delegables, IList<string> concedidos, Func<DatosUsuario, string?> guardar);

        /// <summary>Pide una nueva contraseña para el usuario. Devuelve true si se guardó.</summary>
        bool PedirNuevaPassword(Usuario usuario, Func<string, string?> guardar);

        void MostrarError(string mensaje);
        void MostrarInfo(string mensaje);
        bool Confirmar(string mensaje);
    }
}
