using System.ClientModel;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OpenAI;
using OpenAI.Chat;
using PedidosApi.Data;
using PedidosApi.DTOs.Ai;

namespace PedidosApi.Services
{
    public class AiService : IAiService
    {
        private readonly AppDbContext _context;
        private readonly ChatClient? _chatClient;
        private readonly string _model;
        private readonly bool _enabled;

        private static readonly string SystemPrompt = @"
Eres un asistente experto en gestión de inventarios y ventas para el sistema ""Ventas La Ganga"".
Tus funciones:
 
1. RESPONDER preguntas sobre el negocio usando las herramientas disponibles (consultar base de datos).
2. RECOMENDAR acciones basadas en datos (reordenar stock, promover productos, etc.).
3. AYUDAR con la carga de datos (autocompletar formularios).
4. GENERAR reportes e insights.
5. ANALIZAR clientes: historial de compras, frecuencia, productos favoritos, sugerencias de venta.
6. BUSCAR artículos por descripción en lenguaje natural (ej: ""gaseosas pequeñas"", ""lácteos"").
7. RESUMIR el estado del negocio: ventas del día/semana/mes, top productos, alertas de stock.
8. EDITAR registros: podés actualizar trabajadores, clientes, artículos y proveedores.
9. AGREGAR STOCK: podés aumentar el stock de un artículo existente.
 
Reglas:
- Sé conciso pero amable.
- Siempre que sea posible, ofrece acciones sugeridas que el usuario pueda ejecutar (ej: ""Ver artículos con stock bajo"").
- Si no tienes datos suficientes, usa las herramientas disponibles para consultar.
- ANTES de hacer cualquier cambio (editar o agregar stock), EXPLICÁ al usuario qué vas a hacer y PEDÍ CONFIRMACIÓN explícita. No modifiques nada sin aprobación.
- Cuando te pidan insights de un cliente, usá get_client_purchase_history.
- Cuando te pidan buscar artículos de forma natural, usá search_articles_natural.
- Cuando te pidan un resumen del negocio o dashboard, usá get_dashboard_insights.
- Cuando te pidan editar un trabajador, usá update_trabajador (campos: nombre, apellidos, telefono, direccion, email).
- Cuando te pidan editar un cliente, usá update_cliente (campos: nombre, apellidos, telefono, direccion, email).
- Cuando te pidan editar un artículo, usá update_articulo (campos: nombre, descripcion, codigo).
- Cuando te pidan editar un proveedor, usá update_proveedor (campos: razonSocial, telefono, direccion, email).
- Cuando te pidan agregar stock a un artículo, usá add_stock_articulo.
- Cuando necesites buscar trabajadores por nombre, usá search_workers.
- Cuando necesites buscar proveedores, usá search_suppliers.
- Responde SIEMPRE en español.
 
Las herramientas disponibles te permiten consultar y modificar: artículos, clientes, proveedores, trabajadores, ventas, ingresos, reportes, auditoría, historial de clientes y análisis del dashboard.
";

        public AiService(AppDbContext context, IConfiguration config)
        {
            _context = context;

            var apiKey = config["Ai:ApiKey"];
            var endpoint = config["Ai:Endpoint"];
            _model = config["Ai:Model"] ?? "gpt-4o-mini";
            _enabled = !string.IsNullOrEmpty(apiKey);

            if (_enabled)
            {
                var credential = new ApiKeyCredential(apiKey!);
                if (!string.IsNullOrEmpty(endpoint))
                {
                    var clientOptions = new OpenAIClientOptions
                    {
                        Endpoint = new Uri(endpoint)
                    };
                    var client = new OpenAIClient(credential, clientOptions);
                    _chatClient = client.GetChatClient(_model);
                }
                else
                {
                    var client = new OpenAIClient(credential);
                    _chatClient = client.GetChatClient(_model);
                }
            }
        }

        public async Task<AiChatResponse> ChatAsync(AiChatRequest request)
        {
            if (!_enabled)
            {
                return new AiChatResponse
                {
                    Reply = "⚠️ La IA no está configurada.\n\nPara activarla, agregá una ApiKey y Endpoint en appsettings.json (sección Ai).\n\nOpciones GRATUITAS:\n• Groq: api.groq.com (modelo: llama-3.3-70b-versatile)\n• OpenRouter: openrouter.ai (modelos gratis)\n• Google Gemini: aistudio.google.com\n• Ollama local: http://localhost:11434/v1\n• DeepSeek: platform.deepseek.com"
                };
            }

            var messages = new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage(SystemPrompt)
            };

            if (request.History != null)
            {
                foreach (var msg in request.History)
                {
                    if (msg.Role == "user")
                        messages.Add(ChatMessage.CreateUserMessage(msg.Content));
                    else if (msg.Role == "assistant")
                        messages.Add(ChatMessage.CreateAssistantMessage(msg.Content));
                }
            }

            messages.Add(ChatMessage.CreateUserMessage(request.Message));

            var chatCompletionOptions = new ChatCompletionOptions
            {
                Temperature = 0.3f,
                MaxOutputTokenCount = 2000,
                ToolChoice = ChatToolChoice.CreateAutoChoice()
            };

            AddTools(chatCompletionOptions);

            var reply = new StringBuilder();
            var suggestedActions = new List<AiSuggestedAction>();
            var dataChanges = new List<AiDataChange>();

            var completion = await CallWithRetryAsync(() => _chatClient!.CompleteChatAsync(messages, chatCompletionOptions));

            var maxIterations = 5;
            while (completion.Value.FinishReason == ChatFinishReason.ToolCalls && maxIterations > 0)
            {
                maxIterations--;
                var assistantMsg = ChatMessage.CreateAssistantMessage(completion.Value);
                messages.Add(assistantMsg);

                foreach (var toolCall in completion.Value.ToolCalls)
                {
                    var fn = toolCall.FunctionName.ToLowerInvariant();
                    var result = await ExecuteToolAsync(toolCall.FunctionName, toolCall.FunctionArguments.ToString());
                    messages.Add(ChatMessage.CreateToolMessage(toolCall.Id, result));

                    if (fn == "get_dashboard_summary")
                    {
                        suggestedActions.Add(new AiSuggestedAction
                        {
                            Label = "Ir al Dashboard",
                            Route = "/reportes"
                        });
                    }

                    // Detectar cambios de datos
                    var change = GetChangeFromToolCall(fn);
                    if (change != null)
                        dataChanges.Add(change);
                }

                completion = await CallWithRetryAsync(() => _chatClient!.CompleteChatAsync(messages, chatCompletionOptions));
            }

            reply.Append(completion.Value.Content[0].Text);
            suggestedActions.AddRange(ExtractActionsFromResponse(reply.ToString()));

