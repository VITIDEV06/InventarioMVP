using InventarioMVP.Entidades;

namespace InventarioMVP.Datos.Repositorios
{
    public interface IProductoRepositorio
    {
        IList<Producto> ObtenerTodos();
        IList<Producto> Buscar(string texto);
        bool ExisteCodigo(string codigo, int excluirId);
        int Insertar(Producto producto);
        bool Actualizar(Producto producto);
        bool Eliminar(int id);
    }
}
