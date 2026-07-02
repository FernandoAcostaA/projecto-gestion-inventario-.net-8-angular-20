using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PedidosApi.DTOs.Ai;
using PedidosApi.Services;

namespace PedidosApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;

        public AiController(IAiService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("chat")]
        public async Task<ActionResult<AiChatResponse>> Chat([FromBody] AiChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { error = "El mensaje no puede estar vacío." });

            var result = await _aiService.ChatAsync(request);
            return Ok(result);
        }

        [HttpPost("recommend")]
        public async Task<ActionResult<string>> Recommend([FromBody] AiRecommendRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Type))
                return BadRequest(new { error = "El tipo de recomendación es requerido." });

            var result = await _aiService.GetRecommendationAsync(request.Type);
            return Ok(new { recommendation = result });
        }

        [HttpPost("autocomplete")]
        public async Task<ActionResult> Autocomplete([FromBody] AiAutocompleteRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.EntityType) || string.IsNullOrWhiteSpace(request.Query))
                return BadRequest(new { error = "EntityType y Query son requeridos." });

            var result = await _aiService.GetAutocompleteAsync(request.EntityType, request.Query, request.Field);
            return Ok(result);
        }

        [HttpPost("cliente-insights")]
        public async Task<ActionResult<string>> GetClientInsights([FromBody] AiClientInsightsRequest request)
        {
            if (request.IdCliente <= 0)
                return BadRequest(new { error = "IdCliente es requerido." });

            var result = await _aiService.GetClientInsightsAsync(request.IdCliente);
            return Ok(new { insights = result });
        }

        [HttpPost("buscar-articulos")]
        public async Task<ActionResult> SearchArticles([FromBody] AiSearchArticlesRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
                return BadRequest(new { error = "Query es requerido." });

            var result = await _aiService.SearchArticlesByAIAsync(request.Query);
            return Ok(result);
        }

        [HttpPost("dashboard-summary")]
        public async Task<ActionResult<string>> GetDashboardSummary([FromBody] AiDashboardSummaryRequest request)
        {
            var result = await _aiService.GetDashboardSummaryAsync(request.Period);
            return Ok(new { summary = result });
        }

        [HttpPost("report")]
        public async Task<ActionResult<string>> GenerateReport([FromBody] AiReportRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
                return BadRequest(new { error = "El prompt del reporte es requerido." });

            var result = await _aiService.GenerateReportAsync(request.Prompt, request.Period);
            return Ok(new { report = result });
        }
    }
}
