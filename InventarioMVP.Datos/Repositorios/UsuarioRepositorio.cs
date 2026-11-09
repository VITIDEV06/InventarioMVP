using InventarioMVP.Entidades;
using MySql.Data.MySqlClient;

namespace InventarioMVP.Datos.Repositorios
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private const string Columnas = "id, nombre_usuario, correo, nombre_completo, password_hash, fecha_creacion";
        private readonly ConexionMySql _conexion;

        public UsuarioRepositorio(ConexionMySql conexion) => _conexion = conexion;

        public Usuario? ObtenerPorUsuarioOCorreo(string identificador)
        {
            using var cn = _conexion.Abrir();
            using var cmd = new MySqlCommand(
                "SELECT " + Columnas + " FROM usuarios WHERE nombre_usuario = @id OR correo = @id LIMIT 1", cn);
            cmd.Parameters.AddWithValue("@id", identificador);
            using var rd = cmd.ExecuteReader();
            return rd.Read() ? Mapear(rd) : null;
        }

        public IList<Usuario> ObtenerTodos()
        {
            var lista = new List<Usuario>();
            using var cn = _conexion.Abrir();
            using var cmd = new MySqlCommand("SELECT " + Columnas + " FROM usuarios ORDER BY nombre_usuario", cn);
            using var rd = cmd.ExecuteReader();
            while (rd.Read()) lista.Add(Mapear(rd));
            return lista;
        }

        public bool Existe(string nombreUsuario, string correo)
        {
            using var cn = _conexion.Abrir();
            using var cmd = new MySqlCommand(
                "SELECT COUNT(*) FROM usuarios WHERE nombre_usuario = @u OR correo = @c", cn);
            cmd.Parameters.AddWithValue("@u", nombreUsuario);
            cmd.Parameters.AddWithValue("@c", correo);
            return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
        }

        public int Insertar(Usuario u)
        {
            using var cn = _conexion.Abrir();
            using var cmd = new MySqlCommand(
                "INSERT INTO usuarios (nombre_usuario, correo, nombre_completo, password_hash) " +
                "VALUES (@u, @c, @n, @h); SELECT LAST_INSERT_ID();", cn);
            cmd.Parameters.AddWithValue("@u", u.NombreUsuario);
            cmd.Parameters.AddWithValue("@c", u.Correo);
            cmd.Parameters.AddWithValue("@n", u.NombreCompleto);
            cmd.Parameters.AddWithValue("@h", u.PasswordHash);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private static Usuario Mapear(MySqlDataReader rd) => new()
        {
            Id = rd.GetInt32("id"),
            NombreUsuario = rd.GetString("nombre_usuario"),
            Correo = rd.GetString("correo"),
            NombreCompleto = rd.GetString("nombre_completo"),
            PasswordHash = rd.GetString("password_hash"),
            FechaCreacion = rd.GetDateTime("fecha_creacion")
        };
    }
}
