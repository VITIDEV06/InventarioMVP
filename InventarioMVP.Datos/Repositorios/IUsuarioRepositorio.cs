using InventarioMVP.Entidades;

namespace InventarioMVP.Datos.Repositorios
{
    public interface IUsuarioRepositorio
    {
        Usuario? ObtenerPorUsuarioOCorreo(string identificador);
        Usuario? ObtenerPorId(int id);
        IList<Usuario> ObtenerTodos();
        bool Existe(string nombreUsuario, string correo, int excluirId = 0);
        int Insertar(Usuario usuario);

        /// <summary>Actualiza nombre completo, correo, rol y estado.</summary>
        void Actualizar(Usuario usuario);
        void ActualizarPerfil(int id, string nombreCompleto, string correo);
        void ActualizarPassword(int id, string passwordHash);
        void RegistrarAcceso(int id);
        int ContarAdministradoresActivos();

        /// <summary>Permisos efectivos: los del rol más los concedidos individualmente.</summary>
        ISet<string> ObtenerPermisos(int usuarioId);
        IList<string> ObtenerPermisosConcedidos(int usuarioId);
        IList<Permiso> ObtenerPermisosDelegables();
        void ReemplazarPermisosConcedidos(int usuarioId, IEnumerable<string> codigos);
    }
}
