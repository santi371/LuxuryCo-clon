using System;

namespace LuxuryCo.Back.DTOs;

public class CheckoutRequestDto
{
    // Payment Simulation
    public string CardToken { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = Guid.NewGuid().ToString();

    // Shipping Address
    public string DireccionLinea1 { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string CodigoPostal { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Referencias { get; set; } = string.Empty;
}

public class CheckoutResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public string? TrackingNumber { get; set; }
}
