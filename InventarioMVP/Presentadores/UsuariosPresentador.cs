using InventarioMVP.Negocio.Servicios;
using InventarioMVP.Vistas;

namespace InventarioMVP.Presentadores
{
    public class UsuariosPresentador
    {
        private readonly IUsuariosView _vista;
        private readonly UsuarioService _usuarios;

        public UsuariosPresentador(IUsuariosView vista, UsuarioService usuarios)
        {
            _vista = vista;
            _usuarios = usuarios;
            _vista.CargarSolicitado += (_, _) => Ejecutar(Recargar);
            _vista.NuevoSolicitado += (_, _) => Ejecutar(Nuevo);
            _vista.EditarSolicitado += (_, _) => Ejecutar(Editar);
            _vista.EstadoSolicitado += (_, _) => Ejecutar(AlternarEstado);
            _vista.PasswordSolicitada += (_, _) => Ejecutar(RestablecerPassword);
        }

        private void Recargar() => _vista.MostrarUsuarios(_usuarios.Listar());

        private void Nuevo()
        {
            var delegables = _usuarios.ObtenerPermisosDelegables();
            bool guardado = _vista.EditarUsuario(null, delegables, new List<string>(), d =>
                Ejecutor.Intentar(() => _usuarios.Crear(d.NombreCompleto, d.Correo, d.NombreUsuario, d.Password, d.RolId, d.Permisos)));
            if (!guardado) return;
            Recargar();
            _vista.MostrarInfo("Usuario creado correctamente.");
        }

        private void Editar()
        {
            var u = _vista.UsuarioSeleccionado ?? throw new ValidacionException("Seleccione un usuario de la tabla para editarlo.");
            var delegables = _usuarios.ObtenerPermisosDelegables();
            var concedidos = _usuarios.ObtenerPermisosConcedidos(u.Id);
            bool guardado = _vista.EditarUsuario(u, delegables, concedidos, d =>
                Ejecutor.Intentar(() => _usuarios.Actualizar(u.Id, d.NombreCompleto, d.Correo, d.RolId, d.Activo, d.Permisos)));
            if (!guardado) return;
            Recargar();
            _vista.MostrarInfo("Usuario actualizado correctamente.");
        }

        private void AlternarEstado()
        {
            var u = _vista.UsuarioSeleccionado ?? throw new ValidacionException("Seleccione un usuario de la tabla.");
            string accion = u.Activo ? "desactivar" : "activar";
            if (!_vista.Confirmar($"¿Desea {accion} la cuenta de \"{u.NombreUsuario}\"?")) return;
            _usuarios.Actualizar(u.Id, u.NombreCompleto, u.Correo, u.RolId, !u.Activo, _usuarios.ObtenerPermisosConcedidos(u.Id));
            Recargar();
            _vista.MostrarInfo(u.Activo ? "Cuenta desactivada." : "Cuenta activada.");
        }

        private void RestablecerPassword()
        {
            var u = _vista.UsuarioSeleccionado ?? throw new ValidacionException("Seleccione un usuario de la tabla.");
            if (_vista.PedirNuevaPassword(u, p => Ejecutor.Intentar(() => _usuarios.RestablecerPassword(u.Id, p))))
                _vista.MostrarInfo($"Contraseña de \"{u.NombreUsuario}\" restablecida.");
        }

        private void Ejecutar(Action accion) => Ejecutor.Ejecutar(accion, _vista.MostrarError);
    }
}
