namespace PedidosApi.DTOs.Reportes
{
    public class TopArticuloDto
    {
        public int IdArticulo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Categoria { get; set; }
        public int TotalVendido { get; set; }
        public decimal TotalIngresos { get; set; }
    }
}
