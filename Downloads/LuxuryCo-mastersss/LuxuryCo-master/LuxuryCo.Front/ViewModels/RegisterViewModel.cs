using System.ComponentModel.DataAnnotations;

namespace LuxuryCo.Front.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y máximo {1} caracteres.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
