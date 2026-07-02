namespace PedidosApi.DTOs.Reportes
{
    public class VentasPorPeriodoDto
    {
        public string Periodo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
    }
}
