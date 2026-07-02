namespace PedidosApi.DTOs
{
    public class VentaConDetallesDto
    {
        public int IdCliente { get; set; }
        public int? IdTrabajador { get; set; }
        public DateTime Fecha { get; set; }
        public string TipoComprobante { get; set; } = string.Empty;
        public string Serie { get; set; } = string.Empty;
        public string Correlativo { get; set; } = string.Empty;
        public decimal Igv { get; set; }

        public List<DetalleVentaDto> Detalles { get; set; } = new();
    }

    public class ReposicionStockDto
    {
        public int IdArticulo { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
    }
}
