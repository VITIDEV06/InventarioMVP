using System.Net.Mail;
using InventarioMVP.Datos.Repositorios;
using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Seguridad;

namespace InventarioMVP.Negocio.Servicios
{
    public class UsuarioService
    {
        private readonly IUsuarioRepositorio _usuarios;
        private readonly IHashService _hash;

        public UsuarioService(IUsuarioRepositorio usuarios, IHashService hash)
        {
            _usuarios = usuarios;
            _hash = hash;
        }

        public IList<Usuario> Listar() => _usuarios.ObtenerTodos();

        public void Registrar(string nombreUsuario, string correo, string nombreCompleto, string password)
        {
            nombreUsuario = (nombreUsuario ?? "").Trim();
            correo = (correo ?? "").Trim();
            nombreCompleto = (nombreCompleto ?? "").Trim();

            if (nombreUsuario.Length < 3 || nombreUsuario.Length > 50)
                throw new ValidacionException("El usuario debe tener entre 3 y 50 caracteres.");
            if (nombreUsuario.Contains('@') || nombreUsuario.Contains(' '))
                throw new ValidacionException("El usuario no puede contener espacios ni '@'.");
            if (!CorreoValido(correo))
                throw new ValidacionException("El correo electrónico no es válido.");
            if (nombreCompleto.Length == 0 || nombreCompleto.Length > 100)
                throw new ValidacionException("El nombre completo es obligatorio (máximo 100 caracteres).");
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                throw new ValidacionException("La contraseña debe tener al menos 8 caracteres.");
            if (_usuarios.Existe(nombreUsuario, correo))
                throw new ValidacionException("Ya existe un usuario con ese nombre o correo.");

            _usuarios.Insertar(new Usuario
            {
                NombreUsuario = nombreUsuario,
                Correo = correo,
                NombreCompleto = nombreCompleto,
                PasswordHash = _hash.GenerarHash(password)
            });
        }

        private static bool CorreoValido(string correo)
        {
            if (correo.Length == 0 || correo.Length > 120) return false;
            try { return new MailAddress(correo).Address == correo; }
            catch (FormatException) { return false; }
        }
    }
}
