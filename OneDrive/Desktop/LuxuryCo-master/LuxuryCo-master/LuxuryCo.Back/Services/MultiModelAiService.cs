using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LuxuryCo.Database.Data;
using LuxuryCo.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.CircuitBreaker;

namespace LuxuryCo.Back.Services;

public class MultiModelAiService : IAiService
{
    private readonly LuxuryCoDbContext _context;
    private readonly PromptSecurityService _promptSecurity;
    private readonly IntentParserService _intentParser;
    private readonly ColombianDialectParserService _dialectParser;
    private readonly PermissionEngine _permissionEngine;
    private readonly ConfirmationService _confirmationService;
    private readonly ToolExecutorService _toolExecutor;
    private readonly IAiProvider _groqProvider;
    private readonly IAiProvider _geminiProvider;

    private readonly IAiProvider _openRouterProvider;
    private readonly MediatR.IMediator _mediator;

    // Resiliency Policies
    private readonly AsyncPolicy _retryAndFallbackPolicy;

    public MultiModelAiService(
        LuxuryCoDbContext context,
        PromptSecurityService promptSecurity,
        IntentParserService intentParser,
        ColombianDialectParserService dialectParser,
        PermissionEngine permissionEngine,
        ConfirmationService confirmationService,
        ToolExecutorService toolExecutor,
        GroqProvider groqProvider,
        GeminiProvider geminiProvider,
        OpenRouterProvider openRouterProvider,
        MediatR.IMediator mediator)
    {
        _context = context;
        _promptSecurity = promptSecurity;
        _intentParser = intentParser;
        _dialectParser = dialectParser;
        _permissionEngine = permissionEngine;
        _confirmationService = confirmationService;
        _toolExecutor = toolExecutor;
        _groqProvider = groqProvider;
        _geminiProvider = geminiProvider;
        _openRouterProvider = openRouterProvider;
        _mediator = mediator;

        // Polly: Retry up to 2 times, then fallback to Gemini if Groq fails
        _retryAndFallbackPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromMilliseconds(200 * retryAttempt))
            .WrapAsync(Policy.TimeoutAsync(TimeSpan.FromSeconds(8)));
    }

    public async Task<string> GetAdminBusinessAdviceAsync(string userMessage, int adminUserId)
    {
        // 1. Audit Log initialization
        var auditLog = new AiActionLog
        {
            UserId = adminUserId,
            SessionId = "ADMIN_CONSOLE",
            PromptOriginal = userMessage,
            Timestamp = DateTime.UtcNow,
            TraceId = System.Diagnostics.Activity.Current?.Id ?? Guid.NewGuid().ToString()
        };

        // 2. WAF Prompt Security Sanitization
        if (!_promptSecurity.IsPromptSafe(userMessage, out string violationReason))
        {
            auditLog.SanitizedPrompt = "[BLOCKED]";
            auditLog.ErrorMessage = $"Security Block: {violationReason}";
            auditLog.Success = false;
            
            _context.AiActionLogs.Add(auditLog);
            await _context.SaveChangesAsync();
            return $"Acceso Denegado por Seguridad de IA: {violationReason}";
        }

        string sanitized = _promptSecurity.SanitizedInput(userMessage);
        auditLog.SanitizedPrompt = sanitized;

        // 3. Dialect Parsing (Colombian slang translations)
        var dialectResult = _dialectParser.ParseAmount(sanitized);
        if (dialectResult.Parsed)
        {
            sanitized += $" [Monto Detectado: {dialectResult.Amount} COP, Clarificación Requerida: {dialectResult.RequiresClarification}]";
        }

        // 4. Intent Classification
        var intent = await _intentParser.ParseIntentAsync(sanitized);
        auditLog.IntentDetected = intent.Intent;
        auditLog.Confidence = intent.Confidence;

        // 5. RBAC & Validation Layer
        var hasPermission = await _permissionEngine.HasPermissionAsync(adminUserId, "ADMIN");
        if (!hasPermission)
        {
            auditLog.Success = false;
            auditLog.ErrorMessage = "Insuficientes privilegios RBAC.";
            
            _context.AiActionLogs.Add(auditLog);
            await _context.SaveChangesAsync();
            return "No tienes los permisos requeridos para interactuar con la IA de administración.";
        }

        // 6. Intent Execution or Confirmation Routing
        if (intent.Intent != "GENERAL_CONVERSATION")
        {
            // Dialect parsing check: if high ambiguity, override auto-run
            bool requiresConfirmation = intent.Confidence < 0.95 || dialectResult.RequiresClarification;
            auditLog.RiskLevel = requiresConfirmation ? "MEDIUM" : "LOW";

            if (requiresConfirmation)
            {
                // Register Pending Action in confirmation engine
                string actionDesc = $"Modificar {intent.Intent} con parámetros: Producto {intent.Parameters.ProductId}, Cantidad/Monto: {(dialectResult.Parsed ? dialectResult.Amount : intent.Parameters.Amount)}";
                var token = await _confirmationService.RegisterPendingActionAsync(adminUserId, intent.Intent, intent.Parameters, actionDesc);

                auditLog.ActionExecuted = "PENDING_CONFIRMATION";
                _context.AiActionLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                return $"[REQUIERE_CONFIRMACION] He registrado la intención '{intent.Intent}' para tu aprobación. Token: {token}. Descripción: {actionDesc}";
            }
            else
            {
                // Dispatch to CQRS via MediatR
                if (intent.Intent == "CREATE_USER")
                {
                    var cmd = new LuxuryCo.Back.Application.Commands.CreateAdminUserCommand
                    {
                        AdminId = adminUserId,
                        Name = intent.Parameters.Name,
                        Email = intent.Parameters.Email,
                        Phone = intent.Parameters.RawAmountText // Assuming mapped here
                    };
                    var cmdResult = await _mediator.Send(cmd);
                    
                    auditLog.Success = true;
                    auditLog.ActionExecuted = "CREATE_USER";
                    auditLog.ErrorMessage = cmdResult;
                    _context.AiActionLogs.Add(auditLog);
                    await _context.SaveChangesAsync();
                    return cmdResult;
                }
                else if (intent.Intent == "GENERATE_REPORT" || intent.Intent == "GENERATE_DOCUMENT")
                {
                    var cmd = new LuxuryCo.Back.Application.Commands.GenerateReportCommand
                    {
                        AdminId = adminUserId,
                        ReportType = intent.Parameters.ProductName ?? "General",
                        Parameters = ""
                    };
                    var cmdResult = await _mediator.Send(cmd);
                    
                    auditLog.Success = true;
                    auditLog.ActionExecuted = "GENERATE_REPORT";
                    auditLog.ErrorMessage = cmdResult;
                    _context.AiActionLogs.Add(auditLog);
                    await _context.SaveChangesAsync();
                    return cmdResult;
                }
                else
                {
                    // Fallback to old tool executor for legacy intents (until fully migrated)
                    var toolResult = await _toolExecutor.ExecuteToolAsync(intent.Intent, intent.Parameters, adminUserId);
                    
                    auditLog.Success = toolResult.Success;
                    auditLog.ActionExecuted = intent.Intent;
                    auditLog.BeforeState = toolResult.BeforeStateJson;
                    auditLog.AfterState = toolResult.AfterStateJson;
                    auditLog.ErrorMessage = toolResult.Message;

                    _context.AiActionLogs.Add(auditLog);
                    await _context.SaveChangesAsync();

                    return toolResult.Message;
                }
            }
        }

        // 7. General Business Advice conversation with RAG
        // Build Prompt Context (RAG: catalog & metrics)
        var totalProducts = await _context.Productos.CountAsync();
        var totalStock = await _context.Productos.SumAsync(p => (int?)p.stock) ?? 0;
        
        string systemPrompt = $@"
Eres el Asistente Financiero y Operativo de lujo (AI) exclusivo para el Administrador de la marca 'LuxuryCo'.
REGLAS ESTRICTAS E INQUEBRANTABLES:
1. NUNCA inventes números internos. Para datos internos, usa EXCLUSIVAMENTE los datos proporcionados abajo.
2. Habla de manera profesional, estratégica y directa.

DATOS EN TIEMPO REAL:
- Total de productos en catálogo: {totalProducts}
- Unidades totales de ropa en stock: {totalStock}
";

        // Query AI Provider with Polly Fallback
        ProviderResponse response;
        try
        {
            response = await _retryAndFallbackPolicy.ExecuteAsync(() => 
                _groqProvider.GenerateCompletionAsync(systemPrompt, sanitized));
            
            if (!response.Success)
            {
                // Fallback to Gemini
                response = await _geminiProvider.GenerateCompletionAsync(systemPrompt, sanitized);
            }

            if (!response.Success)
            {
                // Fallback to OpenRouter
                response = await _openRouterProvider.GenerateCompletionAsync(systemPrompt, sanitized);
            }
        }
        catch (Exception ex)
        {
            response = new ProviderResponse
            {
                Success = false,
                ErrorMessage = $"Polly policies exhausted: {ex.Message}"
            };
        }

        auditLog.ModelUsed = response.Success ? "Groq" : "None";
        auditLog.Success = response.Success;
        auditLog.ErrorMessage = response.ErrorMessage;

        _context.AiActionLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        if (response.Success)
        {
            return _promptSecurity.SanitizeOutput(response.Reply);
        }

        return "El servicio de IA empresarial está temporalmente no disponible. Inténtalo más tarde.";
    }

    public async Task<StylistResponse> GetClientStylistAdviceAsync(
        string userMessage,
        string sessionId,
        int? userId = null,
        List<ChatHistoryEntry>? history = null,
        int? lastProductId = null)
    {
        var stylistResult = new StylistResponse();
        int activeUserId = userId ?? 0;

        // 1. WAF Security Sanitization
        if (!_promptSecurity.IsPromptSafe(userMessage, out string violationReason))
        {
            stylistResult.Reply = $"Acceso bloqueado por políticas de seguridad: {violationReason}";
            return stylistResult;
        }

        string sanitized = _promptSecurity.SanitizedInput(userMessage);

        // Interceptores locales directos de alta prioridad (Probador Virtual & Generación de Diseños)
        bool forceImageGen = false;
        string lowerSanitized = sanitized.ToLower();
        if (lowerSanitized.Contains("hacer una imagen") || 
            lowerSanitized.Contains("hazme") || 
            lowerSanitized.Contains("diseña") || 
            lowerSanitized.Contains("diseñame") || 
            lowerSanitized.Contains("créame") || 
            lowerSanitized.Contains("creame") || 
            lowerSanitized.Contains("pruebame") || 
            lowerSanitized.Contains("pruébame") || 
            lowerSanitized.Contains("ver puesto") || 
            lowerSanitized.Contains("cómo me queda") || 
            lowerSanitized.Contains("dibuja") || 
            lowerSanitized.Contains("genera una imagen") || 
            lowerSanitized.Contains("crear una imagen") || 
            lowerSanitized.Contains("dibuja una imagen") ||
            lowerSanitized.Contains("crear imagen") ||
            lowerSanitized.Contains("hacer imagen"))
        {
            forceImageGen = true;
        }

        // 2. Intent Parsing
        var intent = forceImageGen 
            ? new ParsedIntent { Intent = "GENERATE_IMAGE", Confidence = 0.99 }
            : await _intentParser.ParseIntentAsync(sanitized);

        // 2a. Si quiere agregar al carrito pero no está logueado → informarle amablemente
        if (intent.Intent == "ADD_TO_CART" && activeUserId == 0)
        {
            stylistResult.Reply = "🔒 Para agregar productos al carrito necesitas **iniciar sesión** o **crear una cuenta**. " +
                                  "\n\n¿Quieres [iniciar sesión](/Account/Login) o [crear una cuenta](/Account/Register)? " +
                                  "\n\nUna vez logueado, puedo ayudarte a agregar este artículo de inmediato. 😊";
            return stylistResult;
        }

        // 2c. Si el cliente quiere generar, diseñar o probarse una imagen (Probador en línea)
        if (intent.Intent == "GENERATE_IMAGE")
        {
            string promptToUse = string.Empty;
            if (!string.IsNullOrWhiteSpace(intent.Parameters.ProductName))
            {
                promptToUse = intent.Parameters.ProductName;
            }
            else if (lastProductId.HasValue && lastProductId.Value > 0)
            {
                var prod = await _context.Productos.FindAsync(lastProductId.Value);
                if (prod != null)
                {
                    promptToUse = $"{prod.nombre}";
                }
            }

            if (string.IsNullOrWhiteSpace(promptToUse))
            {
                promptToUse = sanitized;
            }

            // Si es una petición de probarse la prenda (virtual try-on), enriquecerla
            if (sanitized.ToLower().Contains("pruebame") || sanitized.ToLower().Contains("pruébame") || 
                sanitized.ToLower().Contains("ver puesto") || sanitized.ToLower().Contains("cómo me queda") || 
                sanitized.ToLower().Contains("pruébamela") || sanitized.ToLower().Contains("pruebamela"))
            {
                promptToUse = $"A professional model elegantly wearing the luxury apparel: {promptToUse}, highly detailed fashion studio mockup, editorial photo";
            }

            stylistResult.Reply = $"[[IMAGE_GEN_TRIGGER:{promptToUse}]]";
            return stylistResult;
        }

        // 2b. Si quiere agregar al carrito y está logueado, intentar resolver el producto
        if (intent.Intent == "ADD_TO_CART" && activeUserId > 0)
        {
            // Si la IA no detectó un ProductId concreto pero hay un nombre, buscar en BD por nombre
            if (intent.Parameters.ProductId == 0 && !string.IsNullOrWhiteSpace(intent.Parameters.ProductName))
            {
                var normName = intent.Parameters.ProductName.ToLower().Trim();
                var matchedProduct = await _context.Productos
                    .Where(p => p.activo && p.nombre.ToLower().Contains(normName))
                    .FirstOrDefaultAsync();

                if (matchedProduct != null)
                {
                    intent.Parameters.ProductId = matchedProduct.id_producto;
                }
            }

            // Si la IA no detectó un ProductId concreto pero hay un último producto mostrado, usarlo
            if (intent.Parameters.ProductId == 0 && lastProductId.HasValue && lastProductId.Value > 0)
            {
                intent.Parameters.ProductId = lastProductId.Value;
            }

            if (intent.Parameters.ProductId > 0)
            {
                // Ejecutar la herramienta de agregar al carrito
                var toolResult = await _toolExecutor.ExecuteToolAsync(intent.Intent, intent.Parameters, activeUserId);
                stylistResult.Reply = toolResult.Message;
                return stylistResult;
            }
            else
            {
                // No hay suficiente contexto para saber qué producto agregar
                stylistResult.Reply = "No tengo claro qué producto deseas agregar. ¿Puedes indicarme el nombre o hacer clic en **'Ver producto'** de la tarjeta que te mostré?";
                return stylistResult;
            }
        }

        // 3. Stylist RAG Catalogo Recommendations
        // Mapeo de palabras clave de género/categoría a la columna 'seccion' de la BD
        var genderSectionKeywords = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "hombre" , "hombre"  }, { "masculino" , "hombre"  }, { "caballero"  , "hombre"  }, { "hombres", "hombre" },
            { "mujer"  , "mujer"   }, { "femenino"  , "mujer"   }, { "dama"       , "mujer"   }, { "mujeres", "mujer"  }, { "señora", "mujer" },
            { "niño"   , "niños"   }, { "niña"      , "niños"   }, { "niños"      , "niños"   }, { "niñas"  , "niños"  }, { "infante", "niños" }, { "bebé", "niños" }
        };

        // Detectar si la búsqueda es por sección (género/categoría)
        string? sectionFilter = null;
        if (intent.Intent == "SEARCH_PRODUCT" && !string.IsNullOrWhiteSpace(intent.Parameters.ProductName))
        {
            var productNameLower = intent.Parameters.ProductName.ToLower().Trim();
            if (genderSectionKeywords.TryGetValue(productNameLower, out var mappedSection))
            {
                sectionFilter = mappedSection;
            }
        }
        // También verificar el mensaje directo del usuario para mayor robustez
        if (sectionFilter == null)
        {
            foreach (var kv in genderSectionKeywords)
            {
                if (lowerSanitized.Contains(kv.Key) && intent.Intent == "SEARCH_PRODUCT")
                {
                    sectionFilter = kv.Value;
                    break;
                }
            }
        }

        var activeProducts = await _context.Productos
            .Where(p => p.activo && p.stock > 0)
            .Select(p => new
            {
                p.id_producto, p.nombre, p.precio, p.seccion,
                imagen = p.Imagenes.Where(i => i.principal).Select(i => i.url_imagen).FirstOrDefault()
            })
            .ToListAsync();

        // Aplicar filtro de sección/género si fue detectado
        var filteredProducts = sectionFilter != null
            ? activeProducts.Where(p => p.seccion != null && p.seccion.ToLower().Contains(sectionFilter.ToLower())).ToList()
            : activeProducts;

        // Si el filtro de sección no dio resultados, usar el catálogo completo como fallback
        if (sectionFilter != null && filteredProducts.Count == 0)
        {
            filteredProducts = activeProducts;
        }

        var catalogForPrompt = filteredProducts.Select(p => new { p.id_producto, p.nombre, p.precio, p.seccion });
        var productsJson = JsonSerializer.Serialize(catalogForPrompt);

        // 4. Construir el historial formateado para el prompt (contexto de conversación)
        string historyBlock = string.Empty;
        if (history != null && history.Count > 0)
        {
            // Tomar máximo los últimos 10 turnos para no exceder tokens
            var recentHistory = history.TakeLast(10).ToList();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("\nHISTORIAL RECIENTE DE LA CONVERSACIÓN (del más antiguo al más reciente):");
            foreach (var entry in recentHistory)
            {
                string roleLabel = entry.Role == "user" ? "Cliente" : "Estilista";
                sb.AppendLine($"{roleLabel}: {entry.Content}");
            }
            historyBlock = sb.ToString();
        }

        var loginStatus = activeUserId > 0 ? "LOGUEADO (puede agregar al carrito)" : "NO LOGUEADO (si pide agregar al carrito, díselo que debe iniciar sesión)";
        var categoryContext = sectionFilter != null 
            ? $"\nCONTEXTO: El cliente está buscando específicamente productos de la sección '{sectionFilter}'. El catálogo ya está pre-filtrado para esta categoría."
            : string.Empty;

        var systemPrompt = $@"Eres un Asesor de Estilo exclusivo y 'Personal Shopper' de LuxuryCo.
