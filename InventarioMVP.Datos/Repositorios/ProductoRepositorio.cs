using InventarioMVP.Entidades;
using MySql.Data.MySqlClient;

namespace InventarioMVP.Datos.Repositorios
{
    public class ProductoRepositorio : IProductoRepositorio
    {
        private const string Columnas = "id, codigo, nombre, descripcion, categoria, precio, stock";
        private readonly ConexionMySql _conexion;

        public ProductoRepositorio(ConexionMySql conexion) => _conexion = conexion;

        public IList<Producto> ObtenerTodos()
        {
            using var cn = _conexion.Abrir();
            using var cmd = new MySqlCommand("SELECT " + Columnas + " FROM productos ORDER BY nombre", cn);
            return Leer(cmd);
        }

        public IList<Producto> Buscar(string texto)
        {
            using var cn = _conexion.Abrir();
            using var cmd = new MySqlCommand(
                "SELECT " + Columnas + " FROM productos " +
                "WHERE codigo LIKE @t OR nombre LIKE @t OR categoria LIKE @t OR descripcion LIKE @t " +
                "ORDER BY nombre", cn);
            cmd.Parameters.AddWithValue("@t", "%" + EscaparLike(texto) + "%");
            return Leer(cmd);
        }

        public bool ExisteCodigo(string codigo, int excluirId)
        {
            using var cn = _conexion.Abrir();
            using var cmd = new MySqlCommand(
                "SELECT COUNT(*) FROM productos WHERE codigo = @c AND id <> @id", cn);
            cmd.Parameters.AddWithValue("@c", codigo);
            cmd.Parameters.AddWithValue("@id", excluirId);
            return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
        }

        public int Insertar(Producto p)
        {
            using var cn = _conexion.Abrir();
            using var cmd = new MySqlCommand(
                "INSERT INTO productos (codigo, nombre, descripcion, categoria, precio, stock) " +
                "VALUES (@codigo, @nombre, @descripcion, @categoria, @precio, @stock); SELECT LAST_INSERT_ID();", cn);
            Parametros(cmd, p);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public bool Actualizar(Producto p)
        {
            using var cn = _conexion.Abrir();
            using var cmd = new MySqlCommand(
                "UPDATE productos SET codigo=@codigo, nombre=@nombre, descripcion=@descripcion, " +
                "categoria=@categoria, precio=@precio, stock=@stock WHERE id=@id", cn);
            Parametros(cmd, p);
            cmd.Parameters.AddWithValue("@id", p.Id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Eliminar(int id)
        {
            using var cn = _conexion.Abrir();
            using var cmd = new MySqlCommand("DELETE FROM productos WHERE id = @id", cn);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        private static void Parametros(MySqlCommand cmd, Producto p)
        {
            cmd.Parameters.AddWithValue("@codigo", p.Codigo);
            cmd.Parameters.AddWithValue("@nombre", p.Nombre);
            cmd.Parameters.AddWithValue("@descripcion", p.Descripcion);
            cmd.Parameters.AddWithValue("@categoria", p.Categoria);
            cmd.Parameters.AddWithValue("@precio", p.Precio);
            cmd.Parameters.AddWithValue("@stock", p.Stock);
        }

        private static string EscaparLike(string texto) =>
            texto.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

        private static IList<Producto> Leer(MySqlCommand cmd)
        {
            var lista = new List<Producto>();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                lista.Add(new Producto
                {
                    Id = rd.GetInt32("id"),
                    Codigo = rd.GetString("codigo"),
                    Nombre = rd.GetString("nombre"),
                    Descripcion = rd.IsDBNull(rd.GetOrdinal("descripcion")) ? "" : rd.GetString("descripcion"),
                    Categoria = rd.GetString("categoria"),
                    Precio = rd.GetDecimal("precio"),
                    Stock = rd.GetInt32("stock")
                });
            }
            return lista;
        }
    }
}
