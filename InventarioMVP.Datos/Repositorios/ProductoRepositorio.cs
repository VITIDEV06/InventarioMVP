using InventarioMVP.Entidades;
using MySql.Data.MySqlClient;

namespace InventarioMVP.Datos.Repositorios
{
    public class ProductoRepositorio : IProductoRepositorio
    {
        private const string Seleccion =
            "SELECT id, codigo, nombre, descripcion, categoria, precio, stock, stock_minimo FROM productos ";

        private readonly ConexionMySql _db;

        public ProductoRepositorio(ConexionMySql db) => _db = db;

        public IList<Producto> ObtenerTodos() =>
            _db.Consultar(Seleccion + "WHERE activo = 1 ORDER BY nombre", Mapear);

        public IList<Producto> Buscar(string texto) =>
            _db.Consultar(
                Seleccion + "WHERE activo = 1 AND (codigo LIKE @t OR nombre LIKE @t OR categoria LIKE @t OR descripcion LIKE @t) " +
                "ORDER BY nombre", Mapear, ("@t", "%" + EscaparLike(texto) + "%"));

        public Producto? ObtenerPorId(int id) =>
            _db.Consultar(Seleccion + "WHERE id = @id AND activo = 1", Mapear, ("@id", id)).FirstOrDefault();

        public IList<Producto> ObtenerConStockBajo(int limite) =>
            _db.Consultar(
                Seleccion + "WHERE activo = 1 AND stock <= stock_minimo ORDER BY stock, nombre LIMIT @l",
                Mapear, ("@l", limite));

        public bool ExisteCodigo(string codigo, int excluirId) =>
            Convert.ToInt64(_db.Escalar(
                "SELECT COUNT(*) FROM productos WHERE codigo = @c AND id <> @id", ("@c", codigo), ("@id", excluirId))) > 0;

        public int Insertar(Producto p, int usuarioId) =>
            _db.EnTransaccion(op =>
            {
                int id = op.Insertar(
                    "INSERT INTO productos (codigo, nombre, descripcion, categoria, precio, stock, stock_minimo) " +
                    "VALUES (@codigo, @nombre, @descripcion, @categoria, @precio, @stock, @minimo)",
                    ("@codigo", p.Codigo), ("@nombre", p.Nombre), ("@descripcion", p.Descripcion),
                    ("@categoria", p.Categoria), ("@precio", p.Precio), ("@stock", p.Stock), ("@minimo", p.StockMinimo));

                if (p.Stock > 0)
                    op.Ejecutar(
                        "INSERT INTO movimientos_inventario (producto_id, tipo, cantidad, stock_anterior, stock_nuevo, motivo, usuario_id) " +
                        "VALUES (@p, 'ENTRADA', @c, 0, @c, 'Stock inicial', @u)",
                        ("@p", id), ("@c", p.Stock), ("@u", usuarioId));
                return id;
            });

        public bool Actualizar(Producto p) => _db.Ejecutar(
            "UPDATE productos SET codigo=@codigo, nombre=@nombre, descripcion=@descripcion, categoria=@categoria, " +
            "precio=@precio, stock_minimo=@minimo WHERE id=@id AND activo = 1",
            ("@codigo", p.Codigo), ("@nombre", p.Nombre), ("@descripcion", p.Descripcion),
            ("@categoria", p.Categoria), ("@precio", p.Precio), ("@minimo", p.StockMinimo), ("@id", p.Id)) > 0;

        public bool DarDeBaja(int id) =>
            _db.Ejecutar("UPDATE productos SET activo = 0 WHERE id = @id AND activo = 1", ("@id", id)) > 0;

        private static string EscaparLike(string texto) =>
            texto.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

        private static Producto Mapear(MySqlDataReader rd) => new()
        {
            Id = rd.GetInt32("id"),
            Codigo = rd.GetString("codigo"),
            Nombre = rd.GetString("nombre"),
            Descripcion = rd.Texto("descripcion"),
            Categoria = rd.GetString("categoria"),
            Precio = rd.GetDecimal("precio"),
            Stock = rd.GetInt32("stock"),
            StockMinimo = rd.GetInt32("stock_minimo")
        };
    }
}
