namespace PedidosApi.DTOs
{
    public class ArticuloDto
    {
        public int IdArticulo { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public byte[]? Imagen { get; set; }
        public int IdCategoria { get; set; }
        public string? CategoriaNombre { get; set; }
        public int IdPresentacion { get; set; }
        public string? PresentacionNombre { get; set; }
        public int StockTotal { get; set; }
    }
}
