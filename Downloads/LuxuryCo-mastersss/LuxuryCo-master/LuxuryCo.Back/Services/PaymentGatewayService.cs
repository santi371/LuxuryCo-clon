using System;
using System.Threading.Tasks;

namespace LuxuryCo.Back.Services;

public class PaymentGatewayService : IPaymentGatewayService
{
    public async Task<(bool Success, string TransactionId)> ProcessPaymentAsync(string token, decimal amount)
    {
        // Simulamos latencia de red contra Banco/Stripe (2 seg)
        await Task.Delay(2000);

        // Lógica Random 80/20 (80% éxito, 20% decline)
        var rnd = new Random();
        int probability = rnd.Next(1, 101);

        if (probability <= 80)
        {
            return (true, $"TX-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}");
        }
        else
        {
            return (false, string.Empty);
        }
    }
}
