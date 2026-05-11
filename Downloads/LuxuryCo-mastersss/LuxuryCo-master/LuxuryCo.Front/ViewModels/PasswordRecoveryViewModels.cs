using System.ComponentModel.DataAnnotations;

namespace LuxuryCo.Front.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        public string Email { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "El token de seguridad es obligatorio")]
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string NewPassword { get; set; }
        
        [Required(ErrorMessage = "Debes confirmar tu contraseña")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; }
    }
}
