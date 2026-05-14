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
        // 1. Recopilar datos en vivo de la base de datos (Enfoque RAG Simple)
        var totalProducts = await _context.Productos.CountAsync();
        var totalStock = await _context.Productos.SumAsync(p => (int?)p.stock) ?? 0;
        var totalUsers = await _context.Usuarios.CountAsync();
        var outOfStockItems = await _context.Productos.Where(p => p.stock == 0).Select(p => p.nombre).ToListAsync();
        
        // Detalle de inventario para que sepa exactamente qué hay
        var productList = await _context.Productos
            .Where(p => p.stock > 0)
            .Select(p => $"- {p.nombre} (Stock: {p.stock})")
            .ToListAsync();
        
        // ---- NUEVO: Métricas de Ventas y Facturación ----
        var totalOrders = await _context.Pedidos.CountAsync();
        var totalRevenue = await _context.Pedidos.Where(p => p.id_estado_pedido == 2).SumAsync(p => (decimal?)p.total) ?? 0;
        
        var recentInvoices = await _context.Facturas
            .Include(f => f.Pedido)
            .OrderByDescending(f => f.fecha_factura)
            .Take(5)
            .Select(f => $"Factura #{f.id_factura} (Pedido #{f.id_pedido}) - {f.fecha_factura:yyyy-MM-dd}: ${f.total:N0}")
            .ToListAsync();

        string outOfStockStr = outOfStockItems.Any() ? string.Join(", ", outOfStockItems) : "Ninguno";
        string recentInvoicesStr = recentInvoices.Any() ? string.Join("\n", recentInvoices) : "Sin facturas recientes";
        string inventoryStr = productList.Any() ? string.Join("\n", productList) : "Sin inventario";

        // 2. Construir el Prompt del Sistema (System Prompt) con el contexto en vivo
        string systemPrompt = $@"
Eres el Asistente Financiero y Operativo de lujo (AI) exclusivo para el Administrador de la marca 'LuxuryCo'.
Hablas de manera profesional, estratégica y directa. 

REGLAS ESTRICTAS E INQUEBRANTABLES:
1. NUNCA inventes números, ventas, facturas ni productos.
2. USA EXCLUSIVAMENTE los datos proporcionados abajo. Si te piden un dato que no está aquí, responde que no tienes acceso a esa información específica en este momento.

DATOS EN TIEMPO REAL DE LA EMPRESA:
- Total de productos en catálogo: {totalProducts}
- Unidades totales de ropa/accesorios en stock: {totalStock}
- Total de clientes registrados: {totalUsers}
- Productos agotados: {outOfStockStr}

INVENTARIO ACTUAL:
{inventoryStr}

