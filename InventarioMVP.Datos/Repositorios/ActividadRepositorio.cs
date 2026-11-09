using InventarioMVP.Entidades;

namespace InventarioMVP.Datos.Repositorios
{
    public interface IActividadRepositorio
    {
        void Registrar(int? usuarioId, string accion, string? detalle);
        IList<Actividad> ObtenerRecientes(int limite);
    }

    public class ActividadRepositorio : IActividadRepositorio
    {
        private readonly ConexionMySql _db;

        public ActividadRepositorio(ConexionMySql db) => _db = db;

        public void Registrar(int? usuarioId, string accion, string? detalle) => _db.Ejecutar(
            "INSERT INTO actividad (usuario_id, accion, detalle) VALUES (@u, @a, @d)",
            ("@u", usuarioId), ("@a", accion), ("@d", detalle));

        public IList<Actividad> ObtenerRecientes(int limite) => _db.Consultar(
            "SELECT a.id, COALESCE(u.nombre_completo, 'Sistema') AS usuario, a.accion, a.detalle, a.fecha " +
            "FROM actividad a LEFT JOIN usuarios u ON u.id = a.usuario_id ORDER BY a.fecha DESC, a.id DESC LIMIT @l",
            rd => new Actividad
            {
                Id = rd.GetInt64("id"), UsuarioNombre = rd.GetString("usuario"), Accion = rd.GetString("accion"),
                Detalle = rd.Texto("detalle"), Fecha = rd.GetDateTime("fecha")
            }, ("@l", limite));
    }

    public interface IConfiguracionRepositorio
    {
        string? Obtener(string clave);
        void Guardar(string clave, string valor);
    }

    public class ConfiguracionRepositorio : IConfiguracionRepositorio
    {
        private readonly ConexionMySql _db;

        public ConfiguracionRepositorio(ConexionMySql db) => _db = db;

        public string? Obtener(string clave) =>
            _db.Escalar("SELECT valor FROM configuracion WHERE clave = @c", ("@c", clave)) as string;

        public void Guardar(string clave, string valor) => _db.Ejecutar(
            "INSERT INTO configuracion (clave, valor) VALUES (@c, @v) ON DUPLICATE KEY UPDATE valor = VALUES(valor)",
            ("@c", clave), ("@v", valor));
    }
}
