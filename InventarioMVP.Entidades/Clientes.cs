namespace InventarioMVP.Entidades
{
    /// <summary>Cliente. Las propiedades de resumen se llenan en las consultas de listado.</summary>
    public class Cliente
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }

        // Resumen de compras
        public int NumCompras { get; set; }
        public decimal TotalComprado { get; set; }
        public DateTime? UltimaCompra { get; set; }

        public override string ToString() => NombreCompleto;
    }
}
