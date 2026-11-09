using InventarioMVP.Negocio.Servicios;
using InventarioMVP.Vistas;

namespace InventarioMVP.Presentadores
{
    public class RegistroPresentador
    {
        private readonly IRegistroView _vista;
        private readonly UsuarioService _usuarios;

        public RegistroPresentador(IRegistroView vista, UsuarioService usuarios)
        {
            _vista = vista;
            _usuarios = usuarios;
            _vista.RegistrarSolicitado += (_, _) => Registrar();
        }

        // Si no hay error, la vista se cierra con éxito; si lo hay, lo muestra dentro del diálogo.
        private void Registrar() => Ejecutor.Ejecutar(
            () => _usuarios.RegistrarCuenta(_vista.NombreCompleto, _vista.Correo, _vista.NombreUsuario,
                _vista.Password, _vista.Confirmacion),
            _vista.MostrarError);
    }
}
