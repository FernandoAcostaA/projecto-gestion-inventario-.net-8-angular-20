using PedidosApi.DTOs.Ai;

namespace PedidosApi.Services
{
    public interface IAiService
    {
        Task<AiChatResponse> ChatAsync(AiChatRequest request);
        Task<string> GetRecommendationAsync(string type);
        Task<List<Dictionary<string, object>>> GetAutocompleteAsync(string entityType, string query, string? field = null);
        Task<string> GenerateReportAsync(string prompt, string? period = null);
        Task<string> GetClientInsightsAsync(int idCliente);
        Task<List<Dictionary<string, object>>> SearchArticlesByAIAsync(string query);
        Task<string> GetDashboardSummaryAsync(string? period = null);
    }
}
