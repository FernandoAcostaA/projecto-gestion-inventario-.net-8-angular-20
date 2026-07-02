namespace PedidosApi.DTOs.Ai
{
    public class AiReportRequest
    {
        public string Prompt { get; set; } = string.Empty;
        public string? Period { get; set; }
    }
}
