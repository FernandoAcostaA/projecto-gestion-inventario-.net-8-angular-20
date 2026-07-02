namespace PedidosApi.DTOs.Ai
{
    public class AiAutocompleteRequest
    {
        public string EntityType { get; set; } = string.Empty;
        public string Query { get; set; } = string.Empty;
        public string? Field { get; set; }
    }
}
