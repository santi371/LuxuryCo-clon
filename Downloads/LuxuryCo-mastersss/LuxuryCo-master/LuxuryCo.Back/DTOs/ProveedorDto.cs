using System.ComponentModel.DataAnnotations;

namespace LuxuryCo.Back.DTOs;

public class ProveedorDto
{
    public int IdProveedor { get; set; }
    public string Nombre { get; set; }
    public string? Contacto { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public bool Activo { get; set; }
}

public class ProveedorCreateUpdateDto
{
    [Required(ErrorMessage = "El nombre del proveedor es obligatorio")]
    [MaxLength(150)]
    public string Nombre { get; set; }

    [MaxLength(100)]
    public string? Contacto { get; set; }

    [MaxLength(50)]
    public string? Telefono { get; set; }

    [MaxLength(150)]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    public string? Email { get; set; }
}
