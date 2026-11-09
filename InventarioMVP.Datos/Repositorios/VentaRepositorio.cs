using InventarioMVP.Entidades;
using MySql.Data.MySqlClient;

namespace InventarioMVP.Datos.Repositorios
{
    public interface IVentaRepositorio
    {
        /// <summary>
        /// Registra la venta, sus detalles, descuenta el stock y deja los movimientos, todo en una transacción.
        /// Los precios se leen de la base de datos (no se confía en el cliente). Devuelve el id de la venta.
        /// </summary>
        int Registrar(int usuarioId, int? clienteId, string? clienteOcasional, IList<LineaVenta> lineas);

        IList<Venta> Buscar(string texto, int? numero, DateTime desde, DateTime hastaExclusivo, int? usuarioId = null, int? clienteId = null);
        Venta? ObtenerConDetalle(int id);
    }

    public class VentaRepositorio : IVentaRepositorio
    {
        private readonly ConexionMySql _db;

        public VentaRepositorio(ConexionMySql db) => _db = db;

        public int Registrar(int usuarioId, int? clienteId, string? clienteOcasional, IList<LineaVenta> lineas) =>
            _db.EnTransaccion(op =>
            {
                // 1) Bloquear y validar cada producto (en orden de id para evitar interbloqueos)
                var items = new List<(LineaVenta Linea, int Stock, decimal Precio, string Nombre)>();
                decimal total = 0;
                foreach (var linea in lineas.OrderBy(l => l.ProductoId))
                {
                    var fila = op.Consultar(
                        "SELECT stock, precio, nombre FROM productos WHERE id = @id AND activo = 1 FOR UPDATE",
                        rd => (Stock: rd.GetInt32(0), Precio: rd.GetDecimal(1), Nombre: rd.GetString(2)),
                        ("@id", linea.ProductoId));
                    if (fila.Count == 0)
                        throw new ConflictoDatosException("Uno de los productos ya no está disponible.");

                    var (stock, precio, nombre) = fila[0];
                    if (linea.Cantidad > stock)
                        throw new ConflictoDatosException(
                            $"Stock insuficiente de \"{nombre}\": disponible {stock}, solicitado {linea.Cantidad}.");

                    items.Add((linea, stock, precio, nombre));
                    total += precio * linea.Cantidad;
                }

                // 2) Cabecera de la venta
                int ventaId = op.Insertar(
                    "INSERT INTO ventas (cliente_id, cliente_ocasional, usuario_id, total) VALUES (@c, @o, @u, @t)",
                    ("@c", clienteId), ("@o", clienteId == null ? clienteOcasional : null),
                    ("@u", usuarioId), ("@t", total));

                // 3) Detalle, descuento de stock y movimiento de inventario
                string motivo = "Venta V-" + ventaId.ToString("D6");
                foreach (var (linea, stock, precio, _) in items)
                {
                    op.Ejecutar(
                        "INSERT INTO detalle_ventas (venta_id, producto_id, cantidad, precio_unitario, subtotal) " +
                        "VALUES (@v, @p, @c, @pr, @s)",
                        ("@v", ventaId), ("@p", linea.ProductoId), ("@c", linea.Cantidad),
                        ("@pr", precio), ("@s", precio * linea.Cantidad));

                    int nuevo = stock - linea.Cantidad;
                    op.Ejecutar("UPDATE productos SET stock = @n WHERE id = @id", ("@n", nuevo), ("@id", linea.ProductoId));
                    op.Ejecutar(
                        "INSERT INTO movimientos_inventario (producto_id, tipo, cantidad, stock_anterior, stock_nuevo, motivo, usuario_id, venta_id) " +
                        "VALUES (@p, 'VENTA', @c, @a, @n, @m, @u, @v)",
                        ("@p", linea.ProductoId), ("@c", linea.Cantidad), ("@a", stock), ("@n", nuevo),
                        ("@m", motivo), ("@u", usuarioId), ("@v", ventaId));
                }
                return ventaId;
            });

        public IList<Venta> Buscar(string texto, int? numero, DateTime desde, DateTime hastaExclusivo, int? usuarioId = null, int? clienteId = null) =>
            _db.Consultar(
                CabeceraSql +
                "WHERE v.fecha >= @d AND v.fecha < @h " +
                "AND (@usr IS NULL OR v.usuario_id = @usr) AND (@cli IS NULL OR v.cliente_id = @cli) " +
                "AND (@t = '' OR (@num IS NOT NULL AND v.id = @num) OR COALESCE(c.nombre_completo, v.cliente_ocasional) LIKE @like) " +
                "ORDER BY v.fecha DESC, v.id DESC LIMIT 500",
                Mapear,
                ("@d", desde), ("@h", hastaExclusivo), ("@usr", usuarioId), ("@cli", clienteId),
                ("@t", texto), ("@num", numero),
                ("@like", "%" + texto.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%"));

        public Venta? ObtenerConDetalle(int id) =>
            _db.Usar(op =>
            {
                var venta = op.Consultar(CabeceraSql + "WHERE v.id = @id", Mapear, ("@id", id)).FirstOrDefault();
                if (venta == null) return null;
                venta.Detalles = op.Consultar(
                    "SELECT d.producto_id, p.codigo, p.nombre, d.cantidad, d.precio_unitario, d.subtotal " +
                    "FROM detalle_ventas d JOIN productos p ON p.id = d.producto_id WHERE d.venta_id = @id ORDER BY d.id",
                    rd => new DetalleVenta
                    {
                        ProductoId = rd.GetInt32(0), ProductoCodigo = rd.GetString(1), ProductoNombre = rd.GetString(2),
                        Cantidad = rd.GetInt32(3), PrecioUnitario = rd.GetDecimal(4), Subtotal = rd.GetDecimal(5)
                    }, ("@id", id));
                return venta;
            });

        private const string CabeceraSql =
            "SELECT v.id, v.fecha, v.cliente_id, COALESCE(c.nombre_completo, v.cliente_ocasional) AS cliente, " +
            "v.usuario_id, u.nombre_completo AS usuario, v.total " +
            "FROM ventas v LEFT JOIN clientes c ON c.id = v.cliente_id JOIN usuarios u ON u.id = v.usuario_id ";

        private static Venta Mapear(MySqlDataReader rd) => new()
        {
            Id = rd.GetInt32("id"),
            Fecha = rd.GetDateTime("fecha"),
            ClienteId = rd.IsDBNull(rd.GetOrdinal("cliente_id")) ? null : rd.GetInt32("cliente_id"),
            ClienteNombre = rd.Texto("cliente"),
            UsuarioId = rd.GetInt32("usuario_id"),
            UsuarioNombre = rd.GetString("usuario"),
            Total = rd.GetDecimal("total")
        };
    }
}
