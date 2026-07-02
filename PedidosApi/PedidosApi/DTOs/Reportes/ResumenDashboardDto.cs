namespace PedidosApi.DTOs.Reportes
{
    public class ResumenDashboardDto
    {
        public int TotalClientes { get; set; }
        public int TotalProveedores { get; set; }
        public int TotalArticulos { get; set; }
        public int VentasHoy { get; set; }
        public decimal TotalVentasHoy { get; set; }
        public int IngresosHoy { get; set; }
        public decimal TotalIngresosHoy { get; set; }
        public int ArticulosStockBajo { get; set; }
        public decimal VentasMesActual { get; set; }
        public decimal IngresosMesActual { get; set; }
    }
}
