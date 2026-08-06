using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace LuxuryCo.Back.Services;

public class IntentParserService
{
    private readonly GroqProvider _groqProvider;

    public IntentParserService(GroqProvider groqProvider)
    {
        _groqProvider = groqProvider;
    }

    public async Task<ParsedIntent> ParseIntentAsync(string prompt)
    {
        var systemPrompt = @"
Analiza el mensaje del usuario y clasifícalo estrictamente en una de las siguientes intenciones (Intent):
- UPDATE_PRICE (Cambiar, subir o bajar precio de un producto)
- UPDATE_STOCK (Aumentar o reducir stock en inventario)
- SEARCH_PRODUCT (Buscar o filtrar productos por nombre, categoría, género o sección)
- ADD_TO_CART (Agregar prendas al carrito)
- GENERATE_IMAGE (Diseñar, generar, visualizar, probar o ver cómo luce una prenda)
- GENERATE_DOCUMENT (Generar, exportar, crear o descargar un documento Word/PDF con información, resumen, cotizaciones o perfiles solicitados)
- CREATE_INVOICE_DRAFT (Generar borrador de factura/cotización)
- REGISTER_USER (Registrar, crear cuenta, inscribir usuario. El usuario debe proporcionar nombre, email y opcionalmente teléfono. NUNCA pidas contraseña, la IA no debe manejar contraseñas.)
- GENERAL_CONVERSATION (Preguntas de estilo, saludos, etc.)

REGLAS IMPORTANTES:
- Si el usuario menciona 'hombre', 'masculino', 'caballero' → SEARCH_PRODUCT con ProductName='hombre'
- Si el usuario dice 'hazme', 'diseña', 'genera', 'pruébame' → GENERATE_IMAGE
- Si el usuario quiere registrarse (REGISTER_USER), extrae Nombre e Email. Si falta alguno, la IA debe pedirlos de forma conversacional.
- NUNCA PIDAS CONTRASEÑAS en el chat. Si detectas una contraseña en el input del usuario, bórrala inmediatamente del texto.
- Si la intención no está totalmente clara, clasifica como GENERAL_CONVERSATION.

Debes devolver EXCLUSIVAMENTE un objeto JSON con el siguiente formato, sin explicaciones ni markdown:
{
  ""Intent"": ""INTENCION_DETECTADA"",
  ""Confidence"": 0.95,
  ""Parameters"": {
    ""ProductName"": ""nombre o fragmento o categoría"",
    ""ProductId"": 0,
    ""Amount"": 0.0,
    ""Quantity"": 0,
    ""RawAmountText"": """",
    ""Name"": ""Juan Perez"",
    ""Email"": ""juan@example.com""
  }
}
";

        try
        {
            var response = await _groqProvider.GenerateCompletionAsync(systemPrompt, prompt, temperature: 0.1);
            if (response.Success)
            {
                var cleanJson = response.Reply.Trim();
                
                // Remove markdown json wrappers if present
                if (cleanJson.StartsWith("```"))
                {
                    cleanJson = cleanJson.Replace("```json", "").Replace("```", "").Trim();
                }

                var parsed = JsonSerializer.Deserialize<ParsedIntent>(cleanJson);
                if (parsed != null)
                {
                    return parsed;
                }
            }
        }
        catch
        {
            // Fallback inside catch
        }

        return new ParsedIntent
        {
            Intent = "GENERAL_CONVERSATION",
            Confidence = 0.50
        };
    }
}

public class ParsedIntent
{
    public string Intent { get; set; } = "GENERAL_CONVERSATION";
    public double Confidence { get; set; }
    public IntentParameters Parameters { get; set; } = new();
}

public class IntentParameters
{
    public string ProductName { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public decimal Amount { get; set; }
    public int Quantity { get; set; }
    public string RawAmountText { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
