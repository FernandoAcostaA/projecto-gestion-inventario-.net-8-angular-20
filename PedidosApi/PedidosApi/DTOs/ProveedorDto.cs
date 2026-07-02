namespace PedidosApi.DTOs
{
    public class ProveedorDto
    {
        public int IdProveedor { get; set; }
        public string RazonSocial { get; set; } = string.Empty;
        public string SectorComercial { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = string.Empty;
        public string NumDocumento { get; set; } = string.Empty;
        public string? Direccion { get; set; }   // ✅ Añadido
        public string? Telefono { get; set; }
        public string? Email { get; set; }
    }
}
