using InventarioMVP.Datos.Repositorios;
using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Seguridad;

namespace InventarioMVP.Negocio.Servicios
{
    public class AutenticacionService
    {
        private readonly IUsuarioRepositorio _usuarios;
        private readonly IVerificarHashService _verificar;

        public AutenticacionService(IUsuarioRepositorio usuarios, IVerificarHashService verificar)
        {
            _usuarios = usuarios;
            _verificar = verificar;
        }

        /// <summary>Devuelve el usuario autenticado. Lanza ValidacionException si los datos son inválidos o incorrectos.</summary>
        public Usuario Autenticar(string identificador, string password)
        {
            identificador = (identificador ?? "").Trim();
            if (identificador.Length == 0) throw new ValidacionException("Ingrese su usuario o correo.");
            if (string.IsNullOrEmpty(password)) throw new ValidacionException("Ingrese su contraseña.");

            var usuario = _usuarios.ObtenerPorUsuarioOCorreo(identificador);
            // Mismo mensaje para usuario inexistente y contraseña incorrecta (no revela cuál falló).
            if (usuario == null || !_verificar.Verificar(password, usuario.PasswordHash))
                throw new ValidacionException("Usuario o contraseña incorrectos.");
            return usuario;
        }
    }
}
