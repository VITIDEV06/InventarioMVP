using InventarioMVP.Negocio.Servicios;
using InventarioMVP.Vistas;

namespace InventarioMVP.Presentadores
{
    public class LoginPresentador
    {
        private readonly ILoginView _vista;
        private readonly AutenticacionService _autenticacion;

        public LoginPresentador(ILoginView vista, AutenticacionService autenticacion)
        {
            _vista = vista;
            _autenticacion = autenticacion;
            _vista.IniciarSesionSolicitado += (_, _) => IniciarSesion();
        }

        private void IniciarSesion()
        {
            _vista.Ocupado(true);
            try
            {
                var usuario = _autenticacion.Autenticar(_vista.Identificador, _vista.Password);
                _vista.CompletarInicioSesion(usuario);
            }
            catch (ValidacionException ex)
            {
                _vista.LimpiarPassword();
                _vista.MostrarError(ex.Message);
            }
            catch (Exception ex)
            {
                _vista.MostrarError("No se pudo conectar con la base de datos: " + ex.Message);
            }
            finally
            {
                _vista.Ocupado(false);
            }
        }
    }
}
