using System;
using System.Collections.Generic;
using System.Linq;

namespace LuxuryCo.Front.ViewModels;

public class DetalleCarritoViewModel
{
    public int IdDetalleCarrito { get; set; }
    public int IdProducto { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string? MarcaNombre { get; set; }
    public string? ImagenUrl { get; set; }
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public string? Talla { get; set; }
    public decimal Subtotal => PrecioUnitario * Cantidad;
}

public class CarritoViewModel
{
    public int IdCarrito { get; set; }
    public int IdUsuario { get; set; }
    public DateTime FechaCreacion { get; set; }
    public List<DetalleCarritoViewModel> Detalles { get; set; } = new();
    
    public decimal Total => Detalles.Sum(d => d.Subtotal);
    public int TotalItems => Detalles.Sum(d => d.Cantidad);
}
