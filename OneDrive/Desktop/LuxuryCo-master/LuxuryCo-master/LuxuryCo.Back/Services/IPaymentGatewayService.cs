using System.Threading.Tasks;

namespace LuxuryCo.Back.Services;

public interface IPaymentGatewayService
{
    Task<(bool Success, string TransactionId)> ProcessPaymentAsync(string token, decimal amount);
}
