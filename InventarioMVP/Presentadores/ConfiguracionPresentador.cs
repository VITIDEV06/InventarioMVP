using InventarioMVP.Negocio.Servicios;
using InventarioMVP.Vistas;

namespace InventarioMVP.Presentadores
{
    public class ConfiguracionPresentador
    {
        private readonly IConfiguracionView _vista;
        private readonly UsuarioService _usuarios;
        private readonly ConfiguracionService _config;

        public ConfiguracionPresentador(IConfiguracionView vista, UsuarioService usuarios, ConfiguracionService config)
        {
            _vista = vista;
            _usuarios = usuarios;
            _config = config;

            _vista.CargarSolicitado += (_, _) => Ejecutar(Cargar);
            _vista.PerfilGuardado += (_, _) => Ejecutar(() =>
            {
                _usuarios.ActualizarMiPerfil(_vista.NombreCompleto, _vista.Correo);
                Cargar();
                _vista.MostrarInfo("Sus datos personales se actualizaron correctamente.");
            });
            _vista.PasswordCambiada += (_, _) => Ejecutar(() =>
            {
                _usuarios.CambiarMiPassword(_vista.PasswordActual, _vista.PasswordNueva, _vista.PasswordConfirmacion);
                _vista.LimpiarPasswords();
                _vista.MostrarInfo("Contraseña actualizada correctamente.");
            });
            _vista.ParametrosGuardados += (_, _) => Ejecutar(() =>
            {
                _config.GuardarStockMinimoDefecto(_vista.StockMinimo);
                _vista.MostrarInfo("Parámetros guardados.");
            });
        }

        private void Cargar()
        {
            _vista.MostrarPerfil(_usuarios.ObtenerMiPerfil());
            _vista.MostrarParametros(_config.StockMinimoDefecto(), _config.EsAdministrativa);
        }

        private void Ejecutar(Action accion) => Ejecutor.Ejecutar(accion, _vista.MostrarError);
    }
}
