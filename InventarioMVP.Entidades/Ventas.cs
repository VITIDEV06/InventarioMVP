namespace InventarioMVP.Entidades
{
    /// <summary>Línea solicitada al registrar una venta (el precio real lo toma la base de datos).</summary>
    public class LineaVenta
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }

    public class DetalleVenta
    {
        public int ProductoId { get; set; }
        public string ProductoCodigo { get; set; } = string.Empty;
        public string ProductoNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class Venta
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int? ClienteId { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public List<DetalleVenta> Detalles { get; set; } = new();

        public string Numero => "V-" + Id.ToString("D6");
    }
}
