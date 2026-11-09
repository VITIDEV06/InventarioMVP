using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Seguridad;
using InventarioMVP.Negocio.Servicios;
using InventarioMVP.Presentadores;
using InventarioMVP.Vistas;

namespace InventarioMVP
{
    /// <summary>Servicios de Negocio compartidos por toda la aplicación (una única sesión de usuario).</summary>
    internal sealed record Servicios(
        SesionUsuario Sesion, AutenticacionService Autenticacion, UsuarioService Usuarios, ProductoService Productos,
        InventarioService Inventario, ClienteService Clientes, VentaService Ventas, DashboardService Dashboard,
        ReporteService Reportes, ConfiguracionService Configuracion);

    /// <summary>
    /// Crea cada pantalla junto con su presentador y arma el menú lateral según los permisos del usuario.
    /// Ocultar una pestaña es solo una comodidad: los servicios de Negocio vuelven a validar cada operación.
    /// </summary>
    internal sealed class FabricaVistas
    {
        private readonly Servicios _s;

        public FabricaVistas(Servicios servicios) => _s = servicios;

        public IList<ItemMenu> Menu(Usuario usuario)
        {
            var sesion = _s.Sesion;
            var items = new List<ItemMenu> { new("Inicio", "layout-dashboard", () => CrearInicio(usuario)) };
            if (sesion.Tiene(Permisos.InventarioVer)) items.Add(new("Inventario", "warehouse", CrearInventario));
            if (sesion.Tiene(Permisos.ProductosVer)) items.Add(new("Productos", "package", CrearProductos));
            if (sesion.Tiene(Permisos.VentasPropiasVer) || sesion.Tiene(Permisos.VentasRegistrar) || sesion.Tiene(Permisos.VentasVerTodas))
                items.Add(new("Ventas", "shopping-cart", CrearVentas));
            if (sesion.Tiene(Permisos.ClientesVer)) items.Add(new("Clientes", "contact", CrearClientes));
            if (sesion.Tiene(Permisos.UsuariosGestionar)) items.Add(new("Usuarios", "users", CrearUsuarios));
            if (sesion.Tiene(Permisos.ReportesVer)) items.Add(new("Reportes", "chart-no-axes-combined", CrearReportes));
            if (sesion.Tiene(Permisos.ConfigPersonal)) items.Add(new("Configuración", "settings", CrearConfiguracion));
            return items;
        }

        private Control CrearInicio(Usuario usuario)
        {
            var vista = new UcInicio(usuario);
            new InicioPresentador(vista, _s.Dashboard);
            return vista;
        }

        private Control CrearInventario()
        {
            var vista = new UcInventario();
            new InventarioPresentador(vista, _s.Inventario);
            return vista;
        }

        private Control CrearProductos()
        {
            var vista = new UcProductos();
            new ProductosPresentador(vista, _s.Productos, _s.Configuracion);
            return vista;
        }

        private Control CrearVentas()
        {
            var vista = new UcVentas();
            new VentasPresentador(vista, _s.Ventas);
            return vista;
        }

        private Control CrearClientes()
        {
            var vista = new UcClientes();
            new ClientesPresentador(vista, _s.Clientes);
            return vista;
        }

        private Control CrearUsuarios()
        {
            var vista = new UcUsuarios();
            new UsuariosPresentador(vista, _s.Usuarios);
            return vista;
        }

        private Control CrearReportes()
        {
            var vista = new UcReportes();
            new ReportesPresentador(vista, _s.Reportes);
            return vista;
        }

        private Control CrearConfiguracion()
        {
            var vista = new UcConfiguracion();
            new ConfiguracionPresentador(vista, _s.Usuarios, _s.Configuracion);
            return vista;
        }
    }
}
