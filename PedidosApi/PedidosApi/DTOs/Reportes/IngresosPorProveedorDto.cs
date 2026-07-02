namespace PedidosApi.DTOs.Reportes
{
    public class IngresosPorProveedorDto
    {
        public int IdProveedor { get; set; }
        public string Proveedor { get; set; } = string.Empty;
        public int CantidadCompras { get; set; }
        public decimal TotalGastado { get; set; }
    }
}
