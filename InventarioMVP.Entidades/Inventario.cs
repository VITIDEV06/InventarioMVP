namespace InventarioMVP.Entidades
{
    public static class TipoMovimiento
    {
        public const string Entrada = "ENTRADA";
        public const string Salida = "SALIDA";
        public const string Venta = "VENTA";
    }

    public class MovimientoInventario
    {
        public long Id { get; set; }
        public int ProductoId { get; set; }
        public string ProductoCodigo { get; set; } = string.Empty;
        public string ProductoNombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public int StockAnterior { get; set; }
        public int StockNuevo { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string UsuarioNombre { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}
