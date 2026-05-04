namespace LuxuryCo.Front.ViewModels
{
    public class ProductoViewModel
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public bool Activo { get; set; }
        public string? CategoriaNombre { get; set; }
        public string? MarcaNombre { get; set; }
        public string? ImagenPrincipalUrl { get; set; }
        public string? Referencia { get; set; }
        public string? Seccion { get; set; } // "Hombre", "Mujer" o "Accesorios"
        public List<string> ImagenesUrls { get; set; } = new List<string>();
        public List<ImagenViewModel> ImagenesDetalle { get; set; } = new List<ImagenViewModel>();
    }

    public class ImagenViewModel
    {
        public int IdImagen { get; set; }
        public string UrlImagen { get; set; }
        public bool Principal { get; set; }
        public int Orden { get; set; }
    }
}
