using InventarioMVP.Datos.Repositorios;
using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Seguridad;

namespace InventarioMVP.Negocio.Servicios
{
    public class UsuarioService
    {
        private readonly IUsuarioRepositorio _usuarios;
        private readonly IHashService _hash;
        private readonly IVerificarHashService _verificar;
        private readonly IActividadRepositorio _actividad;
        private readonly SesionUsuario _sesion;

        public UsuarioService(IUsuarioRepositorio usuarios, IHashService hash, IVerificarHashService verificar,
            IActividadRepositorio actividad, SesionUsuario sesion)
        {
            _usuarios = usuarios;
            _hash = hash;
            _verificar = verificar;
            _actividad = actividad;
            _sesion = sesion;
        }

        // ------------------------------------------------------------------ Registro público

        /// <summary>
        /// Registro público de una cuenta. SIEMPRE crea un usuario con el rol USUARIO:
        /// nadie puede convertirse en administrador desde aquí.
        /// </summary>
        public void RegistrarCuenta(string nombreCompleto, string correo, string nombreUsuario, string password, string confirmacion)
        {
            var datos = Normalizar(nombreCompleto, correo, nombreUsuario);
            Validaciones.Password(password);
            if (password != confirmacion) throw new ValidacionException("Las contraseñas no coinciden.");
            if (_usuarios.Existe(datos.Usuario, datos.Correo))
                throw new ValidacionException("Ya existe una cuenta con ese usuario o correo.");

            int id = _usuarios.Insertar(new Usuario
            {
                NombreUsuario = datos.Usuario, Correo = datos.Correo, NombreCompleto = datos.Nombre,
                PasswordHash = _hash.GenerarHash(password), RolId = Roles.Usuario, Activo = true
            });
            _actividad.Registrar(id, "REGISTRO", "Nueva cuenta registrada: " + datos.Usuario);
        }

        // ------------------------------------------------------------------ Administración (solo administrador)

        public IList<Usuario> Listar()
        {
            _sesion.Exigir(Permisos.UsuariosGestionar);
            return _usuarios.ObtenerTodos();
        }

        public IList<Permiso> ObtenerPermisosDelegables()
        {
            _sesion.Exigir(Permisos.UsuariosGestionar);
            return _usuarios.ObtenerPermisosDelegables();
        }

        public IList<string> ObtenerPermisosConcedidos(int usuarioId)
        {
            _sesion.Exigir(Permisos.UsuariosGestionar);
            return _usuarios.ObtenerPermisosConcedidos(usuarioId);
        }

        /// <summary>Crea una cuenta con el rol indicado (incluido administrador). Solo un administrador puede hacerlo.</summary>
        public void Crear(string nombreCompleto, string correo, string nombreUsuario, string password,
            int rolId, IEnumerable<string> permisosExtra)
        {
            _sesion.Exigir(Permisos.UsuariosGestionar);
            var datos = Normalizar(nombreCompleto, correo, nombreUsuario);
            Validaciones.Password(password);
            ValidarRol(rolId);
            if (_usuarios.Existe(datos.Usuario, datos.Correo))
                throw new ValidacionException("Ya existe una cuenta con ese usuario o correo.");

            int id = _usuarios.Insertar(new Usuario
            {
                NombreUsuario = datos.Usuario, Correo = datos.Correo, NombreCompleto = datos.Nombre,
                PasswordHash = _hash.GenerarHash(password), RolId = rolId, Activo = true
            });
            if (rolId == Roles.Usuario) _usuarios.ReemplazarPermisosConcedidos(id, permisosExtra);
            _actividad.Registrar(_sesion.UsuarioId, "USUARIO_CREADO", $"{datos.Usuario} ({Roles.Nombre(rolId)})");
        }

