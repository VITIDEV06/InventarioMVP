using InventarioMVP.Entidades;

namespace InventarioMVP.Datos.Repositorios
{
    public interface IProductoRepositorio
    {
        /// <summary>Solo productos activos.</summary>
        IList<Producto> ObtenerTodos();
        IList<Producto> Buscar(string texto);
        Producto? ObtenerPorId(int id);
        IList<Producto> ObtenerConStockBajo(int limite);

        /// <summary>Incluye también productos dados de baja (el código es único en toda la tabla).</summary>
        bool ExisteCodigo(string codigo, int excluirId);

        /// <summary>Inserta el producto y, si tiene stock inicial, registra el movimiento de entrada (transacción).</summary>
        int Insertar(Producto producto, int usuarioId);

        /// <summary>Actualiza los datos del producto. El stock solo cambia mediante movimientos.</summary>
        bool Actualizar(Producto producto);

        /// <summary>Baja lógica: conserva el historial de ventas y movimientos.</summary>
        bool DarDeBaja(int id);
    }
}
