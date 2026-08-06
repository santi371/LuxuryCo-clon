namespace LuxuryCo.Back.DTOs;

public class ProductoCreateUpdateDto
{
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int? IdCategoria { get; set; }
    public int? IdMarca { get; set; }
    public string? Seccion { get; set; } // "Hombre", "Mujer" o "Accesorios"
}
