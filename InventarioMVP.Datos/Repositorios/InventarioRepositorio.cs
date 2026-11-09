using InventarioMVP.Entidades;
using MySql.Data.MySqlClient;

namespace InventarioMVP.Datos.Repositorios
{
    public interface IInventarioRepositorio
    {
        /// <summary>
        /// Registra una ENTRADA o SALIDA y actualiza el stock en una sola transacción
        /// (la fila del producto se bloquea para evitar inconsistencias). Devuelve el stock resultante.
        /// </summary>
        int RegistrarMovimiento(int productoId, string tipo, int cantidad, string motivo, int usuarioId);

        IList<MovimientoInventario> ObtenerMovimientos(DateTime desde, DateTime hastaExclusivo, int? productoId = null, int? usuarioId = null);
    }

    public class InventarioRepositorio : IInventarioRepositorio
    {
        private readonly ConexionMySql _db;

        public InventarioRepositorio(ConexionMySql db) => _db = db;

        public int RegistrarMovimiento(int productoId, string tipo, int cantidad, string motivo, int usuarioId) =>
            _db.EnTransaccion(op =>
            {
                var fila = op.Consultar(
                    "SELECT stock, nombre FROM productos WHERE id = @id AND activo = 1 FOR UPDATE",
                    rd => (Stock: rd.GetInt32(0), Nombre: rd.GetString(1)), ("@id", productoId));
                if (fila.Count == 0) throw new ConflictoDatosException("El producto ya no está disponible.");

                var (stock, nombre) = fila[0];
                if (tipo == TipoMovimiento.Salida && cantidad > stock)
                    throw new ConflictoDatosException($"Stock insuficiente de \"{nombre}\": disponible {stock}, solicitado {cantidad}.");

                int nuevo = tipo == TipoMovimiento.Entrada ? stock + cantidad : stock - cantidad;
                op.Ejecutar("UPDATE productos SET stock = @n WHERE id = @id", ("@n", nuevo), ("@id", productoId));
                op.Ejecutar(
                    "INSERT INTO movimientos_inventario (producto_id, tipo, cantidad, stock_anterior, stock_nuevo, motivo, usuario_id) " +
                    "VALUES (@p, @t, @c, @a, @n, @m, @u)",
                    ("@p", productoId), ("@t", tipo), ("@c", cantidad), ("@a", stock), ("@n", nuevo),
                    ("@m", string.IsNullOrWhiteSpace(motivo) ? null : motivo), ("@u", usuarioId));
                return nuevo;
            });

        public IList<MovimientoInventario> ObtenerMovimientos(DateTime desde, DateTime hastaExclusivo, int? productoId = null, int? usuarioId = null) =>
            _db.Consultar(
                "SELECT m.id, m.producto_id, p.codigo, p.nombre, m.tipo, m.cantidad, m.stock_anterior, m.stock_nuevo, " +
                "m.motivo, u.nombre_completo AS usuario, m.fecha " +
                "FROM movimientos_inventario m " +
                "JOIN productos p ON p.id = m.producto_id " +
                "JOIN usuarios u ON u.id = m.usuario_id " +
                "WHERE m.fecha >= @d AND m.fecha < @h " +
                "AND (@prod IS NULL OR m.producto_id = @prod) AND (@usr IS NULL OR m.usuario_id = @usr) " +
                "ORDER BY m.fecha DESC, m.id DESC LIMIT 1000",
                Mapear, ("@d", desde), ("@h", hastaExclusivo), ("@prod", productoId), ("@usr", usuarioId));

        private static MovimientoInventario Mapear(MySqlDataReader rd) => new()
        {
            Id = rd.GetInt64("id"),
            ProductoId = rd.GetInt32("producto_id"),
            ProductoCodigo = rd.GetString("codigo"),
            ProductoNombre = rd.GetString("nombre"),
            Tipo = rd.GetString("tipo"),
            Cantidad = rd.GetInt32("cantidad"),
            StockAnterior = rd.GetInt32("stock_anterior"),
            StockNuevo = rd.GetInt32("stock_nuevo"),
            Motivo = rd.Texto("motivo"),
            UsuarioNombre = rd.GetString("usuario"),
            Fecha = rd.GetDateTime("fecha")
        };
    }
}
