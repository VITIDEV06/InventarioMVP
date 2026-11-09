using InventarioMVP.Entidades;

namespace InventarioMVP.Datos.Repositorios
{
    public interface IReporteRepositorio
    {
        // --- Dashboard ---
        int ContarProductos();
        int ContarProductosStockBajo();
        int ContarClientes();
        int ContarClientesNuevos(DateTime desde);
        decimal Ingresos(DateTime desde, DateTime hastaExclusivo, int? usuarioId);
        IList<PuntoVenta> VentasPorDia(DateTime desde, DateTime hastaExclusivo, int? usuarioId);
        IList<Venta> UltimasVentas(int limite, int? usuarioId);
        IList<ProductoVendido> MasVendidos(int limite, DateTime desde, DateTime hastaExclusivo, int? usuarioId);

        // --- Reportes (tablas genéricas) ---
        TablaReporte Ventas(DateTime desde, DateTime hastaExclusivo);
        TablaReporte Ingresos(string agrupacion, DateTime desde, DateTime hastaExclusivo);
        TablaReporte ProductosVendidos(bool masVendidos, DateTime desde, DateTime hastaExclusivo);
        TablaReporte InventarioDisponible();
        TablaReporte StockBajo();
        TablaReporte Movimientos(DateTime desde, DateTime hastaExclusivo);
        TablaReporte VentasPorUsuario(DateTime desde, DateTime hastaExclusivo);
    }

    public class ReporteRepositorio : IReporteRepositorio
    {
        private readonly ConexionMySql _db;

        public ReporteRepositorio(ConexionMySql db) => _db = db;

        // ---------------------------------------------------------------- Dashboard

        public int ContarProductos() => Convert.ToInt32(_db.Escalar("SELECT COUNT(*) FROM productos WHERE activo = 1"));

        public int ContarProductosStockBajo() =>
            Convert.ToInt32(_db.Escalar("SELECT COUNT(*) FROM productos WHERE activo = 1 AND stock <= stock_minimo"));

        public int ContarClientes() => Convert.ToInt32(_db.Escalar("SELECT COUNT(*) FROM clientes"));

        public int ContarClientesNuevos(DateTime desde) => Convert.ToInt32(
            _db.Escalar("SELECT COUNT(*) FROM clientes WHERE fecha_registro >= @d", ("@d", desde)));

        public decimal Ingresos(DateTime desde, DateTime hastaExclusivo, int? usuarioId) => Convert.ToDecimal(
            _db.Escalar(
                "SELECT COALESCE(SUM(total), 0) FROM ventas WHERE fecha >= @d AND fecha < @h AND (@u IS NULL OR usuario_id = @u)",
                ("@d", desde), ("@h", hastaExclusivo), ("@u", usuarioId)));

        public IList<PuntoVenta> VentasPorDia(DateTime desde, DateTime hastaExclusivo, int? usuarioId) =>
            _db.Consultar(
                "SELECT DATE(fecha) AS dia, SUM(total) AS total FROM ventas " +
                "WHERE fecha >= @d AND fecha < @h AND (@u IS NULL OR usuario_id = @u) GROUP BY DATE(fecha) ORDER BY dia",
                rd => new PuntoVenta(rd.GetDateTime("dia"), rd.Decimal("total")),
                ("@d", desde), ("@h", hastaExclusivo), ("@u", usuarioId));

        public IList<Venta> UltimasVentas(int limite, int? usuarioId) =>
            _db.Consultar(
                "SELECT v.id, v.fecha, COALESCE(c.nombre_completo, v.cliente_ocasional) AS cliente, " +
                "u.nombre_completo AS usuario, v.total " +
                "FROM ventas v LEFT JOIN clientes c ON c.id = v.cliente_id JOIN usuarios u ON u.id = v.usuario_id " +
                "WHERE (@u IS NULL OR v.usuario_id = @u) ORDER BY v.fecha DESC, v.id DESC LIMIT @l",
                rd => new Venta
                {
                    Id = rd.GetInt32("id"), Fecha = rd.GetDateTime("fecha"), ClienteNombre = rd.Texto("cliente"),
                    UsuarioNombre = rd.GetString("usuario"), Total = rd.GetDecimal("total")
                }, ("@u", usuarioId), ("@l", limite));

        public IList<ProductoVendido> MasVendidos(int limite, DateTime desde, DateTime hastaExclusivo, int? usuarioId) =>
            _db.Consultar(
                "SELECT p.codigo, p.nombre, SUM(d.cantidad) AS unidades, SUM(d.subtotal) AS ingresos " +
                "FROM detalle_ventas d JOIN ventas v ON v.id = d.venta_id JOIN productos p ON p.id = d.producto_id " +
                "WHERE v.fecha >= @d AND v.fecha < @h AND (@u IS NULL OR v.usuario_id = @u) " +
                "GROUP BY p.id ORDER BY unidades DESC, ingresos DESC LIMIT @l",
                rd => new ProductoVendido
                {
                    Codigo = rd.GetString("codigo"), Nombre = rd.GetString("nombre"),
                    Unidades = rd.Entero("unidades"), Ingresos = rd.Decimal("ingresos")
                }, ("@d", desde), ("@h", hastaExclusivo), ("@u", usuarioId), ("@l", limite));

        // ---------------------------------------------------------------- Reportes

