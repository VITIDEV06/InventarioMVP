namespace InventarioMVP.Negocio.Seguridad
{
    /// <summary>Códigos de permiso (coinciden con la tabla permisos de la base de datos).</summary>
    public static class Permisos
    {
        public const string DashboardGeneral = "DASHBOARD_GENERAL";
        public const string ProductosVer = "PRODUCTOS_VER";
        public const string ProductosGestionar = "PRODUCTOS_GESTIONAR";
        public const string InventarioVer = "INVENTARIO_VER";
        public const string InventarioMover = "INVENTARIO_MOVER";
        public const string ClientesVer = "CLIENTES_VER";
        public const string ClientesGestionar = "CLIENTES_GESTIONAR";
        public const string VentasPropiasVer = "VENTAS_PROPIAS_VER";
        public const string VentasRegistrar = "VENTAS_REGISTRAR";
        public const string VentasVerTodas = "VENTAS_VER_TODAS";
        public const string ReportesVer = "REPORTES_VER";
        public const string UsuariosGestionar = "USUARIOS_GESTIONAR";
        public const string ConfigPersonal = "CONFIG_PERSONAL";
        public const string ConfigAdmin = "CONFIG_ADMIN";
    }

    public static class Roles
    {
        public const int Admin = 1;
        public const int Usuario = 2;

        public static string Nombre(int rolId) => rolId == Admin ? "Administrador" : "Usuario";
    }
}
