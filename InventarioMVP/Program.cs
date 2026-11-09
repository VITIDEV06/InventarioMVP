using System.Text.Json;
using InventarioMVP.Datos;
using InventarioMVP.Datos.Repositorios;
using InventarioMVP.Negocio.Reportes;
using InventarioMVP.Negocio.Seguridad;
using InventarioMVP.Negocio.Servicios;
using InventarioMVP.Presentadores;
using InventarioMVP.Vistas;

namespace InventarioMVP
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            string cadena;
            try { cadena = LeerCadenaConexion(); }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo leer appsettings.json (copie appsettings.example.json como appsettings.json " +
                                "y escriba su contraseña): " + ex.Message, "Inventario",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var servicios = Componer(cadena);
            var fabrica = new FabricaVistas(servicios);

            while (true)
            {
                using var login = new FrmLogin();
                new LoginPresentador(login, servicios.Autenticacion);
                login.CrearCuentaSolicitada += (_, _) =>
                {
                    using var registro = new FrmRegistro();
                    new RegistroPresentador(registro, servicios.Usuarios);
                    if (registro.ShowDialog(login) == DialogResult.OK) login.CuentaCreada(registro.NombreUsuario);
                };
                if (login.ShowDialog() != DialogResult.OK || login.UsuarioAutenticado == null) return;

                using var dashboard = new FrmDashboard(login.UsuarioAutenticado, fabrica.Menu(login.UsuarioAutenticado));
                Application.Run(dashboard);
                servicios.Autenticacion.CerrarSesion();
                if (!dashboard.CerrarSesionSolicitado) return;
            }
        }

        /// <summary>Raíz de composición: Datos -> Negocio (con una sesión compartida) -> Presentadores -> Vistas.</summary>
        private static Servicios Componer(string cadenaConexion)
        {
            var db = new ConexionMySql(cadenaConexion);
            var usuarios = new UsuarioRepositorio(db);
            var productos = new ProductoRepositorio(db);
            var inventario = new InventarioRepositorio(db);
            var clientes = new ClienteRepositorio(db);
            var ventas = new VentaRepositorio(db);
            var reportes = new ReporteRepositorio(db);
            var actividad = new ActividadRepositorio(db);
            var configuracion = new ConfiguracionRepositorio(db);

            IHashService hash = new BCryptHashService();
            IVerificarHashService verificar = new BCryptVerificarHashService();
            var sesion = new SesionUsuario();

            return new Servicios(
                sesion,
                new AutenticacionService(usuarios, verificar, actividad, sesion),
                new UsuarioService(usuarios, hash, verificar, actividad, sesion),
                new ProductoService(productos, actividad, sesion),
                new InventarioService(productos, inventario, actividad, sesion),
                new ClienteService(clientes, ventas, actividad, sesion),
                new VentaService(ventas, productos, clientes, actividad, sesion),
                new DashboardService(reportes, actividad, productos, sesion),
                new ReporteService(reportes, actividad, sesion, new ExportadorExcel(), new ExportadorPdf()),
                new ConfiguracionService(configuracion, actividad, sesion));
        }

        private static string LeerCadenaConexion()
        {
            var ruta = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            using var doc = JsonDocument.Parse(File.ReadAllText(ruta));
            return doc.RootElement.GetProperty("ConnectionStrings").GetProperty("MySql").GetString()
                   ?? throw new InvalidOperationException("Falta ConnectionStrings:MySql.");
        }
    }
}
