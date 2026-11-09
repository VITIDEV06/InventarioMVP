using InventarioMVP.Datos.Repositorios;
using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Seguridad;

namespace InventarioMVP.Negocio.Servicios
{
    public class AutenticacionService
    {
        private const int MaxIntentos = 5;
        private static readonly TimeSpan Bloqueo = TimeSpan.FromMinutes(1);

        private readonly IUsuarioRepositorio _usuarios;
        private readonly IVerificarHashService _verificar;
        private readonly IActividadRepositorio _actividad;
        private readonly SesionUsuario _sesion;
        private readonly Dictionary<string, (int Fallos, DateTime Desde)> _intentos = new();

        public AutenticacionService(IUsuarioRepositorio usuarios, IVerificarHashService verificar,
            IActividadRepositorio actividad, SesionUsuario sesion)
        {
            _usuarios = usuarios;
            _verificar = verificar;
            _actividad = actividad;
            _sesion = sesion;
        }

        /// <summary>
        /// Autentica y abre la sesión (con los permisos efectivos del usuario).
        /// Lanza ValidacionException si los datos son inválidos, incorrectos o la cuenta está desactivada.
        /// </summary>
        public Usuario Autenticar(string identificador, string password)
        {
            identificador = (identificador ?? "").Trim();
            if (identificador.Length == 0) throw new ValidacionException("Ingrese su usuario o correo.");
            if (string.IsNullOrEmpty(password)) throw new ValidacionException("Ingrese su contraseña.");

            string clave = identificador.ToLowerInvariant();
            if (_intentos.TryGetValue(clave, out var estado) && estado.Fallos >= MaxIntentos)
            {
                var restante = estado.Desde + Bloqueo - DateTime.Now;
                if (restante > TimeSpan.Zero)
                    throw new ValidacionException($"Demasiados intentos fallidos. Intente de nuevo en {Math.Ceiling(restante.TotalSeconds)} segundos.");
                _intentos.Remove(clave);
            }

            var usuario = _usuarios.ObtenerPorUsuarioOCorreo(identificador);
            // Mismo mensaje para usuario inexistente y contraseña incorrecta (no revela cuál falló).
            if (usuario == null || !_verificar.Verificar(password, usuario.PasswordHash))
            {
                var fallos = _intentos.TryGetValue(clave, out var e) ? e.Fallos + 1 : 1;
                _intentos[clave] = (fallos, DateTime.Now);
                throw new ValidacionException("Usuario o contraseña incorrectos.");
            }

            _intentos.Remove(clave);
            if (!usuario.Activo)
                throw new ValidacionException("Su cuenta está desactivada. Contacte al administrador.");

            _usuarios.RegistrarAcceso(usuario.Id);
            _sesion.Iniciar(usuario, _usuarios.ObtenerPermisos(usuario.Id));
            _actividad.Registrar(usuario.Id, "INICIO_SESION", "Inicio de sesión");
            usuario.PasswordHash = string.Empty;
            return usuario;
        }

        public void CerrarSesion()
        {
            if (_sesion.EstaAutenticado)
                _actividad.Registrar(_sesion.UsuarioId, "CIERRE_SESION", "Cierre de sesión");
            _sesion.Cerrar();
        }
    }
}
