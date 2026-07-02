namespace PedidosApi.DTOs.Ai
{
    public class AiChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public List<AiMessage>? History { get; set; }
    }

    public class AiMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
