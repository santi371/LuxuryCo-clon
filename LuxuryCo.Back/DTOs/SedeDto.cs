namespace LuxuryCo.Back.DTOs;

public class SedeDto
{
    public int IdSede { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public bool Activa { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class SedeCreateUpdateDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public bool Activa { get; set; } = true;
}