            return new AiChatResponse
            {
                Reply = reply.ToString(),
                SuggestedActions = suggestedActions.Count > 0 ? suggestedActions : null,
                DataChanges = dataChanges.Count > 0 ? dataChanges : null
            };

            async Task<ClientResult<ChatCompletion>> CallWithRetryAsync(Func<Task<ClientResult<ChatCompletion>>> call)
            {
                var maxRetries = 3;
                var delay = 1000;
                for (var retry = 0; ; retry++)
                {
                    try
                    {
                        return await call();
                    }
                    catch (ClientResultException ex) when (ex.Status == 429 && retry < maxRetries)
                    {
                        await Task.Delay(delay);
                        delay *= 2;
                    }
                }
            }
        }

        public async Task<string> GetRecommendationAsync(string type)
        {
            if (!_enabled)
                return "IA no configurada.";

            var contextData = type switch
            {
                "low_stock" => await GetLowStockContextAsync(),
                "top_selling" => await GetTopSellingContextAsync(),
                "anomalies" => await GetAnomaliesContextAsync(),
                _ => await GetDashboardContextAsync()
            };

            var prompt = $@"Basado en los siguientes datos del negocio, genera recomendaciones accionables:
{contextData}

Responde en español con recomendaciones específicas y concretas.";

            var messages = new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage("Eres un analista de negocios experto en retail e inventarios."),
                ChatMessage.CreateUserMessage(prompt)
            };

            var completion = await _chatClient!.CompleteChatAsync(messages, new ChatCompletionOptions
            {
                Temperature = 0.3f,
                MaxOutputTokenCount = 1500
            });

