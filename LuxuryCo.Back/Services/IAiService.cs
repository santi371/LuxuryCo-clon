using System.Threading.Tasks;

namespace LuxuryCo.Back.Services;

public interface IAiService
{
    Task<string> GetAdminBusinessAdviceAsync(string userMessage);
    Task<string> GetClientStylistAdviceAsync(string userMessage, string sessionId, int? userId = null);
}
