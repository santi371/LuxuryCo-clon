namespace LuxuryCo.Front.ViewModels
{
    public class SedeViewModel
    {
        public int IdSede { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public bool Activa { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
