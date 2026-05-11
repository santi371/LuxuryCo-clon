using System.ComponentModel.DataAnnotations;

namespace LuxuryCo.Back.DTOs;

public class UsuarioCreateDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Nombre { get; set; }

    [Required(ErrorMessage = "El apellido es obligatorio")]
    public string Apellido { get; set; }

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de email incorrecto")]
    public string Email { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string Password { get; set; }

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    public string Telefono { get; set; }

    [Required(ErrorMessage = "El rol es obligatorio")]
    public int IdRol { get; set; }

    public int? IdSede { get; set; }
}
