namespace InventarioMVP.Entidades
{
    public class Actividad
    {
        public long Id { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }

    public record PuntoVenta(DateTime Dia, decimal Total);

    public class ProductoVendido
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int Unidades { get; set; }
        public decimal Ingresos { get; set; }
    }

    /// <summary>Tabla genérica de un reporte: sirve para mostrar en pantalla y para exportar.</summary>
    public class TablaReporte
    {
        public string Titulo { get; set; } = string.Empty;
        public List<string> Columnas { get; set; } = new();
        public List<object?[]> Filas { get; set; } = new();
    }

    public enum PeriodoIngresos { Hoy, Semana, Mes, Anio, Todo }

    /// <summary>Datos del dashboard. Todo proviene de la base de datos.</summary>
    public class DashboardDatos
    {
        public bool EsGeneral { get; set; }
        public int TotalProductos { get; set; }
        public int ProductosStockBajo { get; set; }
        public int TotalClientes { get; set; }
        public int ClientesNuevos { get; set; }
        public decimal Ingresos { get; set; }
        public List<PuntoVenta> VentasPorDia { get; set; } = new();
        public List<Venta> UltimasVentas { get; set; } = new();
        public List<ProductoVendido> MasVendidos { get; set; } = new();
        public List<Producto> StockBajo { get; set; } = new();
        public List<Actividad> Actividad { get; set; } = new();
    }
}
