using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LuxuryCo.Front.ViewModels
{
    public class MarcaViewModel
    {
        public int IdMarca { get; set; }

        [Required(ErrorMessage = "El nombre de la marca es obligatorio")]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public string LogoUrl { get; set; }

        [Display(Name = "Subir Logo")]
        public IFormFile LogoImagen { get; set; }
    }
}