            return completion.Value.Content[0].Text;
        }

        public async Task<List<Dictionary<string, object>>> GetAutocompleteAsync(string entityType, string query, string? field = null)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return new List<Dictionary<string, object>>();

            var lowerQuery = query.ToLower();

            switch (entityType.ToLower())
            {
                case "articulo":
                    var articulos = await _context.Articulos
                        .Where(a => a.Nombre.ToLower().Contains(lowerQuery) || a.Codigo.ToLower().Contains(lowerQuery))
                        .Take(10)
                        .Select(a => new { a.Id, a.Nombre, a.Codigo })
                        .ToListAsync();
                    return articulos.Select(a => new Dictionary<string, object>
                    {
                        ["id"] = a.Id,
                        ["nombre"] = a.Nombre,
                        ["codigo"] = a.Codigo ?? ""
                    }).ToList();

                case "cliente":
                    var clientes = await _context.Clientes
                        .Where(c => c.Nombre.ToLower().Contains(lowerQuery) || (c.Apellidos != null && c.Apellidos.ToLower().Contains(lowerQuery)))
                        .Take(10)
                        .Select(c => new { c.IdCliente, c.Nombre, c.Apellidos, c.NumDocumento })
                        .ToListAsync();
                    return clientes.Select(c => new Dictionary<string, object>
                    {
                        ["id"] = c.IdCliente,
                        ["nombre"] = c.Nombre + " " + (c.Apellidos ?? ""),
                        ["documento"] = c.NumDocumento ?? ""
                    }).ToList();

                case "proveedor":
                    var proveedores = await _context.Proveedores
                        .Where(p => p.RazonSocial.ToLower().Contains(lowerQuery))
                        .Take(10)
                        .Select(p => new { p.IdProveedor, p.RazonSocial, p.NumDocumento })
                        .ToListAsync();
                    return proveedores.Select(p => new Dictionary<string, object>
                    {
                        ["id"] = p.IdProveedor,
                        ["nombre"] = p.RazonSocial,
                        ["documento"] = p.NumDocumento ?? ""
                    }).ToList();

                default:
                    return new List<Dictionary<string, object>>();
            }
        }

        public async Task<string> GenerateReportAsync(string prompt, string? period = null)
        {
            if (!_enabled)
                return "IA no configurada.";

            var contextData = await GetFullContextAsync(period);
            var fullPrompt = $@"Genera un reporte de negocio detallado basado en los siguientes datos:
{contextData}

Instrucciones del usuario: {prompt}

Genera un reporte bien estructurado con:
1. Resumen ejecutivo
2. Análisis de datos relevantes
3. Tendencias identificadas
4. Recomendaciones accionables

Responde en español con formato claro.";

            var messages = new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage("Eres un analista senior de negocios. Genera reportes profesionales y detallados."),
                ChatMessage.CreateUserMessage(fullPrompt)
            };

            var completion = await _chatClient!.CompleteChatAsync(messages, new ChatCompletionOptions
            {
                Temperature = 0.3f,
                MaxOutputTokenCount = 3000
            });

            return completion.Value.Content[0].Text;
        }

        public async Task<string> GetClientInsightsAsync(int idCliente)
        {
            if (!_enabled)
                return "IA no configurada.";

            var cliente = await _context.Clientes.FindAsync(idCliente);
            if (cliente == null)
                return "Cliente no encontrado.";

            var ventas = await _context.Ventas
                .Where(v => v.IdCliente == idCliente)
                .OrderByDescending(v => v.Fecha)
                .Take(5)
                .Select(v => new
                {
                    v.IdVenta,
                    v.Fecha,
                    Total = v.DetallesVenta.Sum(d => d.Cantidad * d.PrecioVenta - d.Descuento),
                    CantidadArticulos = v.DetallesVenta.Sum(d => d.Cantidad)
                })
                .ToListAsync();

            var totalGastado = await _context.DetallesVenta
                .Where(dv => dv.Venta!.IdCliente == idCliente)
                .SumAsync(dv => dv.Cantidad * dv.PrecioVenta - dv.Descuento);

            var totalCompras = await _context.Ventas.CountAsync(v => v.IdCliente == idCliente);
            var ultimaCompra = ventas.FirstOrDefault();

            var topArticulos = await _context.DetallesVenta
                .Where(dv => dv.Venta!.IdCliente == idCliente)
                .GroupBy(dv => new { dv.DetalleIngreso!.IdArticulo, dv.DetalleIngreso!.Articulo!.Nombre })
                .Select(g => new
                {
                    g.Key.Nombre,
                    totalComprado = g.Sum(x => x.Cantidad)
                })
                .OrderByDescending(x => x.totalComprado)
                .Take(5)
                .ToListAsync();

            var contextData = JsonSerializer.Serialize(new
            {
                cliente = $"{cliente.Nombre} {cliente.Apellidos}",
                clienteDocumento = cliente.NumDocumento,
                totalComprasRealizadas = totalCompras,
                totalGastado = totalGastado,
                ultimaCompra = ultimaCompra?.Fecha.ToString("yyyy-MM-dd"),
                ultimoMonto = ultimaCompra?.Total ?? 0,
                ventasRecientes = ventas,
                articulosFrecuentes = topArticulos
            });

            var prompt = $@"Eres un vendedor experto. Analiza el siguiente historial de un cliente y genera:
1. Un resumen breve del cliente (tipo de cliente, frecuencia de compra)
2. Recomendaciones de productos que podrían interesarle
3. Sugerencia para el vendedor (cómo tratar al cliente, qué ofrecerle)

Datos del cliente:
{contextData}

Responde en español, sé conciso y práctico (máximo 4 párrafos).";

            var messages = new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage("Eres un vendedor experto y analista de clientes."),
                ChatMessage.CreateUserMessage(prompt)
            };

            var completion = await _chatClient!.CompleteChatAsync(messages, new ChatCompletionOptions
            {
                Temperature = 0.4f,
                MaxOutputTokenCount = 1000
            });

            return completion.Value.Content[0].Text;
        }

        public async Task<List<Dictionary<string, object>>> SearchArticlesByAIAsync(string query)
        {
            if (!_enabled || string.IsNullOrWhiteSpace(query))
                return new List<Dictionary<string, object>>();

            var prompt = $@"Interpreta la siguiente búsqueda en lenguaje natural de un usuario que busca artículos en un negocio.

Búsqueda: ""{query}""

Extrae palabras clave de búsqueda (nombre, categoría, presentación, código) y responde SOLO con un JSON así:
{{""keywords"": ""palabras clave separadas por espacio"", ""categoria"": ""nombre de categoría o null"", ""presentacion"": ""nombre de presentación o null""}}

No agregues explicaciones, solo el JSON.";

            var messages = new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage("Eres un asistente que interpreta búsquedas de inventario. Responde solo con JSON válido."),
                ChatMessage.CreateUserMessage(prompt)
            };

            try
            {
                var completion = await _chatClient!.CompleteChatAsync(messages, new ChatCompletionOptions
                {
                    Temperature = 0.1f,
                    MaxOutputTokenCount = 200
                });

                var interpretation = completion.Value.Content[0].Text.Trim();
                interpretation = interpretation.Replace("```json", "").Replace("```", "").Trim();

                var parsed = JsonSerializer.Deserialize<JsonElement>(interpretation);
                var keywords = TryGetString(parsed, "keywords", query);
                var categoria = TryGetString(parsed, "categoria", null);
                var presentacion = TryGetString(parsed, "presentacion", null);

                var lowerKeywords = keywords.ToLower();

                var articulosQuery = _context.Articulos.AsQueryable();

                if (!string.IsNullOrEmpty(categoria))
                {
                    var catLower = categoria.ToLower();
                    articulosQuery = articulosQuery.Where(a => a.Categoria!.Nombre.ToLower().Contains(catLower));
                }

                if (!string.IsNullOrEmpty(presentacion))
                {
                    var presLower = presentacion.ToLower();
                    articulosQuery = articulosQuery.Where(a => a.Presentacion!.Nombre.ToLower().Contains(presLower));
                }

                var results = await articulosQuery
                    .Where(a => a.Nombre.ToLower().Contains(lowerKeywords) || a.Codigo.ToLower().Contains(lowerKeywords) || a.Descripcion!.ToLower().Contains(lowerKeywords))
                    .Take(10)
                    .Select(a => new { a.Id, a.Nombre, a.Codigo, Categoria = a.Categoria!.Nombre, Presentacion = a.Presentacion!.Nombre })
                    .ToListAsync();

                if (results.Count == 0)
                {
                    results = await _context.Articulos
                        .Where(a => a.Nombre.ToLower().Contains(lowerKeywords) || a.Codigo.ToLower().Contains(lowerKeywords))
                        .Take(10)
                        .Select(a => new { a.Id, a.Nombre, a.Codigo, Categoria = a.Categoria!.Nombre, Presentacion = a.Presentacion!.Nombre })
                        .ToListAsync();
                }

                return results.Select(a => new Dictionary<string, object>
                {
                    ["id"] = a.Id,
                    ["nombre"] = a.Nombre,
                    ["codigo"] = a.Codigo ?? "",
                    ["categoria"] = a.Categoria,
                    ["presentacion"] = a.Presentacion
                }).ToList();
            }
            catch
            {
                var lowerQuery = query.ToLower();
                var fallback = await _context.Articulos
                    .Where(a => a.Nombre.ToLower().Contains(lowerQuery) || a.Codigo.ToLower().Contains(lowerQuery))
                    .Take(10)
                    .Select(a => new { a.Id, a.Nombre, a.Codigo })
                    .ToListAsync();

                return fallback.Select(a => new Dictionary<string, object>
                {
                    ["id"] = a.Id,
                    ["nombre"] = a.Nombre,
                    ["codigo"] = a.Codigo ?? ""
                }).ToList();
            }
        }

        public async Task<string> GetDashboardSummaryAsync(string? period = null)
        {
            if (!_enabled)
                return "IA no configurada.";

            var hoy = DateTime.Today;
            var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek);
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            var ventasHoy = await _context.Ventas.CountAsync(v => v.Fecha.Date == hoy);
            var ventasSemana = await _context.Ventas.CountAsync(v => v.Fecha >= inicioSemana);
            var ventasMes = await _context.Ventas.CountAsync(v => v.Fecha >= inicioMes);

            var montoHoy = await _context.DetallesVenta
                .Where(dv => dv.Venta!.Fecha.Date == hoy)
                .SumAsync(dv => dv.Cantidad * dv.PrecioVenta - dv.Descuento);
            var montoMes = await _context.DetallesVenta
                .Where(dv => dv.Venta!.Fecha >= inicioMes)
                .SumAsync(dv => dv.Cantidad * dv.PrecioVenta - dv.Descuento);

            var totalArticulos = await _context.Articulos.CountAsync();
            var totalClientes = await _context.Clientes.CountAsync();

            var stockBajo = await _context.DetallesIngreso
                .Where(di => di.StockActual <= 5)
                .GroupBy(di => di.Articulo!.Nombre)
                .Select(g => g.Key)
                .ToListAsync();

            var topVendidos = await _context.DetallesVenta
                .Where(dv => dv.Venta!.Fecha >= inicioMes)
                .GroupBy(dv => dv.DetalleIngreso!.Articulo!.Nombre)
                .Select(g => new { articulo = g.Key, total = g.Sum(x => x.Cantidad) })
                .OrderByDescending(x => x.total)
                .Take(5)
                .ToListAsync();

            var contextData = JsonSerializer.Serialize(new
            {
                fecha = hoy.ToString("yyyy-MM-dd"),
                ventasHoy = new { cantidad = ventasHoy, monto = montoHoy },
                ventasSemana = new { cantidad = ventasSemana },
                ventasMes = new { cantidad = ventasMes, monto = montoMes },
                totalArticulos,
                totalClientes,
                articulosStockBajo = stockBajo.Take(10),
                topVendidos
            });

            var prompt = $@"Eres un analista de negocios. Genera un resumen ejecutivo del estado del negocio basado en estos datos:
{contextData}

