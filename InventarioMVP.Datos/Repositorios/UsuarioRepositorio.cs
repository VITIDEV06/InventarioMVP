using InventarioMVP.Entidades;
using MySql.Data.MySqlClient;

namespace InventarioMVP.Datos.Repositorios
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private const string Seleccion =
            "SELECT u.id, u.nombre_usuario, u.correo, u.nombre_completo, u.password_hash, u.fecha_creacion, " +
            "u.rol_id, u.activo, u.ultimo_acceso, r.nombre AS rol " +
            "FROM usuarios u JOIN roles r ON r.id = u.rol_id ";

        private readonly ConexionMySql _db;

        public UsuarioRepositorio(ConexionMySql db) => _db = db;

        public Usuario? ObtenerPorUsuarioOCorreo(string identificador) =>
            _db.Consultar(Seleccion + "WHERE u.nombre_usuario = @id OR u.correo = @id LIMIT 1", Mapear, ("@id", identificador))
               .FirstOrDefault();

        public Usuario? ObtenerPorId(int id) =>
            _db.Consultar(Seleccion + "WHERE u.id = @id", Mapear, ("@id", id)).FirstOrDefault();

        public IList<Usuario> ObtenerTodos() =>
            _db.Consultar(Seleccion + "ORDER BY u.nombre_usuario", Mapear);

        public bool Existe(string nombreUsuario, string correo, int excluirId = 0) =>
            Convert.ToInt64(_db.Escalar(
                "SELECT COUNT(*) FROM usuarios WHERE (nombre_usuario = @u OR correo = @c) AND id <> @x",
                ("@u", nombreUsuario), ("@c", correo), ("@x", excluirId))) > 0;

        public int Insertar(Usuario u) => _db.Insertar(
            "INSERT INTO usuarios (nombre_usuario, correo, nombre_completo, password_hash, rol_id, activo) " +
            "VALUES (@u, @c, @n, @h, @r, @a)",
            ("@u", u.NombreUsuario), ("@c", u.Correo), ("@n", u.NombreCompleto), ("@h", u.PasswordHash),
            ("@r", u.RolId), ("@a", u.Activo));

        public void Actualizar(Usuario u) => _db.Ejecutar(
            "UPDATE usuarios SET nombre_completo = @n, correo = @c, rol_id = @r, activo = @a WHERE id = @id",
            ("@n", u.NombreCompleto), ("@c", u.Correo), ("@r", u.RolId), ("@a", u.Activo), ("@id", u.Id));

        public void ActualizarPerfil(int id, string nombreCompleto, string correo) => _db.Ejecutar(
            "UPDATE usuarios SET nombre_completo = @n, correo = @c WHERE id = @id",
            ("@n", nombreCompleto), ("@c", correo), ("@id", id));

        public void ActualizarPassword(int id, string passwordHash) => _db.Ejecutar(
            "UPDATE usuarios SET password_hash = @h WHERE id = @id", ("@h", passwordHash), ("@id", id));

        public void RegistrarAcceso(int id) => _db.Ejecutar(
            "UPDATE usuarios SET ultimo_acceso = NOW() WHERE id = @id", ("@id", id));

        public int ContarAdministradoresActivos() =>
            Convert.ToInt32(_db.Escalar("SELECT COUNT(*) FROM usuarios WHERE rol_id = 1 AND activo = 1"));

        public ISet<string> ObtenerPermisos(int usuarioId) =>
            _db.Consultar(
                "SELECT p.codigo FROM permisos p " +
                "JOIN rol_permisos rp ON rp.permiso_id = p.id " +
                "JOIN usuarios u ON u.rol_id = rp.rol_id WHERE u.id = @id " +
                "UNION " +
                "SELECT p.codigo FROM permisos p " +
                "JOIN usuario_permisos up ON up.permiso_id = p.id WHERE up.usuario_id = @id",
                rd => rd.GetString(0), ("@id", usuarioId)).ToHashSet();

        public IList<string> ObtenerPermisosConcedidos(int usuarioId) =>
            _db.Consultar(
                "SELECT p.codigo FROM permisos p JOIN usuario_permisos up ON up.permiso_id = p.id " +
                "WHERE up.usuario_id = @id", rd => rd.GetString(0), ("@id", usuarioId));

        public IList<Permiso> ObtenerPermisosDelegables() =>
            _db.Consultar("SELECT codigo, descripcion FROM permisos WHERE delegable = 1 ORDER BY id",
                rd => new Permiso { Codigo = rd.GetString("codigo"), Descripcion = rd.GetString("descripcion") });

        public void ReemplazarPermisosConcedidos(int usuarioId, IEnumerable<string> codigos) =>
            _db.EnTransaccion(op =>
            {
                op.Ejecutar("DELETE FROM usuario_permisos WHERE usuario_id = @u", ("@u", usuarioId));
                foreach (var codigo in codigos.Distinct())
                    op.Ejecutar(
                        "INSERT INTO usuario_permisos (usuario_id, permiso_id) " +
                        "SELECT @u, id FROM permisos WHERE codigo = @c AND delegable = 1",
                        ("@u", usuarioId), ("@c", codigo));
            });

        private static Usuario Mapear(MySqlDataReader rd) => new()
        {
            Id = rd.GetInt32("id"),
            NombreUsuario = rd.GetString("nombre_usuario"),
            Correo = rd.GetString("correo"),
            NombreCompleto = rd.GetString("nombre_completo"),
            PasswordHash = rd.GetString("password_hash"),
            FechaCreacion = rd.GetDateTime("fecha_creacion"),
            RolId = rd.GetInt32("rol_id"),
            Rol = rd.GetString("rol"),
            Activo = rd.GetBoolean("activo"),
            UltimoAcceso = rd.FechaNula("ultimo_acceso")
        };
    }
}