        public void Actualizar(int id, string nombreCompleto, string correo, int rolId, bool activo, IEnumerable<string> permisosExtra)
        {
            _sesion.Exigir(Permisos.UsuariosGestionar);
            var actual = _usuarios.ObtenerPorId(id) ?? throw new ValidacionException("El usuario ya no existe.");
            nombreCompleto = Validaciones.Texto(nombreCompleto, "El nombre completo", 100);
            correo = Validaciones.Correo(correo);
            ValidarRol(rolId);
            if (_usuarios.Existe(actual.NombreUsuario, correo, id))
                throw new ValidacionException("Ya existe otra cuenta con ese correo.");

            bool pierdeAdmin = actual.EsAdministrador && actual.Activo && (rolId != Roles.Admin || !activo);
            if (pierdeAdmin)
            {
                if (id == _sesion.UsuarioId)
                    throw new ValidacionException("No puede desactivar su propia cuenta ni quitarse el rol de administrador.");
                if (_usuarios.ContarAdministradoresActivos() <= 1)
                    throw new ValidacionException("Debe existir al menos un administrador activo.");
            }

            actual.NombreCompleto = nombreCompleto;
            actual.Correo = correo;
            actual.RolId = rolId;
            actual.Activo = activo;
            _usuarios.Actualizar(actual);
            // Los administradores ya tienen todos los permisos: solo los usuarios reciben permisos individuales
            _usuarios.ReemplazarPermisosConcedidos(id, rolId == Roles.Usuario ? permisosExtra : Array.Empty<string>());
            _actividad.Registrar(_sesion.UsuarioId, "USUARIO_EDITADO",
                $"{actual.NombreUsuario}: {Roles.Nombre(rolId)}, {(activo ? "activo" : "desactivado")}");
        }

        public void RestablecerPassword(int id, string nuevaPassword)
        {
            _sesion.Exigir(Permisos.UsuariosGestionar);
            Validaciones.Password(nuevaPassword);
            var usuario = _usuarios.ObtenerPorId(id) ?? throw new ValidacionException("El usuario ya no existe.");
            _usuarios.ActualizarPassword(id, _hash.GenerarHash(nuevaPassword));
            _actividad.Registrar(_sesion.UsuarioId, "PASSWORD_RESTABLECIDA", usuario.NombreUsuario);
        }

        // ------------------------------------------------------------------ Cuenta propia

        public Usuario ObtenerMiPerfil()
        {
            _sesion.Exigir(Permisos.ConfigPersonal);
            var u = _usuarios.ObtenerPorId(_sesion.UsuarioId) ?? throw new ValidacionException("La cuenta ya no existe.");
            u.PasswordHash = string.Empty;
            return u;
        }

        public void ActualizarMiPerfil(string nombreCompleto, string correo)
        {
            _sesion.Exigir(Permisos.ConfigPersonal);
            nombreCompleto = Validaciones.Texto(nombreCompleto, "El nombre completo", 100);
            correo = Validaciones.Correo(correo);
            var actual = _usuarios.ObtenerPorId(_sesion.UsuarioId) ?? throw new ValidacionException("La cuenta ya no existe.");
            if (_usuarios.Existe(actual.NombreUsuario, correo, actual.Id))
                throw new ValidacionException("Ya existe otra cuenta con ese correo.");

            _usuarios.ActualizarPerfil(actual.Id, nombreCompleto, correo);
            _sesion.ActualizarDatos(nombreCompleto, correo);
            _actividad.Registrar(actual.Id, "PERFIL_ACTUALIZADO", "Datos personales actualizados");
        }

        public void CambiarMiPassword(string actual, string nueva, string confirmacion)
        {
            _sesion.Exigir(Permisos.ConfigPersonal);
            var usuario = _usuarios.ObtenerPorId(_sesion.UsuarioId) ?? throw new ValidacionException("La cuenta ya no existe.");
            if (!_verificar.Verificar(actual ?? "", usuario.PasswordHash))
                throw new ValidacionException("La contraseña actual es incorrecta.");
            Validaciones.Password(nueva);
            if (nueva != confirmacion) throw new ValidacionException("Las contraseñas nuevas no coinciden.");
            if (_verificar.Verificar(nueva, usuario.PasswordHash))
                throw new ValidacionException("La nueva contraseña debe ser distinta de la actual.");

            _usuarios.ActualizarPassword(usuario.Id, _hash.GenerarHash(nueva));
            _actividad.Registrar(usuario.Id, "PASSWORD_CAMBIADA", "Contraseña actualizada");
        }

        // ------------------------------------------------------------------ Internos

        private static (string Nombre, string Correo, string Usuario) Normalizar(string nombreCompleto, string correo, string nombreUsuario) =>
            (Validaciones.Texto(nombreCompleto, "El nombre completo", 100),
             Validaciones.Correo(correo),
             Validaciones.NombreUsuario(nombreUsuario));

        private static void ValidarRol(int rolId)
        {
            if (rolId != Roles.Admin && rolId != Roles.Usuario) throw new ValidacionException("El rol no es válido.");
        }
    }
}
