namespace PedidosApi.DTOs
{
    public class VentaDto
    {
        public int IdVenta { get; set; }
        public int IdCliente { get; set; }
        public int? IdTrabajador { get; set; }
        public DateTime Fecha { get; set; }
        public string TipoComprobante { get; set; } = string.Empty;
        public string Serie { get; set; } = string.Empty;
        public string Correlativo { get; set; } = string.Empty;
        public decimal Igv { get; set; }

        public string? ClienteNombre { get; set; }
        public string? TrabajadorNombre { get; set; }

        public List<DetalleVentaDto> Detalles { get; set; } = new();
    }
}
