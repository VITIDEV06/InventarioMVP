namespace InventarioMVP.Entidades
{
    /// <summary>Producto del inventario.</summary>
    public class Producto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; } = 5;

        public bool StockBajo => Stock <= StockMinimo;
        public string Estado => Stock == 0 ? "Agotado" : StockBajo ? "Stock bajo" : "Disponible";
        public override string ToString() => $"{Codigo} - {Nombre}";
    }
}
