using System.ComponentModel.DataAnnotations;

namespace LuxuryCo.Back.DTOs;

public class UsuarioUpdateDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Nombre { get; set; }

    [Required(ErrorMessage = "El apellido es obligatorio")]
    public string Apellido { get; set; }

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    public string Telefono { get; set; }

    [Required(ErrorMessage = "El rol es obligatorio")]
    public int IdRol { get; set; }

    public int? IdSede { get; set; }
}
