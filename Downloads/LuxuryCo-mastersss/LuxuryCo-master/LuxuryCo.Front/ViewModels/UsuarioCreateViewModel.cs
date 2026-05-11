using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LuxuryCo.Front.ViewModels
{
    public class UsuarioCreateViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email incorrecto")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debes seleccionar un rol")]
        public int IdRol { get; set; }

        // Propiedad auxiliar para llenar el <select> en la vista
        public IEnumerable<SelectListItem> RolesDisponibles { get; set; } = new List<SelectListItem>();

        public int? IdSede { get; set; }
        public IEnumerable<SelectListItem> SedesDisponibles { get; set; } = new List<SelectListItem>();
    }
}
