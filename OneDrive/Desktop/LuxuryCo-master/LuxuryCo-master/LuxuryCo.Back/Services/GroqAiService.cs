using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LuxuryCo.Database.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LuxuryCo.Back.Services;

public partial class GroqAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly LuxuryCoDbContext _context;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public GroqAiService(HttpClient httpClient, IConfiguration config, LuxuryCoDbContext context)
    {
        _httpClient = httpClient;
        _context = context;
        _apiKey = config["Groq:ApiKey"] ?? throw new ArgumentException("Groq API Key is missing");
        _model = config["Groq:Model"] ?? "llama3-8b-8192";
        _httpClient.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<string> GetAdminBusinessAdviceAsync(string userMessage, int adminUserId)
    {
        // [PASO 1: MEMORIA DE ENTRADA]
        // Guardamos la pregunta del administrador en la base de datos para que la IA la recuerde en futuros mensajes.
        // Se asocia al ID del admin y a una sesión fija "ADMIN_CONSOLE".
        _context.HistorialesChatAi.Add(new LuxuryCo.Database.Models.HistorialChatAi
        {
            id_usuario = adminUserId, session_id = "ADMIN_CONSOLE", role = "user",
            content = userMessage, fecha_creacion = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // [PASO 2: RAG (Retrieval-Augmented Generation) - RECOPILACIÓN DE DATOS]
        // La IA no tiene acceso a internet ni a nuestra base de datos por sí sola.
        // Nosotros consultamos los datos financieros y de stock en tiempo real usando Entity Framework y se los "inyectamos".
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

        // ---- NUEVO: Proveedores ----
        var suppliersList = await _context.Proveedores
            .Select(p => $"- {p.nombre} (Contacto: {p.contacto}, Tel: {p.telefono})")
            .ToListAsync();

        string outOfStockStr = outOfStockItems.Count > 0 ? string.Join(", ", outOfStockItems) : "Ninguno";
        string recentInvoicesStr = recentInvoices.Count > 0 ? string.Join("\n", recentInvoices) : "Sin facturas recientes";
        string inventoryStr = productList.Count > 0 ? string.Join("\n", productList) : "Sin inventario";
        string suppliersStr = suppliersList.Count > 0 ? string.Join("\n", suppliersList) : "Sin proveedores registrados";

        // [PASO 3: INGENIERÍA DE PROMPTS (PROMPT ENGINEERING)]
        string systemPrompt = $@"
Eres el Asistente Financiero y Operativo de lujo (AI) exclusivo para el Administrador de la marca 'LuxuryCo'.
Hablas de manera profesional, estratégica y directa. 

REGLAS ESTRICTAS E INQUEBRANTABLES:
1. NUNCA inventes números internos (ventas, stock, facturas, clientes). Para datos internos, usa EXCLUSIVAMENTE los datos proporcionados abajo.
2. TIENES PERMITIDO Y SE ESPERA que uses tu conocimiento general sobre el mercado global para:
   - Comparar precios de la competencia (otras tiendas de moda/streetwear).
   - Recomendar nuevos proveedores de la web.
   - Dar análisis comparativo de la industria.

DATOS EN TIEMPO REAL DE LA EMPRESA:
- Total de productos en catálogo: {totalProducts}
- Unidades totales de ropa/accesorios en stock: {totalStock}
- Total de clientes registrados: {totalUsers}
- Productos agotados: {outOfStockStr}

INVENTARIO ACTUAL:
{inventoryStr}

PROVEEDORES ACTUALES:
{suppliersStr}

MÉTRICAS DE VENTAS Y FACTURACIÓN:
- Total de Pedidos Realizados: {totalOrders}
- Ingresos Totales (Ventas Aprobadas): ${totalRevenue:N0}
- Últimas 5 Facturas Generadas:
{recentInvoicesStr}
";

        // [PASO 4: RECUPERACIÓN DE MEMORIA (SHORT-TERM MEMORY)]
        // Extraemos los últimos 8 mensajes (para no exceder el límite de tokens de Groq) 
        // y filtramos para que solo recuerde cosas de los últimos 3 días (evitando guardar contexto obsoleto).
        var limitDate = DateTime.UtcNow.AddDays(-3);
        var recentHistory = await _context.HistorialesChatAi
            .Where(h => h.id_usuario == adminUserId && h.session_id == "ADMIN_CONSOLE" && h.fecha_creacion >= limitDate)
            .OrderByDescending(h => h.fecha_creacion)
            .Take(8)
            .ToListAsync();
        recentHistory.Reverse(); // Invertimos para que queden en orden cronológico (del más viejo al más nuevo)

        // Ensamblamos el arreglo final de mensajes: [1] Instrucciones Maestras -> [2..N] Historial de Chat
        var messages = new List<object> { new { role = "system", content = systemPrompt } };
        foreach (var msg in recentHistory)
            messages.Add(new { msg.role, msg.content });

        // [PASO 5: PETICIÓN A LA API DE GROQ]
        var requestBody = new
        {
            model = _model,
            messages = messages.ToArray(),
            temperature = 0.5 // <-- Aumentado a 0.5 para permitir análisis comparativo y recomendaciones del mercado.
        };

        var jsonBody = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        // Ejecutamos el request HTTP al modelo en la nube
        var response = await _httpClient.PostAsync("chat/completions", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error de la IA: {error}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResponse);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var responseData = System.Text.Json.JsonSerializer.Deserialize<GroqResponse>(responseString, _jsonOptions);

        var aiReply = responseData?.Choices?[0]?.Message?.Content ?? "No hay respuesta de Groq.";

        // Guardar respuesta de la IA
        _context.HistorialesChatAi.Add(new LuxuryCo.Database.Models.HistorialChatAi
        {
            id_usuario = adminUserId, session_id = "ADMIN_CONSOLE", role = "assistant",
            content = aiReply, fecha_creacion = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        return aiReply;
    }

    public async Task<StylistResponse> GetClientStylistAdviceAsync(
        string userMessage,
        string sessionId,
        int? userId = null,
        List<ChatHistoryEntry>? history = null,
        int? lastProductId = null)
    {
        // 1. Guardar el mensaje del usuario en la base de datos
        // Esto crea el primer registro de la interacción actual en el historial.
        _context.HistorialesChatAi.Add(new LuxuryCo.Database.Models.HistorialChatAi
        {
            id_usuario = userId, session_id = sessionId, role = "user",
            content = userMessage, fecha_creacion = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // [PASO 2: RECUPERAR CONTEXTO DEL CLIENTE (MEMORIA DE 3 DÍAS)]
        var limitDate = DateTime.UtcNow.AddDays(-3);
        var recentHistory = await _context.HistorialesChatAi
            .Where(h => ((userId != null && h.id_usuario == userId) || (userId == null && h.session_id == sessionId)) 
                        && h.fecha_creacion >= limitDate)
            .OrderByDescending(h => h.fecha_creacion)
            .Take(8)
            .ToListAsync();
        recentHistory.Reverse();

        // [PASO 3: CATÁLOGO EN VIVO (RAG PARA EL ESTILISTA)]
        // Traemos solo los productos que están a la venta. 
        // También incluimos la URL de la imagen principal para poder dibujar la Tarjeta Visual si la IA recomienda el producto.
        var activeProducts = await _context.Productos
            .Where(p => p.activo && p.stock > 0)
            .Select(p => new
            {
                p.id_producto, p.nombre, p.precio, p.seccion,
                imagen = p.Imagenes.Where(i => i.principal).Select(i => i.url_imagen).FirstOrDefault()
            })
            .ToListAsync();

        // [OPTIMIZACIÓN DE TOKENS]
        // Creamos un objeto JSON minimalista. A la IA no le importan las imágenes, 
        // le importa saber el ID y el Nombre para armar la etiqueta [PRODUCTO:id].
        var catalogForPrompt = activeProducts.Select(p => new { p.id_producto, p.nombre, p.precio, p.seccion });
        var productsJson = System.Text.Json.JsonSerializer.Serialize(catalogForPrompt);

        // [PASO 4: PROMPT ENGINEERING DE COMPORTAMIENTO]
        // Le indicamos estrictamente que la forma de recomendar ropa es devolviendo un string como "[PRODUCTO:15]"
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
            messages.Add(new { msg.role, msg.content });

        var requestBody = new { model = _model, messages = messages.ToArray(), temperature = 0.65 };
        var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var httpResponse = await _httpClient.PostAsync("chat/completions", content);
        var responseBody = await httpResponse.Content.ReadAsStringAsync();

        if (!httpResponse.IsSuccessStatusCode)
            throw new Exception($"Error de la IA: {responseBody}");

        var responseData = System.Text.Json.JsonSerializer.Deserialize<GroqResponse>(
            responseBody, _jsonOptions);

        var aiReply = responseData?.Choices?[0]?.Message?.Content ?? "Sin respuesta.";

        // [PASO 5: MAGIA DE EXTRACCIÓN (REGEX)]
        // Escaneamos toda la respuesta generada por Groq buscando nuestro formato de etiqueta secreto: "[PRODUCTO:numero]".
        // Por ejemplo, si Groq dice: "Te recomiendo esto [PRODUCTO:15] y esto [PRODUCTO:3]".
        var cards = new List<ProductCard>();
        var tagPattern = ProductTagRegex();
        var matches = tagPattern.Matches(aiReply);

        // Sacamos una lista única de IDs (ej: [15, 3])
        var mentionedIds = matches.Cast<System.Text.RegularExpressions.Match>()
            .Select(m => int.Parse(m.Groups[1].Value))
            .Distinct().ToList();

        // [PASO 6: CREACIÓN DE TARJETAS VISUALES]
        // Cruzamos los IDs devueltos por Groq con nuestro listado de la base de datos que sí tiene la ruta de la imagen.
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
                    Url = $"/Shop/Product/{p.id_producto}"
                });
            }
        }

        // [PASO 7: LIMPIEZA DE TEXTO PARA EL CLIENTE]
        // Removemos los textos "[PRODUCTO:id]" de la respuesta final. 
        // El cliente solo verá el texto natural ("Te recomiendo esto y esto") y el frontend renderizará las Tarjetas debajo del mensaje.
        var cleanReply = tagPattern.Replace(aiReply, "").Trim();

        // [PASO 8: GUARDAR LA RESPUESTA EN LA MEMORIA]
        // Se guarda el mensaje final (con las etiquetas intactas o limpias, elegimos limpio para historial normal)
        _context.HistorialesChatAi.Add(new LuxuryCo.Database.Models.HistorialChatAi
        {
            id_usuario = userId, session_id = sessionId, role = "assistant",
            content = aiReply, fecha_creacion = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        return new StylistResponse { Reply = cleanReply, Cards = cards };
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"\[PRODUCTO:(\d+)\]")]
    private static partial System.Text.RegularExpressions.Regex ProductTagRegex();
}

public class GroqResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("choices")]
    public List<GroqChoice>? Choices { get; set; }
}

public class GroqChoice
{
    [System.Text.Json.Serialization.JsonPropertyName("message")]
    public GroqMessage? Message { get; set; }
}

public class GroqMessage
{
    [System.Text.Json.Serialization.JsonPropertyName("content")]
    public string? Content { get; set; }
}
