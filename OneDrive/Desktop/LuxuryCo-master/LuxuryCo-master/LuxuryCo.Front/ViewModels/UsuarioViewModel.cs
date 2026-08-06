namespace LuxuryCo.Front.ViewModels
{
    public class UsuarioViewModel
    {
        public int IdUsuario { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Email { get; set; }
        public string? Rol { get; set; }
        public bool Activo { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public int? IdSede { get; set; }
        public string? SedeNombre { get; set; }
    }
}
