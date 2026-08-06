using System.ComponentModel.DataAnnotations;

namespace LuxuryCo.Back.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ]+(\s[a-zA-ZáéíóúÁÉÍÓÚñÑ]+)*$", ErrorMessage = "El nombre solo puede contener letras y no debe tener espacios múltiples o al inicio/final.")]
    public string Nombre { get; set; }

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ]+(\s[a-zA-ZáéíóúÁÉÍÓÚñÑ]+)*$", ErrorMessage = "El apellido solo puede contener letras y no debe tener espacios múltiples o al inicio/final.")]
    public string Apellido { get; set; }

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "Formato de email inválido.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, ErrorMessage = "La contraseña debe tener al menos {2} caracteres.", MinimumLength = 8)]
    public string Password { get; set; }

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^\d{7,15}$", ErrorMessage = "El teléfono solo debe contener números (entre 7 y 15 dígitos).")]
    public string Telefono { get; set; }
}