Incluye:
1. Resumen de ventas (hoy, semana, mes) con montos
2. Productos más vendidos del mes
3. Alertas de stock bajo (menciona cuántos y ejemplos)
4. Recomendaciones accionables para mejorar

Responde en español, con formato claro y conciso (máximo 5 párrafos). Usa un tono profesional pero amigable.";

            var messages = new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage("Eres un analista senior de negocios especializado en retail."),
                ChatMessage.CreateUserMessage(prompt)
            };

            var completion = await _chatClient!.CompleteChatAsync(messages, new ChatCompletionOptions
            {
                Temperature = 0.3f,
                MaxOutputTokenCount = 1500
            });

            return completion.Value.Content[0].Text;
        }

        private void AddTools(ChatCompletionOptions options)
        {
            options.Tools.Add(ChatTool.CreateFunctionTool(
                functionName: "get_dashboard_summary",
                functionDescription: "Resumen del dashboard: clientes, productos, ventas, stock bajo"
            ));

            options.Tools.Add(ChatTool.CreateFunctionTool(
                functionName: "get_low_stock_articles",
                functionDescription: "Artículos con stock <= 5"
            ));

            options.Tools.Add(ChatTool.CreateFunctionTool(
                functionName: "get_top_selling_articles",
                functionDescription: "Artículos más vendidos en un período",
                functionParameters: BinaryData.FromString("{\"type\":\"object\",\"properties\":{\"top\":{\"type\":[\"integer\",\"string\"],\"description\":\"Cantidad\"},\"fechaInicio\":{\"type\":\"string\",\"description\":\"Inicio ISO\"},\"fechaFin\":{\"type\":\"string\",\"description\":\"Fin ISO\"}}}")
            ));

            options.Tools.Add(ChatTool.CreateFunctionTool(
                functionName: "get_sales_by_period",
                functionDescription: "Ventas agrupadas por día/semana/mes",
                functionParameters: BinaryData.FromString("{\"type\":\"object\",\"properties\":{\"fechaInicio\":{\"type\":\"string\",\"description\":\"Inicio ISO\"},\"fechaFin\":{\"type\":\"string\",\"description\":\"Fin ISO\"},\"agrupacion\":{\"type\":\"string\",\"description\":\"dia/semana/mes\"}}}")
            ));

            options.Tools.Add(ChatTool.CreateFunctionTool(
                functionName: "search_articles",
                functionDescription: "Busca artículos por nombre o código",
                functionParameters: BinaryData.FromString("{\"type\":\"object\",\"properties\":{\"query\":{\"type\":\"string\",\"description\":\"texto\"}}}")
            ));

            options.Tools.Add(ChatTool.CreateFunctionTool(
                functionName: "search_clients",
                functionDescription: "Busca clientes por nombre o apellido",
                functionParameters: BinaryData.FromString("{\"type\":\"object\",\"properties\":{\"query\":{\"type\":\"string\",\"description\":\"texto\"}}}")
            ));

            options.Tools.Add(ChatTool.CreateFunctionTool(
                functionName: "search_workers",
                functionDescription: "Busca trabajadores por nombre o apellido",
                functionParameters: BinaryData.FromString("{\"type\":\"object\",\"properties\":{\"query\":{\"type\":\"string\",\"description\":\"texto\"}}}")
            ));

            options.Tools.Add(ChatTool.CreateFunctionTool(
                functionName: "search_suppliers",
                functionDescription: "Busca proveedores por razón social",
                functionParameters: BinaryData.FromString("{\"type\":\"object\",\"properties\":{\"query\":{\"type\":\"string\",\"description\":\"texto\"}}}")
            ));

            options.Tools.Add(ChatTool.CreateFunctionTool(
                functionName: "get_purchases_by_supplier",
                functionDescription: "Compras agrupadas por proveedor en un período",
                functionParameters: BinaryData.FromString("{\"type\":\"object\",\"properties\":{\"fechaInicio\":{\"type\":\"string\",\"description\":\"Inicio ISO\"},\"fechaFin\":{\"type\":\"string\",\"description\":\"Fin ISO\"}}}")
            ));

            options.Tools.Add(ChatTool.CreateFunctionTool(
                functionName: "get_audit_logs",
                functionDescription: "Registros de auditoría recientes",
                functionParameters: BinaryData.FromString("{\"type\":\"object\",\"properties\":{\"limit\":{\"type\":[\"integer\",\"string\"],\"description\":\"cantidad\"}}}")
            ));

            options.Tools.Add(ChatTool.CreateFunctionTool(
                functionName: "get_client_purchase_history",
                functionDescription: "Historial de compras de un cliente",
                functionParameters: BinaryData.FromString("{\"type\":\"object\",\"properties\":{\"idCliente\":{\"type\":[\"integer\",\"string\"],\"description\":\"ID\"}}}")
            ));

            options.Tools.Add(ChatTool.CreateFunctionTool(
                functionName: "search_articles_natural",
                functionDescription: "Busca artículos por lenguaje natural (ej: 'gaseosas')",
                functionParameters: BinaryData.FromString("{\"type\":\"object\",\"properties\":{\"query\":{\"type\":\"string\",\"description\":\"descripción\"}}}")
            ));

            options.Tools.Add(ChatTool.CreateFunctionTool(
                functionName: "get_dashboard_insights",
                functionDescription: "Datos del dashboard: ventas, top, stock bajo, clientes"
            ));

            options.Tools.Add(ChatTool.CreateFunctionTool(
                functionName: "update_trabajador",
                functionDescription: "Actualiza trabajador (nombre/apellidos/telefono/direccion/email)",
                functionParameters: BinaryData.FromString("{\"type\":\"object\",\"properties\":{\"id\":{\"type\":[\"integer\",\"string\"],\"description\":\"ID\"},\"campo\":{\"type\":\"string\",\"description\":\"campo\"},\"valor\":{\"type\":\"string\",\"description\":\"valor\"}}}")
            ));

            options.Tools.Add(ChatTool.CreateFunctionTool(
                functionName: "update_cliente",
                functionDescription: "Actualiza cliente (nombre/apellidos/telefono/direccion/email)",
                functionParameters: BinaryData.FromString("{\"type\":\"object\",\"properties\":{\"id\":{\"type\":[\"integer\",\"string\"],\"description\":\"ID\"},\"campo\":{\"type\":\"string\",\"description\":\"campo\"},\"valor\":{\"type\":\"string\",\"description\":\"valor\"}}}")
            ));

            options.Tools.Add(ChatTool.CreateFunctionTool(
                functionName: "update_articulo",
                functionDescription: "Actualiza artículo (nombre/descripcion/codigo)",
                functionParameters: BinaryData.FromString("{\"type\":\"object\",\"properties\":{\"id\":{\"type\":[\"integer\",\"string\"],\"description\":\"ID\"},\"campo\":{\"type\":\"string\",\"description\":\"campo\"},\"valor\":{\"type\":\"string\",\"description\":\"valor\"}}}")
            ));

            options.Tools.Add(ChatTool.CreateFunctionTool(
                functionName: "update_proveedor",
                functionDescription: "Actualiza proveedor (razonSocial/telefono/direccion/email)",
                functionParameters: BinaryData.FromString("{\"type\":\"object\",\"properties\":{\"id\":{\"type\":[\"integer\",\"string\"],\"description\":\"ID\"},\"campo\":{\"type\":\"string\",\"description\":\"campo\"},\"valor\":{\"type\":\"string\",\"description\":\"valor\"}}}")
            ));

            options.Tools.Add(ChatTool.CreateFunctionTool(
                functionName: "add_stock_articulo",
                functionDescription: "Agrega stock a un artículo",
                functionParameters: BinaryData.FromString("{\"type\":\"object\",\"properties\":{\"idArticulo\":{\"type\":[\"integer\",\"string\"],\"description\":\"ID\"},\"cantidad\":{\"type\":[\"integer\",\"string\"],\"description\":\"stock a agregar\"}}}")
            ));
        }

        private async Task<string> ExecuteToolAsync(string functionName, string argumentsJson)
        {
            try
            {
                var fn = functionName.ToLowerInvariant();
                var args = string.IsNullOrEmpty(argumentsJson) || argumentsJson == "{}"
                    ? new JsonElement()
                    : JsonSerializer.Deserialize<JsonElement>(argumentsJson);

                return fn switch
                {
                    "get_dashboard_summary" => await GetDashboardSummaryAsync(),
                    "get_low_stock_articles" => await GetLowStockArticlesAsync(),
                    "get_top_selling_articles" => await GetTopSellingArticlesAsync(args),
                    "get_sales_by_period" => await GetSalesByPeriodAsync(args),
                    "search_articles" => await SearchArticlesAsync(args),
                    "search_clients" => await SearchClientsAsync(args),
                    "search_workers" => await SearchWorkersAsync(args),
                    "search_suppliers" => await SearchSuppliersAsync(args),
                    "get_purchases_by_supplier" => await GetPurchasesBySupplierAsync(args),
                    "get_audit_logs" => await GetAuditLogsAsync(args),
                    "get_client_purchase_history" => await GetClientPurchaseHistoryAsync(args),
                    "search_articles_natural" => await SearchArticlesByNaturalLanguageAsync(args),
                    "get_dashboard_insights" => await GetDashboardInsightsAsync(args),
                    "update_trabajador" => await UpdateTrabajadorAsync(args),
                    "update_cliente" => await UpdateClienteAsync(args),
                    "update_articulo" => await UpdateArticuloAsync(args),
                    "update_proveedor" => await UpdateProveedorAsync(args),
                    "add_stock_articulo" => await AddStockArticuloAsync(args),
                    _ => JsonSerializer.Serialize(new { error = $"Función desconocida: {functionName}" })
                };
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        private static AiDataChange? GetChangeFromToolCall(string functionName)
        {
            var fn = functionName.ToLowerInvariant();
            return fn switch
            {
                "update_trabajador" => new AiDataChange { Entity = "trabajadores", Action = "updated" },
                "update_cliente" => new AiDataChange { Entity = "clientes", Action = "updated" },
                "update_articulo" => new AiDataChange { Entity = "articulos", Action = "updated" },
                "update_proveedor" => new AiDataChange { Entity = "proveedores", Action = "updated" },
                "add_stock_articulo" => new AiDataChange { Entity = "articulos", Action = "stock_added" },
                _ => null
            };
        }

        private async Task<string> GetDashboardSummaryAsync()
        {
            var hoy = DateTime.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            var totalClientes = await _context.Clientes.CountAsync();
            var totalProveedores = await _context.Proveedores.CountAsync();
            var totalArticulos = await _context.Articulos.CountAsync();
            var ventasHoy = await _context.Ventas.CountAsync(v => v.Fecha.Date == hoy);
            var ingresosHoy = await _context.Ingresos.CountAsync(i => i.Fecha.Date == hoy);

            var stockBajo = await _context.DetallesIngreso
                .Where(di => di.StockActual <= 5)
                .GroupBy(di => di.IdArticulo)
                .CountAsync();

            return JsonSerializer.Serialize(new
            {
                totalClientes,
                totalProveedores,
                totalArticulos,
                ventasHoy,
                ingresosHoy,
                articulosStockBajo = stockBajo
            });
        }

        private async Task<string> GetLowStockArticlesAsync()
        {
            var articulos = await _context.DetallesIngreso
                .Where(di => di.StockActual <= 5)
                .GroupBy(di => new { di.IdArticulo, Nombre = di.Articulo!.Nombre })
                .Select(g => new
                {
                    g.Key.IdArticulo,
                    nombre = g.Key.Nombre,
                    stockActual = g.Min(x => x.StockActual)
                })
                .OrderBy(a => a.stockActual)
                .Take(50)
                .ToListAsync();

            return JsonSerializer.Serialize(articulos);
        }

        private async Task<string> GetTopSellingArticlesAsync(JsonElement args)
        {
            var top = TryGetInt(args, "top", 10);
            var inicio = TryGetDate(args, "fechaInicio", DateTime.Today.AddMonths(-3));
            var fin = TryGetDate(args, "fechaFin", DateTime.Today);

            var resultado = await _context.DetallesVenta
                .Where(dv => dv.Venta!.Fecha >= inicio && dv.Venta!.Fecha <= fin)
                .GroupBy(dv => new { dv.DetalleIngreso!.IdArticulo, dv.DetalleIngreso!.Articulo!.Nombre })
                .Select(g => new
                {
                    g.Key.IdArticulo,
                    g.Key.Nombre,
                    totalVendido = g.Sum(x => x.Cantidad),
                    totalIngresos = g.Sum(x => x.Cantidad * x.PrecioVenta - x.Descuento)
                })
                .OrderByDescending(r => r.totalVendido)
                .Take(top)
                .ToListAsync();

            return JsonSerializer.Serialize(resultado);
        }

        private async Task<string> GetSalesByPeriodAsync(JsonElement args)
        {
            var inicio = TryGetDate(args, "fechaInicio", DateTime.Today.AddMonths(-1));
            var fin = TryGetDate(args, "fechaFin", DateTime.Today);
            var agrupacion = TryGetString(args, "agrupacion", "dia");

            var query = await _context.Ventas
                .Where(v => v.Fecha >= inicio && v.Fecha <= fin)
                .Join(_context.DetallesVenta,
                    v => v.IdVenta,
                    dv => dv.IdVenta,
                    (v, dv) => new { v.Fecha, Total = dv.Cantidad * dv.PrecioVenta - dv.Descuento })
                .ToListAsync();

            var resultado = agrupacion switch
            {
                "mes" => query
                    .GroupBy(x => new { x.Fecha.Year, x.Fecha.Month })
                    .Select(g => new { periodo = $"{g.Key.Year}-{g.Key.Month:D2}", total = g.Sum(x => x.Total) })
                    .ToList(),

                "semana" => query
                    .GroupBy(x => new
                    {
                        x.Fecha.Year,
                        Semana = System.Globalization.CultureInfo.InvariantCulture.Calendar
                            .GetWeekOfYear(x.Fecha, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday)
                    })
                    .Select(g => new { periodo = $"{g.Key.Year}-Sem{g.Key.Semana:D2}", total = g.Sum(x => x.Total) })
                    .ToList(),

                _ => query
                    .GroupBy(x => x.Fecha.Date)
                    .Select(g => new { periodo = g.Key.ToString("yyyy-MM-dd"), total = g.Sum(x => x.Total) })
                    .ToList()
            };

            return JsonSerializer.Serialize(resultado);
        }

        private async Task<string> SearchArticlesAsync(JsonElement args)
        {
            var query = TryGetString(args, "query", "");
            if (string.IsNullOrWhiteSpace(query))
                return "[]";

            var lowerQuery = query.ToLower();

            var articulos = await _context.Articulos
                .Where(a => a.Nombre.ToLower().Contains(lowerQuery) || a.Codigo.ToLower().Contains(lowerQuery))
                .Take(10)
                .Select(a => new { a.Id, a.Nombre, a.Codigo })
                .ToListAsync();

            return JsonSerializer.Serialize(articulos);
        }

        private async Task<string> SearchClientsAsync(JsonElement args)
        {
            var query = TryGetString(args, "query", "");
            if (string.IsNullOrWhiteSpace(query))
                return "[]";

            var lowerQuery = query.ToLower();

            var clientes = await _context.Clientes
                .Where(c => c.Nombre.ToLower().Contains(lowerQuery) || (c.Apellidos != null && c.Apellidos.ToLower().Contains(lowerQuery)))
                .Take(10)
                .Select(c => new { c.IdCliente, nombreCompleto = c.Nombre + " " + (c.Apellidos ?? ""), c.NumDocumento })
                .ToListAsync();

            return JsonSerializer.Serialize(clientes);
        }

        private async Task<string> SearchWorkersAsync(JsonElement args)
        {
            var query = TryGetString(args, "query", "");
            if (string.IsNullOrWhiteSpace(query))
                return "[]";

            var lowerQuery = query.ToLower();

            var trabajadores = await _context.Trabajadores
                .Where(t => t.Nombre.ToLower().Contains(lowerQuery) || (t.Apellidos != null && t.Apellidos.ToLower().Contains(lowerQuery)))
                .Take(10)
                .Select(t => new { t.IdTrabajador, nombreCompleto = t.Nombre + " " + (t.Apellidos ?? ""), t.NumDocumento })
                .ToListAsync();

            return JsonSerializer.Serialize(trabajadores);
        }

        private async Task<string> SearchSuppliersAsync(JsonElement args)
        {
            var query = TryGetString(args, "query", "");
            if (string.IsNullOrWhiteSpace(query))
                return "[]";

            var lowerQuery = query.ToLower();

            var proveedores = await _context.Proveedores
                .Where(p => p.RazonSocial.ToLower().Contains(lowerQuery))
                .Take(10)
                .Select(p => new { p.IdProveedor, p.RazonSocial, p.NumDocumento })
                .ToListAsync();

            return JsonSerializer.Serialize(proveedores);
        }

        private async Task<string> GetPurchasesBySupplierAsync(JsonElement args)
        {
            var inicio = TryGetDate(args, "fechaInicio", DateTime.Today.AddMonths(-3));
            var fin = TryGetDate(args, "fechaFin", DateTime.Today);

            var resultado = await _context.Ingresos
                .Where(i => i.Fecha >= inicio && i.Fecha <= fin)
                .GroupBy(i => new { i.IdProveedor, i.Proveedor!.RazonSocial })
                .Select(g => new
                {
                    g.Key.IdProveedor,
                    proveedor = g.Key.RazonSocial,
                    totalCompras = g.Count(),
                    totalGastado = g.Sum(i => i.DetallesIngreso.Sum(d => d.StockInicial * d.PrecioCompra))
                })
                .OrderByDescending(r => r.totalGastado)
                .ToListAsync();

            return JsonSerializer.Serialize(resultado);
        }

        private async Task<string> GetAuditLogsAsync(JsonElement args)
        {
            var limit = TryGetInt(args, "limit", 10);

            var logs = await _context.AuditLogs
                .OrderByDescending(al => al.Timestamp)
                .Take(limit)
                .Select(al => new
                {
                    al.EntityName,
                    al.EntityId,
                    al.Action,
                    al.UserName,
                    al.Timestamp
                })
                .ToListAsync();

            return JsonSerializer.Serialize(logs);
        }

        private async Task<string> GetClientPurchaseHistoryAsync(JsonElement args)
        {
            var idCliente = TryGetInt(args, "idCliente", 0);
            if (idCliente == 0)
                return JsonSerializer.Serialize(new { error = "idCliente requerido" });

            var cliente = await _context.Clientes.FindAsync(idCliente);
            if (cliente == null)
                return JsonSerializer.Serialize(new { error = "Cliente no encontrado" });

            var ventas = await _context.Ventas
                .Where(v => v.IdCliente == idCliente)
                .OrderByDescending(v => v.Fecha)
                .Take(5)
                .Select(v => new
                {
                    v.IdVenta,
                    v.Fecha,
                    Total = v.DetallesVenta.Sum(d => d.Cantidad * d.PrecioVenta - d.Descuento),
                    Articulos = v.DetallesVenta.Sum(d => d.Cantidad)
                })
                .ToListAsync();

            var totalGastado = await _context.DetallesVenta
                .Where(dv => dv.Venta!.IdCliente == idCliente)
                .SumAsync(dv => dv.Cantidad * dv.PrecioVenta - dv.Descuento);

            var topArticulos = await _context.DetallesVenta
                .Where(dv => dv.Venta!.IdCliente == idCliente)
                .GroupBy(dv => new { dv.DetalleIngreso!.Articulo!.Nombre })
                .Select(g => new { articulo = g.Key.Nombre, total = g.Sum(x => x.Cantidad) })
                .OrderByDescending(x => x.total)
                .Take(5)
                .ToListAsync();

            return JsonSerializer.Serialize(new
            {
                cliente = $"{cliente.Nombre} {cliente.Apellidos}",
                documento = cliente.NumDocumento,
                totalCompras = ventas.Count,
                totalGastado,
                ultimasVentas = ventas,
                articulosFrecuentes = topArticulos
            });
        }

        private async Task<string> SearchArticlesByNaturalLanguageAsync(JsonElement args)
        {
            var query = TryGetString(args, "query", "");
            if (string.IsNullOrWhiteSpace(query))
                return "[]";

            var lowerQuery = query.ToLower();
            var keywords = lowerQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var articulosQuery = _context.Articulos.AsQueryable();

            foreach (var kw in keywords)
            {
                if (kw.Length < 2) continue;
                var k = kw;
                articulosQuery = articulosQuery.Where(a =>
                    a.Nombre.ToLower().Contains(k) ||
                    (a.Descripcion != null && a.Descripcion.ToLower().Contains(k)) ||
                    (a.Categoria != null && a.Categoria.Nombre.ToLower().Contains(k)) ||
                    (a.Presentacion != null && a.Presentacion.Nombre.ToLower().Contains(k)));
            }

            var results = await articulosQuery
                .Select(a => new
                {
                    a.Id,
                    a.Nombre,
                    a.Codigo,
                    Categoria = a.Categoria != null ? a.Categoria.Nombre : null,
                    Presentacion = a.Presentacion != null ? a.Presentacion.Nombre : null
                })
                .Take(10)
                .ToListAsync();

            if (results.Count == 0)
            {
                results = await _context.Articulos
                    .Where(a => a.Nombre.ToLower().Contains(lowerQuery) || a.Codigo.ToLower().Contains(lowerQuery))
                    .Take(10)
                    .Select(a => new
                    {
                        a.Id,
                        a.Nombre,
                        a.Codigo,
                        Categoria = (string?)null,
                        Presentacion = (string?)null
                    })
                    .ToListAsync();
            }

            return JsonSerializer.Serialize(results);
        }

        private async Task<string> GetDashboardInsightsAsync(JsonElement args)
        {
            var result = await GetDashboardSummaryAsync();
            return result;
        }

        // ---- HERRAMIENTAS DE ESCRITURA ----

        private async Task<string> UpdateTrabajadorAsync(JsonElement args)
        {
            var id = TryGetInt(args, "id", 0);
            var campo = TryGetString(args, "campo", "").ToLower();
            var valor = TryGetString(args, "valor", "");

            var entity = await _context.Trabajadores.FindAsync(id);
            if (entity == null)
                return JsonSerializer.Serialize(new { error = "Trabajador no encontrado" });

            switch (campo)
            {
                case "nombre": entity.Nombre = valor; break;
                case "apellidos": entity.Apellidos = valor; break;
                case "telefono": entity.Telefono = valor; break;
                case "direccion": entity.Direccion = valor; break;
                case "email": entity.Email = valor; break;
                default: return JsonSerializer.Serialize(new { error = $"Campo '{campo}' no permitido. Campos: nombre, apellidos, telefono, direccion, email" });
            }

            await _context.SaveChangesAsync();
            return JsonSerializer.Serialize(new { success = true, message = $"Trabajador actualizado: {campo} = {valor}" });
        }

        private async Task<string> UpdateClienteAsync(JsonElement args)
        {
            var id = TryGetInt(args, "id", 0);
            var campo = TryGetString(args, "campo", "").ToLower();
            var valor = TryGetString(args, "valor", "");

            var entity = await _context.Clientes.FindAsync(id);
            if (entity == null)
                return JsonSerializer.Serialize(new { error = "Cliente no encontrado" });

            switch (campo)
            {
                case "nombre": entity.Nombre = valor; break;
                case "apellidos": entity.Apellidos = valor; break;
                case "telefono": entity.Telefono = valor; break;
                case "direccion": entity.Direccion = valor; break;
                case "email": entity.Email = valor; break;
                default: return JsonSerializer.Serialize(new { error = $"Campo '{campo}' no permitido. Campos: nombre, apellidos, telefono, direccion, email" });
            }

            await _context.SaveChangesAsync();
            return JsonSerializer.Serialize(new { success = true, message = $"Cliente actualizado: {campo} = {valor}" });
        }

        private async Task<string> UpdateArticuloAsync(JsonElement args)
        {
            var id = TryGetInt(args, "id", 0);
            var campo = TryGetString(args, "campo", "").ToLower();
            var valor = TryGetString(args, "valor", "");

            var entity = await _context.Articulos.FindAsync(id);
            if (entity == null)
                return JsonSerializer.Serialize(new { error = "Artículo no encontrado" });

            switch (campo)
            {
                case "nombre": entity.Nombre = valor; break;
                case "descripcion": entity.Descripcion = valor; break;
                case "codigo": entity.Codigo = valor; break;
                default: return JsonSerializer.Serialize(new { error = $"Campo '{campo}' no permitido. Campos: nombre, descripcion, codigo" });
            }

            await _context.SaveChangesAsync();
            return JsonSerializer.Serialize(new { success = true, message = $"Artículo actualizado: {campo} = {valor}" });
        }

        private async Task<string> UpdateProveedorAsync(JsonElement args)
        {
            var id = TryGetInt(args, "id", 0);
            var campo = TryGetString(args, "campo", "").ToLower();
            var valor = TryGetString(args, "valor", "");

            var entity = await _context.Proveedores.FindAsync(id);
            if (entity == null)
                return JsonSerializer.Serialize(new { error = "Proveedor no encontrado" });

            switch (campo)
            {
                case "razonsocial": entity.RazonSocial = valor; break;
                case "telefono": entity.Telefono = valor; break;
                case "direccion": entity.Direccion = valor; break;
                case "email": entity.Email = valor; break;
                default: return JsonSerializer.Serialize(new { error = $"Campo '{campo}' no permitido. Campos: razonSocial, telefono, direccion, email" });
            }

            await _context.SaveChangesAsync();
            return JsonSerializer.Serialize(new { success = true, message = $"Proveedor actualizado: {campo} = {valor}" });
        }

        private async Task<string> AddStockArticuloAsync(JsonElement args)
        {
            var idArticulo = TryGetInt(args, "idArticulo", 0);
            var cantidad = TryGetInt(args, "cantidad", 0);

            if (idArticulo == 0 || cantidad <= 0)
                return JsonSerializer.Serialize(new { error = "idArticulo y cantidad válida son requeridos" });

            var articulo = await _context.Articulos.FindAsync(idArticulo);
            if (articulo == null)
                return JsonSerializer.Serialize(new { error = "Artículo no encontrado" });

            var batch = await _context.DetallesIngreso
                .Where(d => d.IdArticulo == idArticulo)
                .OrderByDescending(d => d.IdDetalleIngreso)
                .FirstOrDefaultAsync();

            if (batch != null)
            {
                batch.StockActual += cantidad;
                batch.StockInicial += cantidad;
                await _context.SaveChangesAsync();
                return JsonSerializer.Serialize(new { success = true, message = $"Stock agregado: +{cantidad} a '{articulo.Nombre}'. Stock actual: {batch.StockActual}" });
            }

            return JsonSerializer.Serialize(new { error = $"No hay lotes de ingreso para '{articulo.Nombre}'. Creá un ingreso primero." });
        }

        private async Task<string> GetLowStockContextAsync()
        {
            var hoy = DateTime.Today;
            var bajoStock = await _context.DetallesIngreso
                .Where(di => di.StockActual <= 10)
                .GroupBy(di => new { di.IdArticulo, Nombre = di.Articulo!.Nombre })
                .Select(g => new { g.Key.Nombre, stockTotal = g.Sum(x => x.StockActual) })
                .OrderBy(x => x.stockTotal)
                .ToListAsync();

            return JsonSerializer.Serialize(new { fecha = hoy.ToString("yyyy-MM-dd"), articulosStockBajo = bajoStock });
        }

        private async Task<string> GetTopSellingContextAsync()
        {
            var inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            var top = await _context.DetallesVenta
                .Where(dv => dv.Venta!.Fecha >= inicioMes)
                .GroupBy(dv => new { dv.DetalleIngreso!.IdArticulo, dv.DetalleIngreso!.Articulo!.Nombre })
                .Select(g => new { g.Key.Nombre, total = g.Sum(x => x.Cantidad) })
                .OrderByDescending(x => x.total)
                .Take(10)
                .ToListAsync();

            return JsonSerializer.Serialize(new { periodo = $"Desde {inicioMes:yyyy-MM-dd}", topArticulos = top });
        }

        private async Task<string> GetAnomaliesContextAsync()
        {
            var hoy = DateTime.Today;
            var ayer = hoy.AddDays(-1);

            var ventasAyer = await _context.DetallesVenta
                .Where(dv => dv.Venta!.Fecha.Date == ayer)
                .SumAsync(dv => (long)dv.Cantidad);

            var ventasDiarias = await _context.DetallesVenta
                .Where(dv => dv.Venta!.Fecha >= hoy.AddMonths(-1))
                .GroupBy(dv => dv.Venta!.Fecha.Date)
                .Select(g => g.Sum(x => (long)x.Cantidad))
                .ToListAsync();

            var promedioDiario = ventasDiarias.Count > 0 ? ventasDiarias.Average() : 0;

            return JsonSerializer.Serialize(new
            {
                fecha = hoy.ToString("yyyy-MM-dd"),
                ventasAyer,
                promedioDiarioUltimoMes = Math.Round(promedioDiario, 2)
            });
        }

        private async Task<string> GetDashboardContextAsync()
        {
            return await GetDashboardSummaryAsync();
        }

        private async Task<string> GetFullContextAsync(string? period = null)
        {
            var hoy = DateTime.Today;
            var inicio = period?.ToLower() switch
            {
                "semana" => hoy.AddDays(-7),
                "mes" => hoy.AddMonths(-1),
                "trimestre" => hoy.AddMonths(-3),
                "año" => hoy.AddYears(-1),
                _ => hoy.AddMonths(-1)
            };

            var totalClientes = await _context.Clientes.CountAsync();
            var totalProveedores = await _context.Proveedores.CountAsync();
            var totalArticulos = await _context.Articulos.CountAsync();
            var totalVentasPeriodo = await _context.Ventas.CountAsync(v => v.Fecha >= inicio);
            var totalVentasMonto = await _context.DetallesVenta
                .Where(dv => dv.Venta!.Fecha >= inicio)
                .SumAsync(dv => dv.Cantidad * dv.PrecioVenta - dv.Descuento);

            var topArticulos = await _context.DetallesVenta
                .Where(dv => dv.Venta!.Fecha >= inicio)
                .GroupBy(dv => dv.DetalleIngreso!.Articulo!.Nombre)
                .Select(g => new { articulo = g.Key, total = g.Sum(x => x.Cantidad) })
                .OrderByDescending(x => x.total)
                .Take(5)
                .ToListAsync();

            return JsonSerializer.Serialize(new
            {
                periodo = $"{inicio:yyyy-MM-dd} a {hoy:yyyy-MM-dd}",
                totalClientes,
                totalProveedores,
                totalArticulos,
                totalVentasPeriodo,
                totalVentasMonto,
                topArticulos
            });
        }

        private static List<AiSuggestedAction> ExtractActionsFromResponse(string response)
        {
            var actions = new List<AiSuggestedAction>();

            if (response.Contains("stock bajo", StringComparison.OrdinalIgnoreCase) ||
                response.Contains("bajo stock", StringComparison.OrdinalIgnoreCase))
            {
                actions.Add(new AiSuggestedAction
                {
                    Label = "Ver artículos con stock bajo",
                    Route = "/articulos"
                });
            }

            if ((response.Contains("venta", StringComparison.OrdinalIgnoreCase) ||
                 response.Contains("reporte", StringComparison.OrdinalIgnoreCase) ||
                 response.Contains("resumen", StringComparison.OrdinalIgnoreCase)))
            {
                actions.Add(new AiSuggestedAction
                {
                    Label = "Ver reportes de ventas",
                    Route = "/reportes"
                });
            }

            return actions;
        }

        private static string TryGetString(JsonElement el, string key, string defaultValue)
        {
            if (el.ValueKind == JsonValueKind.Undefined) return defaultValue;
            return el.TryGetProperty(key, out var prop) && prop.ValueKind == JsonValueKind.String
                ? prop.GetString() ?? defaultValue
                : defaultValue;
        }

        private static int TryGetInt(JsonElement el, string key, int defaultValue)
        {
            if (el.ValueKind == JsonValueKind.Undefined) return defaultValue;
            if (el.TryGetProperty(key, out var prop))
            {
                if (prop.ValueKind == JsonValueKind.Number)
                    return prop.GetInt32();
                if (prop.ValueKind == JsonValueKind.String && int.TryParse(prop.GetString(), out var parsed))
                    return parsed;
            }
            return defaultValue;
        }

        private static DateTime TryGetDate(JsonElement el, string key, DateTime defaultValue)
        {
            if (el.ValueKind == JsonValueKind.Undefined) return defaultValue;
            if (el.TryGetProperty(key, out var prop) && prop.ValueKind == JsonValueKind.String)
            {
                var str = prop.GetString();
                if (!string.IsNullOrEmpty(str) && DateTime.TryParse(str, out var result))
                    return result;
            }
            return defaultValue;
        }
    }
}