MÉTRICAS DE VENTAS Y FACTURACIÓN:
- Total de Pedidos Realizados: {totalOrders}
- Ingresos Totales (Ventas Aprobadas): ${totalRevenue:N0}
- Últimas 5 Facturas Generadas:
{recentInvoicesStr}
";

        // 3. Preparar el cuerpo de la petición para Groq
        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userMessage }
            },
            temperature = 0.1 // <-- Temperatura casi a cero para evitar alucinaciones y forzar precisión
        };

        var jsonBody = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        // 4. Llamar a la API de Groq
        // Hacemos la petición HTTP POST al modelo seleccionado.
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

    public async Task<StylistResponse> GetClientStylistAdviceAsync(string userMessage, string sessionId, int? userId = null)
    {
        // 1. Guardar el mensaje del usuario en la base de datos
        // Esto crea el primer registro de la interacción actual en el historial.
        _context.HistorialesChatAi.Add(new LuxuryCo.Database.Models.HistorialChatAi
        {
            id_usuario = userId, session_id = sessionId, role = "user",
            content = userMessage, fecha_creacion = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // 2. Cargar el historial reciente (los últimos 8 mensajes para dar contexto a la IA)
        // Buscamos por ID de usuario (si está logueado) o por el Session ID del navegador (si es invitado).
        var recentHistory = await _context.HistorialesChatAi
            .Where(h => (userId != null && h.id_usuario == userId) || (userId == null && h.session_id == sessionId))
            .OrderByDescending(h => h.fecha_creacion)
            .Take(8)
            .ToListAsync();
        recentHistory.Reverse();

        // 3. Cargar los productos activos CON su imagen principal
        // Solo traemos productos que tienen stock y están marcados como activos para que la IA los recomiende.
        var activeProducts = await _context.Productos
            .Where(p => p.activo && p.stock > 0)
            .Select(p => new
            {
                p.id_producto, p.nombre, p.precio, p.seccion,
                imagen = p.Imagenes.Where(i => i.principal).Select(i => i.url_imagen).FirstOrDefault()
            })
            .ToListAsync();

        // Crear un catálogo compacto para el prompt (solo id, nombre, precio y sección para no exceder el límite de tokens)
        var catalogForPrompt = activeProducts.Select(p => new { p.id_producto, p.nombre, p.precio, p.seccion });
        var productsJson = System.Text.Json.JsonSerializer.Serialize(catalogForPrompt);

        // 4. Prompt del Sistema: instruir a la IA para que etiquete los productos recomendados
        // Se le dan reglas estrictas sobre qué decir y cómo usar las etiquetas [PRODUCTO:id].
        var systemPrompt = $@"Eres un Asesor de Estilo exclusivo y 'Personal Shopper' de LuxuryCo.
Tono: amable, sofisticado, breve.
REGLA 1: Solo recomienda productos del catálogo JSON. Si no existe lo que piden, dilo amablemente.
REGLA 2: Cuando recomiendes 1 o más productos, incluye la etiqueta [PRODUCTO:id_producto] exactamente así (reemplaza id_producto por el número). Ejemplo: [PRODUCTO:3]
REGLA 3: Máximo 2 productos recomendados por respuesta.
REGLA 4: Nunca menciones costos internos, stock ni datos técnicos.

CATÁLOGO:
{productsJson}";

        // 5. Construir el arreglo de mensajes (Historial)
        // Se inyecta primero el System Prompt, y luego todos los mensajes anteriores de la base de datos.
        var messages = new List<object> { new { role = "system", content = systemPrompt } };
        foreach (var msg in recentHistory)
            messages.Add(new { role = msg.role, content = msg.content });

        var requestBody = new { model = _model, messages = messages.ToArray(), temperature = 0.65 };
        var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var httpResponse = await _httpClient.PostAsync("chat/completions", content);
        var responseBody = await httpResponse.Content.ReadAsStringAsync();

        if (!httpResponse.IsSuccessStatusCode)
            throw new Exception($"Error de la IA: {responseBody}");

        var responseData = System.Text.Json.JsonSerializer.Deserialize<GroqResponse>(
            responseBody,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var aiReply = responseData?.choices?[0]?.message?.content ?? "Sin respuesta.";

        // 6. Extraer las etiquetas [PRODUCTO:id] de la respuesta de la IA usando Expresiones Regulares
        // Esto nos permite saber exactamente qué productos recomendó la IA.
        var cards = new List<ProductCard>();
        var tagPattern = new System.Text.RegularExpressions.Regex(@"\[PRODUCTO:(\d+)\]");
        var matches = tagPattern.Matches(aiReply);

        var mentionedIds = matches.Cast<System.Text.RegularExpressions.Match>()
            .Select(m => int.Parse(m.Groups[1].Value))
            .Distinct().ToList();

        foreach (var id in mentionedIds)
        {
            var p = activeProducts.FirstOrDefault(x => x.id_producto == id);
            if (p != null)
            {
                cards.Add(new ProductCard
                {
                    Id = p.id_producto,
                    Nombre = p.nombre,
                    Precio = p.precio,
                    Seccion = p.seccion ?? "",
                    Imagen = p.imagen ?? "/img/placeholder.png",
                    Url = $"/Catalogo/Detalle/{p.id_producto}"
                });
            }
        }

        // 7. Limpiar las etiquetas del texto que se mostrará al usuario
        // Removemos los "[PRODUCTO:id]" para que el cliente solo lea texto natural.
        var cleanReply = tagPattern.Replace(aiReply, "").Trim();

        // 8. Guardar la respuesta de la IA en la Base de Datos
        // Completamos el ciclo guardando lo que dijo la IA en el historial.
        _context.HistorialesChatAi.Add(new LuxuryCo.Database.Models.HistorialChatAi
        {
            id_usuario = userId, session_id = sessionId, role = "assistant",
            content = aiReply, fecha_creacion = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        return new StylistResponse { Reply = cleanReply, Cards = cards };
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
