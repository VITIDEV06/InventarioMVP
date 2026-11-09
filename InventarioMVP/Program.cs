using System.Text.Json;
using InventarioMVP.Datos;
using InventarioMVP.Datos.Repositorios;
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

            // Raíz de composición: aquí se ensamblan las capas (Datos -> Negocio -> Presentadores -> Vistas).
            var conexion = new ConexionMySql(cadena);
            IUsuarioRepositorio usuarioRepo = new UsuarioRepositorio(conexion);
            IProductoRepositorio productoRepo = new ProductoRepositorio(conexion);
            IHashService hash = new BCryptHashService();
            IVerificarHashService verificar = new BCryptVerificarHashService();

            var autenticacion = new AutenticacionService(usuarioRepo, verificar);
            var usuarioService = new UsuarioService(usuarioRepo, hash);
            var productoService = new ProductoService(productoRepo);

            while (true)
            {
                using var login = new FrmLogin();
                new LoginPresentador(login, autenticacion);
                if (login.ShowDialog() != DialogResult.OK || login.UsuarioAutenticado == null) return;

                using var dashboard = new FrmDashboard(
                    login.UsuarioAutenticado,
                    () =>
                    {
                        var vista = new UcInventario();
                        new InventarioPresentador(vista, productoService);
                        return vista;
                    },
                    () =>
                    {
                        var vista = new UcUsuarios();
                        new UsuariosPresentador(vista, usuarioService);
                        return vista;
                    });
                Application.Run(dashboard);
                if (!dashboard.CerrarSesionSolicitado) return;
            }
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