        public TablaReporte Ventas(DateTime desde, DateTime hastaExclusivo) => Tabla("Reporte de ventas",
            "SELECT CONCAT('V-', LPAD(v.id, 6, '0')) AS `Número`, v.fecha AS `Fecha`, " +
            "COALESCE(c.nombre_completo, v.cliente_ocasional) AS `Cliente`, u.nombre_completo AS `Vendedor`, " +
            "v.total AS `Total (USD)` " +
            "FROM ventas v LEFT JOIN clientes c ON c.id = v.cliente_id JOIN usuarios u ON u.id = v.usuario_id " +
            "WHERE v.fecha >= @d AND v.fecha < @h ORDER BY v.fecha DESC, v.id DESC",
            ("@d", desde), ("@h", hastaExclusivo));

        public TablaReporte Ingresos(string agrupacion, DateTime desde, DateTime hastaExclusivo)
        {
            // La expresión proviene de una lista cerrada (nunca de texto del usuario)
            (string titulo, string periodo) = agrupacion switch
            {
                "DIA" => ("Ingresos por día", "DATE_FORMAT(fecha, '%Y-%m-%d')"),
                "SEMANA" => ("Ingresos por semana",
                    "CONCAT(YEARWEEK(fecha, 1) DIV 100, '-S', LPAD(YEARWEEK(fecha, 1) MOD 100, 2, '0'))"),
                "MES" => ("Ingresos por mes", "DATE_FORMAT(fecha, '%Y-%m')"),
                "ANIO" => ("Ingresos por año", "CAST(YEAR(fecha) AS CHAR)"),
                _ => throw new ArgumentException("Agrupación no válida.", nameof(agrupacion))
            };
            return Tabla(titulo,
                $"SELECT {periodo} AS `Período`, COUNT(*) AS `Ventas`, SUM(total) AS `Ingresos (USD)` " +
                "FROM ventas WHERE fecha >= @d AND fecha < @h GROUP BY 1 ORDER BY 1",
                ("@d", desde), ("@h", hastaExclusivo));
        }

        public TablaReporte ProductosVendidos(bool masVendidos, DateTime desde, DateTime hastaExclusivo) =>
            Tabla(masVendidos ? "Productos más vendidos" : "Productos menos vendidos",
                "SELECT p.codigo AS `Código`, p.nombre AS `Producto`, p.categoria AS `Categoría`, " +
                "COALESCE(SUM(CASE WHEN v.id IS NOT NULL THEN d.cantidad END), 0) AS `Unidades vendidas`, " +
                "COALESCE(SUM(CASE WHEN v.id IS NOT NULL THEN d.subtotal END), 0) AS `Ingresos (USD)` " +
                "FROM productos p LEFT JOIN detalle_ventas d ON d.producto_id = p.id " +
                "LEFT JOIN ventas v ON v.id = d.venta_id AND v.fecha >= @d AND v.fecha < @h " +
                "WHERE p.activo = 1 GROUP BY p.id " +
                (masVendidos ? "ORDER BY 4 DESC, 5 DESC, p.nombre" : "ORDER BY 4 ASC, 5 ASC, p.nombre") + " LIMIT 50",
                ("@d", desde), ("@h", hastaExclusivo));

        public TablaReporte InventarioDisponible() => Tabla("Inventario disponible",
            "SELECT codigo AS `Código`, nombre AS `Producto`, categoria AS `Categoría`, precio AS `Precio (USD)`, " +
            "stock AS `Stock`, stock_minimo AS `Stock mínimo`, (precio * stock) AS `Valor (USD)` " +
            "FROM productos WHERE activo = 1 ORDER BY nombre");

        public TablaReporte StockBajo() => Tabla("Productos con stock bajo",
            "SELECT codigo AS `Código`, nombre AS `Producto`, categoria AS `Categoría`, " +
            "stock AS `Stock`, stock_minimo AS `Stock mínimo` " +
            "FROM productos WHERE activo = 1 AND stock <= stock_minimo ORDER BY stock, nombre");

        public TablaReporte Movimientos(DateTime desde, DateTime hastaExclusivo) => Tabla("Historial de movimientos",
            "SELECT m.fecha AS `Fecha`, p.codigo AS `Código`, p.nombre AS `Producto`, m.tipo AS `Tipo`, " +
            "m.cantidad AS `Cantidad`, m.stock_anterior AS `Stock anterior`, m.stock_nuevo AS `Stock nuevo`, " +
            "m.motivo AS `Motivo`, u.nombre_completo AS `Usuario` " +
            "FROM movimientos_inventario m JOIN productos p ON p.id = m.producto_id JOIN usuarios u ON u.id = m.usuario_id " +
            "WHERE m.fecha >= @d AND m.fecha < @h ORDER BY m.fecha DESC, m.id DESC LIMIT 5000",
            ("@d", desde), ("@h", hastaExclusivo));

        public TablaReporte VentasPorUsuario(DateTime desde, DateTime hastaExclusivo) => Tabla("Rendimiento de ventas por usuario",
            "SELECT u.nombre_completo AS `Usuario`, u.rol AS `Rol`, COUNT(v.id) AS `Ventas`, " +
            "COALESCE(SUM(v.total), 0) AS `Ingresos (USD)`, COALESCE(AVG(v.total), 0) AS `Venta promedio (USD)` " +
            "FROM (SELECT us.id, us.nombre_completo, r.nombre AS rol FROM usuarios us JOIN roles r ON r.id = us.rol_id) u " +
            "LEFT JOIN ventas v ON v.usuario_id = u.id AND v.fecha >= @d AND v.fecha < @h " +
            "GROUP BY u.id ORDER BY 4 DESC, u.nombre_completo",
            ("@d", desde), ("@h", hastaExclusivo));

        private TablaReporte Tabla(string titulo, string sql, params (string, object?)[] p)
        {
            var (columnas, filas) = _db.Usar(op => op.Tabla(sql, p));
            return new TablaReporte { Titulo = titulo, Columnas = columnas, Filas = filas };
        }
    }
}
