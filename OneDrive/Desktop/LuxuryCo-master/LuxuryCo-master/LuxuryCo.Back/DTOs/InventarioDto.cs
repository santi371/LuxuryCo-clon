namespace LuxuryCo.Back.DTOs;

// --- Inventario por Sede ---
public class InventarioSedeDto
{
    public int IdInventario { get; set; }
    public int IdProducto { get; set; }
    public string ProductoNombre { get; set; }
    public string? ProductoImagen { get; set; }
    public int IdSede { get; set; }
    public string SedeNombre { get; set; }
    public int CantidadDisponible { get; set; }
    public int UmbralMinimo { get; set; }
    public bool BajoStock => CantidadDisponible <= UmbralMinimo;
}

public class InventarioAjusteDto
{
    public int IdProducto { get; set; }
    public int IdSede { get; set; }
    public int CantidadNueva { get; set; }
    public int UmbralMinimo { get; set; } = 5;
}

// --- Entrada de mercadería (Abastecimiento) ---
public class AbastecimientoDto
{
    public int IdAbastecimiento { get; set; }
    public int IdProducto { get; set; }
    public string ProductoNombre { get; set; }
    public int? IdProveedor { get; set; }
    public string? ProveedorNombre { get; set; }
    public int IdSede { get; set; }
    public string SedeNombre { get; set; }
    public int Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public DateTime FechaIngreso { get; set; }
    public string? Notas { get; set; }
}

public class AbastecimientoCreateDto
{
    public int IdProducto { get; set; }
    public int? IdProveedor { get; set; }
    public int IdSede { get; set; }
    public int Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public string? Notas { get; set; }
}
