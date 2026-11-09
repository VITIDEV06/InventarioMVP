using InventarioMVP.Entidades;
using MySql.Data.MySqlClient;

namespace InventarioMVP.Datos.Repositorios
{
    public interface IClienteRepositorio
    {
        IList<Cliente> ObtenerTodos();
        IList<Cliente> Buscar(string texto);
        Cliente? ObtenerPorId(int id);
        bool ExisteCorreo(string correo, int excluirId);
        int Insertar(Cliente cliente);
        bool Actualizar(Cliente cliente);
    }

    public class ClienteRepositorio : IClienteRepositorio
    {
        private const string Seleccion =
            "SELECT c.id, c.nombre_completo, c.telefono, c.correo, c.direccion, c.observaciones, c.fecha_registro, " +
            "COUNT(v.id) AS num_compras, COALESCE(SUM(v.total), 0) AS total_comprado, MAX(v.fecha) AS ultima_compra " +
            "FROM clientes c LEFT JOIN ventas v ON v.cliente_id = c.id ";

        private const string Agrupar = "GROUP BY c.id ORDER BY c.nombre_completo";

        private readonly ConexionMySql _db;

        public ClienteRepositorio(ConexionMySql db) => _db = db;

        public IList<Cliente> ObtenerTodos() => _db.Consultar(Seleccion + Agrupar, Mapear);

        public IList<Cliente> Buscar(string texto) =>
            _db.Consultar(
                Seleccion + "WHERE c.nombre_completo LIKE @t OR c.telefono LIKE @t OR c.correo LIKE @t " + Agrupar,
                Mapear, ("@t", "%" + texto.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%"));

        public Cliente? ObtenerPorId(int id) =>
            _db.Consultar(Seleccion + "WHERE c.id = @id " + Agrupar, Mapear, ("@id", id)).FirstOrDefault();

        public bool ExisteCorreo(string correo, int excluirId) =>
            Convert.ToInt64(_db.Escalar(
                "SELECT COUNT(*) FROM clientes WHERE correo = @c AND id <> @id", ("@c", correo), ("@id", excluirId))) > 0;

        public int Insertar(Cliente c) => _db.Insertar(
            "INSERT INTO clientes (nombre_completo, telefono, correo, direccion, observaciones) " +
            "VALUES (@n, @t, @c, @d, @o)", Parametros(c));

        public bool Actualizar(Cliente c) => _db.Ejecutar(
            "UPDATE clientes SET nombre_completo = @n, telefono = @t, correo = @c, direccion = @d, observaciones = @o " +
            "WHERE id = @id", Parametros(c).Append(("@id", (object?)c.Id)).ToArray()) > 0;

        private static (string, object?)[] Parametros(Cliente c) => new (string, object?)[]
        {
            ("@n", c.NombreCompleto), ("@t", Nulo(c.Telefono)), ("@c", Nulo(c.Correo)),
            ("@d", Nulo(c.Direccion)), ("@o", Nulo(c.Observaciones))
        };

        private static string? Nulo(string texto) => string.IsNullOrWhiteSpace(texto) ? null : texto;

        private static Cliente Mapear(MySqlDataReader rd) => new()
        {
            Id = rd.GetInt32("id"),
            NombreCompleto = rd.GetString("nombre_completo"),
            Telefono = rd.Texto("telefono"),
            Correo = rd.Texto("correo"),
            Direccion = rd.Texto("direccion"),
            Observaciones = rd.Texto("observaciones"),
            FechaRegistro = rd.GetDateTime("fecha_registro"),
            NumCompras = rd.Entero("num_compras"),
            TotalComprado = rd.Decimal("total_comprado"),
            UltimaCompra = rd.FechaNula("ultima_compra")
        };
    }
}
