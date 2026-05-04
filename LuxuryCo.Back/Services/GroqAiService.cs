using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LuxuryCo.Database.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LuxuryCo.Back.Services;

public class GroqAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly LuxuryCoDbContext _context;

    public GroqAiService(HttpClient httpClient, IConfiguration config, LuxuryCoDbContext context)
    {
        _httpClient = httpClient;
        _context = context;
        _apiKey = config["Groq:ApiKey"] ?? throw new ArgumentNullException("Groq API Key is missing");
        _model = config["Groq:Model"] ?? "llama3-8b-8192";
        _httpClient.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<string> GetAdminBusinessAdviceAsync(string userMessage)
    {
        // 1. Gather live data from the database (Simple RAG approach)
        var totalProducts = await _context.Productos.CountAsync();
        var totalStock = await _context.Productos.SumAsync(p => (int?)p.stock) ?? 0;
        var totalUsers = await _context.Usuarios.CountAsync();
        var outOfStockItems = await _context.Productos.Where(p => p.stock == 0).Select(p => p.nombre).ToListAsync();

        string outOfStockStr = outOfStockItems.Any() ? string.Join(", ", outOfStockItems) : "Ninguno";

        // 2. Build the System Prompt with the live context
        string systemPrompt = $@"
Eres el Asistente Financiero y Operativo de lujo (AI) exclusivo para el Administrador de la marca 'LuxuryCo'.
Hablas de manera profesional, estratégica y directa. Estás aquí para ayudar a tomar decisiones de negocio, inventario y ventas.
NUNCA interactúas como un asistente genérico, eres parte del equipo directivo de LuxuryCo.

DATOS EN TIEMPO REAL DE LA EMPRESA:
- Total de productos en catálogo: {totalProducts}
- Unidades totales en stock: {totalStock}
- Total de usuarios registrados: {totalUsers}
- Productos agotados (sin stock): {outOfStockStr}

Si te preguntan sobre datos o ventas, usa esta información para dar tu análisis y recomendar estrategias (ej: reabastecimiento, descuentos, marketing).
";

        // 3. Prepare the request body for Groq (OpenAI compatible format)
        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userMessage }
            },
            temperature = 0.5
        };

        var jsonBody = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        // 4. Call Groq API
        var response = await _httpClient.PostAsync("chat/completions", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error de la IA: {error}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResponse);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var responseData = System.Text.Json.JsonSerializer.Deserialize<GroqResponse>(responseString, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return responseData?.choices?[0]?.message?.content ?? "No hay respuesta de Groq.";
    }

    public async Task<string> GetClientStylistAdviceAsync(string userMessage, string sessionId, int? userId = null)
    {
        // 1. Guardar el mensaje del usuario en la Base de Datos
        var userDbMsg = new LuxuryCo.Database.Models.HistorialChatAi
        {
            id_usuario = userId,
            session_id = sessionId,
            role = "user",
            content = userMessage,
            fecha_creacion = DateTime.UtcNow
        };
        _context.HistorialesChatAi.Add(userDbMsg);
        await _context.SaveChangesAsync();

        // 2. Obtener historial reciente de este usuario o sesión (Últimos 10 mensajes para dar contexto)
        var recentHistory = await _context.HistorialesChatAi
            .Where(h => (userId != null && h.id_usuario == userId) || (userId == null && h.session_id == sessionId))
            .OrderByDescending(h => h.fecha_creacion)
            .Take(10)
            .ToListAsync();
        
        recentHistory.Reverse(); // Invertir para orden cronológico

        // 3. Obtener productos activos
        var activeProducts = await _context.Productos
            .Where(p => p.activo && p.stock > 0)
            .Select(p => new { p.nombre, p.precio, p.seccion })
            .ToListAsync();

        var productsJson = System.Text.Json.JsonSerializer.Serialize(activeProducts);

        // 4. System prompt
        var systemPrompt = $@"Eres un Asesor de Estilo exclusivo y 'Personal Shopper' de la tienda LuxuryCo.
Tu tono debe ser amable, sofisticado y siempre dispuesto a ayudar a encontrar el outfit perfecto.
REGLA CRÍTICA 1: SOLO PUEDES RECOMENDAR PRODUCTOS DE ESTE CATÁLOGO JSON EXACTO. Si el cliente pide algo que no está, di amablemente que no lo tenemos por el momento.
REGLA CRÍTICA 2: NUNCA menciones temas técnicos, costos, inventario oculto ni actúes como administrador.
REGLA CRÍTICA 3: Sé breve. Da respuestas cortas, directas y elegantes (máximo 2 párrafos cortos).

CATÁLOGO ACTUAL (JSON):
{productsJson}";

        // 5. Preparar mensajes para Groq
        var messages = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        foreach (var msg in recentHistory)
        {
            messages.Add(new { role = msg.role, content = msg.content });
        }

        var requestBody = new
        {
            model = _model,
            messages = messages.ToArray(),
            temperature = 0.6
        };

        var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.PostAsync("chat/completions", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error de la IA: {error}");
        }

        var responseString = await response.Content.ReadAsStringAsync();
        var responseData = System.Text.Json.JsonSerializer.Deserialize<GroqResponse>(responseString, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        var aiReply = responseData?.choices?[0]?.message?.content ?? "No hay respuesta de Groq.";

        // 6. Guardar la respuesta de la IA en la Base de Datos
        var aiDbMsg = new LuxuryCo.Database.Models.HistorialChatAi
        {
            id_usuario = userId,
            session_id = sessionId,
            role = "assistant",
            content = aiReply,
            fecha_creacion = DateTime.UtcNow
        };
        _context.HistorialesChatAi.Add(aiDbMsg);
        await _context.SaveChangesAsync();

        return aiReply;
    }
}

public class GroqResponse
{
    public List<GroqChoice>? choices { get; set; }
}

public class GroqChoice
{
    public GroqMessage? message { get; set; }
}

public class GroqMessage
{
    public string? content { get; set; }
}
