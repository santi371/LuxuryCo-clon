using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LuxuryCo.Front.ViewModels
{
    public class ProveedorViewModel
    {
        public int IdProveedor { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Contacto { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public bool Activo { get; set; }
    }

    public class ProveedorCreateUpdateViewModel
    {
        public int IdProveedor { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(150, ErrorMessage = "Máximo 150 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Contacto { get; set; }

        [MaxLength(50)]
        public string? Telefono { get; set; }

        [MaxLength(150)]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string? Email { get; set; }
    }
}
