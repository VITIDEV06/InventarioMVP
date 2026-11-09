using InventarioMVP.Datos.Repositorios;
using InventarioMVP.Negocio.Seguridad;

namespace InventarioMVP.Negocio.Servicios
{
    public class ConfiguracionService
    {
        private const string ClaveStockMinimo = "stock_minimo_defecto";

        private readonly IConfiguracionRepositorio _config;
        private readonly IActividadRepositorio _actividad;
        private readonly SesionUsuario _sesion;

        public ConfiguracionService(IConfiguracionRepositorio config, IActividadRepositorio actividad, SesionUsuario sesion)
        {
            _config = config;
            _actividad = actividad;
            _sesion = sesion;
        }

        public bool EsAdministrativa => _sesion.Tiene(Permisos.ConfigAdmin);

        /// <summary>Stock mínimo sugerido al crear productos.</summary>
        public int StockMinimoDefecto() =>
            int.TryParse(_config.Obtener(ClaveStockMinimo), out var v) && v >= 0 ? v : 5;

        public void GuardarStockMinimoDefecto(int valor)
        {
            _sesion.Exigir(Permisos.ConfigAdmin);
            if (valor < 0 || valor > 100000) throw new ValidacionException("El stock mínimo debe estar entre 0 y 100,000.");
            _config.Guardar(ClaveStockMinimo, valor.ToString());
            _actividad.Registrar(_sesion.UsuarioId, "CONFIGURACION", "Stock mínimo por defecto: " + valor);
        }
    }
}
