using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LuxuryCo.Front.ViewModels
{
    public class UsuarioEditViewModel
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        public string Telefono { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty; // Solo lectura

        [Required(ErrorMessage = "Debes seleccionar un rol")]
        public int IdRol { get; set; }

        // Propiedad auxiliar para el despliegue
        public IEnumerable<SelectListItem> RolesDisponibles { get; set; } = new List<SelectListItem>();

        public int? IdSede { get; set; }
        public IEnumerable<SelectListItem> SedesDisponibles { get; set; } = new List<SelectListItem>();
    }
}
