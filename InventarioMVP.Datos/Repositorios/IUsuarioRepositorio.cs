using InventarioMVP.Entidades;

namespace InventarioMVP.Datos.Repositorios
{
    public interface IUsuarioRepositorio
    {
        Usuario? ObtenerPorUsuarioOCorreo(string identificador);
        IList<Usuario> ObtenerTodos();
        bool Existe(string nombreUsuario, string correo);
        int Insertar(Usuario usuario);
    }
}
