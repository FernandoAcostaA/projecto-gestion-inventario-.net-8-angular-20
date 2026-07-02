namespace PedidosApi.DTOs
{
    public class ArticuloCreateDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public byte[]? Imagen { get; set; }
        public int IdCategoria { get; set; }
        public int IdPresentacion { get; set; }
    }
}
