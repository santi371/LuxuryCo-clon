namespace LuxuryCo.Back.DTOs;

public class ProductoDto
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public bool Activo { get; set; }
    public string CategoriaNombre { get; set; }
    public string MarcaNombre { get; set; }
    public string ImagenPrincipalUrl { get; set; }
    public string? Seccion { get; set; } // "Hombre", "Mujer" o "Accesorios"
    public List<string> ImagenesUrls { get; set; } = new List<string>();
    public List<ImagenDto> ImagenesDetalle { get; set; } = new List<ImagenDto>();
}

public class ImagenDto
{
    public int IdImagen { get; set; }
    public string UrlImagen { get; set; }
    public bool Principal { get; set; }
    public int Orden { get; set; }
}

