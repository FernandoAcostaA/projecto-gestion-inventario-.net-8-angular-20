namespace PedidosApi.DTOs
{
    public class DetalleIngresoDto
    {
        public int IdDetalleIngreso { get; set; }
        public int IdArticulo { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public int StockInicial { get; set; }
        public int StockActual { get; set; }
        public DateTime FechaProduccion { get; set; }
        public DateTime FechaVencimiento { get; set; }

        public string? ArticuloNombre { get; set; }
        public int IdIngreso { get; set; }
    }
}