Tono: amable, sofisticado, breve.
ESTADO USUARIO: {loginStatus}{categoryContext}
REGLA 1: Solo recomienda productos del catálogo JSON. Si no existe lo que piden, dílo amablemente.
REGLA 2: Cuando recomiendes 1 o más productos, incluye la etiqueta [PRODUCTO:id_producto] exactamente así (reemplaza id_producto por el número). Ejemplo: [PRODUCTO:3]
REGLA 3: Máximo 2 productos recomendados por respuesta.
REGLA 4: Tienes memoria de la conversación. Usa el historial para responder con coherencia.
REGLA 5: NUNCA afirmes haber realizado acciones del sistema como agregar al carrito (ej. decir ""Ya lo agregué"" o ""Listo, he añadido...""). El sistema backend se encarga de eso. Si el cliente pide agregar al carrito, limita tu respuesta a guiarlo o sugerirle el producto, pero no inventes que modificaste su carrito.
{historyBlock}
CATÁLOGO:
{productsJson}";


        var response = await _retryAndFallbackPolicy.ExecuteAsync(() =>
            _groqProvider.GenerateCompletionAsync(systemPrompt, sanitized));

        string geminiError = "";

        if (!response.Success)
        {
            response = await _geminiProvider.GenerateCompletionAsync(systemPrompt, sanitized);
            if (!response.Success)
            {
                geminiError = response.ErrorMessage;
            }
        }

        if (!response.Success)
        {
            response = await _openRouterProvider.GenerateCompletionAsync(systemPrompt, sanitized);
        }

        if (response.Success)
        {
            var rawReply = response.Reply;
            var tagPattern = new System.Text.RegularExpressions.Regex(@"\[PRODUCTO:(\d+)\]");
            var matches = tagPattern.Matches(rawReply);

            var mentionedIds = matches.Cast<System.Text.RegularExpressions.Match>()
                .Select(m => int.Parse(m.Groups[1].Value))
                .Distinct().ToList();

            foreach (var id in mentionedIds)
            {
                var p = activeProducts.FirstOrDefault(x => x.id_producto == id);
                if (p != null)
                {
                    stylistResult.Cards.Add(new ProductCard
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

            stylistResult.Reply = _promptSecurity.SanitizeOutput(tagPattern.Replace(rawReply, "").Trim());
        }
        else
        {
            stylistResult.Reply = "Lo lamento, no puedo procesar tu recomendación en este momento. Por favor, intenta de nuevo más tarde.";
        }

        return stylistResult;
    }
}
