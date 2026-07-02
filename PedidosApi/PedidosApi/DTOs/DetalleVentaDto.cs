namespace PedidosApi.DTOs
{
    public class DetalleVentaDto
    {
        public int IdDetalleVenta { get; set; }
        public int IdVenta { get; set; }
        public int IdDetalleIngreso { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal Descuento { get; set; }

        public string? ArticuloNombre { get; set; }
        public string? CodigoVenta { get; set; }  // Ej: "B001-0000003"
        public string? ClienteNombre { get; set; }
    }
}
