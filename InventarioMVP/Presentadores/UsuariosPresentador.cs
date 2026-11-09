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
            _vista.CargarSolicitado += (_, _) => Ejecutar(Cargar);
            _vista.RegistrarSolicitado += (_, _) => Ejecutar(Registrar);
        }

        private void Cargar() => _vista.MostrarUsuarios(_usuarios.Listar());

        private void Registrar()
        {
            if (_vista.Password != _vista.ConfirmarPassword)
                throw new ValidacionException("Las contraseñas no coinciden.");
            _usuarios.Registrar(_vista.NombreUsuario, _vista.Correo, _vista.NombreCompleto, _vista.Password);
            _vista.LimpiarFormulario();
            Cargar();
            _vista.MostrarInfo("Usuario registrado correctamente.");
        }

        private void Ejecutar(Action accion)
        {
            try { accion(); }
            catch (ValidacionException ex) { _vista.MostrarError(ex.Message); }
            catch (Exception ex) { _vista.MostrarError("Error de base de datos: " + ex.Message); }
        }
    }
}
