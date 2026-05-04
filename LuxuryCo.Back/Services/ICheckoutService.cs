using System.Threading.Tasks;
using LuxuryCo.Back.DTOs;

namespace LuxuryCo.Back.Services;

public interface ICheckoutService
{
    Task<CheckoutResponseDto> PlaceOrderAsync(int idUsuario, CheckoutRequestDto request);
}
