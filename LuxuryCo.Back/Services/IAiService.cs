using System.Threading.Tasks;

namespace LuxuryCo.Back.Services;

// Interfaz que define los contratos (métodos) para el Servicio de Inteligencia Artificial
public interface IAiService
{
    // Método para el chat del Administrador (análisis de negocio, stock, métricas)
    Task<string> GetAdminBusinessAdviceAsync(string userMessage);
    
    // Método para el chat del Cliente (Asesor de Estilo, devuelve texto y tarjetas de producto visuales)
    Task<StylistResponse> GetClientStylistAdviceAsync(string userMessage, string sessionId, int? userId = null);
}

// Modelo que representa una Tarjeta Visual de Producto que la IA recomienda en el chat
public class ProductCard
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Seccion { get; set; } = string.Empty;
    public string Imagen { get; set; } = "/img/placeholder.png";
    public string Url { get; set; } = "#";
}

// Modelo de respuesta combinada para el widget del Estilista
public class StylistResponse
{
    // El mensaje de texto natural que dice la IA
    public string Reply { get; set; } = string.Empty;
    
    // La lista de productos que la IA detectó y recomendó
    public List<ProductCard> Cards { get; set; } = new();
}
