namespace PedidosApi.DTOs.Ai
{
    public class AiChatResponse
    {
        public string Reply { get; set; } = string.Empty;
        public List<AiSuggestedAction>? SuggestedActions { get; set; }
        public List<AiDataChange>? DataChanges { get; set; }
    }

    public class AiSuggestedAction
    {
        public string Label { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string? Action { get; set; }
    }

    public class AiDataChange
    {
        public string Entity { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
    }
}
