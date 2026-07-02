namespace PedidosApi.DTOs
{
    public class IngresoDto
    {
        public int IdIngreso { get; set; }
        public int IdProveedor { get; set; }
        public int IdTrabajador { get; set; }
        public DateTime Fecha { get; set; }
        public string TipoComprobante { get; set; } = string.Empty;
        public string Serie { get; set; } = string.Empty;
        public string Correlativo { get; set; } = string.Empty;
        public decimal Igv { get; set; }
        public string Estado { get; set; } = string.Empty;

        // ✅ Nombres descriptivos
        public string? NombreProveedor { get; set; }
        public string? NombreTrabajador { get; set; }
    }
}
