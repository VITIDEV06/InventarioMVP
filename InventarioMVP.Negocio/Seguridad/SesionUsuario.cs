using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Servicios;

namespace InventarioMVP.Negocio.Seguridad
{
    /// <summary>Se lanza cuando el usuario no tiene permiso para la operación. Es un error de validación apto para mostrar.</summary>
    public class AccesoDenegadoException : ValidacionException
    {
        public AccesoDenegadoException(string mensaje = "No tiene permiso para realizar esta operación.") : base(mensaje) { }
    }

    /// <summary>
    /// Sesión del usuario autenticado, solo en memoria. Guarda sus permisos efectivos y NUNCA su hash de contraseña.
    /// Todos los servicios de Negocio consultan esta sesión: el control de acceso no depende de la interfaz.
    /// </summary>
    public class SesionUsuario
    {
        private Usuario? _usuario;
        private HashSet<string> _permisos = new();

        public bool EstaAutenticado => _usuario != null;

        public Usuario Usuario => _usuario ?? throw new AccesoDenegadoException("Debe iniciar sesión.");

        public int UsuarioId => Usuario.Id;

        public bool EsAdministrador => _usuario?.EsAdministrador == true;

        public bool Tiene(string permiso) => _usuario != null && _permisos.Contains(permiso);

        /// <summary>Lanza AccesoDenegadoException si no hay sesión o falta el permiso.</summary>
        public void Exigir(string permiso)
        {
            if (_usuario == null) throw new AccesoDenegadoException("Debe iniciar sesión.");
            if (!_permisos.Contains(permiso)) throw new AccesoDenegadoException();
        }

        public void Iniciar(Usuario usuario, IEnumerable<string> permisos)
        {
            _usuario = new Usuario
            {
                Id = usuario.Id, NombreUsuario = usuario.NombreUsuario, Correo = usuario.Correo,
                NombreCompleto = usuario.NombreCompleto, RolId = usuario.RolId, Rol = usuario.Rol,
                Activo = usuario.Activo, UltimoAcceso = usuario.UltimoAcceso, FechaCreacion = usuario.FechaCreacion,
                PasswordHash = string.Empty // el hash no se conserva en memoria de sesión
            };
            _permisos = permisos.ToHashSet();
        }

        public void ActualizarDatos(string nombreCompleto, string correo)
        {
            Usuario.NombreCompleto = nombreCompleto;
            Usuario.Correo = correo;
        }

        public void Cerrar()
        {
            _usuario = null;
            _permisos = new HashSet<string>();
        }
    }
}
