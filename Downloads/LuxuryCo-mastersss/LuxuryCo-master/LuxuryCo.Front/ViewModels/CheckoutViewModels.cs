using System.ComponentModel.DataAnnotations;

namespace LuxuryCo.Front.ViewModels;

public class CheckoutRequestViewModel
{
    [Required(ErrorMessage = "La dirección de envío es obligatoria.")]
    public string DireccionLinea1 { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe indicar su ciudad.")]
    public string Ciudad { get; set; } = string.Empty;

    public string CodigoPostal { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio para la logística.")]
    public string Telefono { get; set; } = string.Empty;

    public string Referencias { get; set; } = string.Empty;

    // Tokens Ocultos para Mock
    public string CardToken { get; set; } = "tok_mock_2026";
    public string IdempotencyKey { get; set; } = Guid.NewGuid().ToString();
}

public class CheckoutResponseViewModel
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public string? TrackingNumber { get; set; }
}
