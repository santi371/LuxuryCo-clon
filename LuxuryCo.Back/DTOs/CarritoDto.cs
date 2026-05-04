using System;
using System.Collections.Generic;

namespace LuxuryCo.Back.DTOs;

public class DetalleCarritoDto
{
    public int IdDetalleCarrito { get; set; }
    public int IdProducto { get; set; }
    public string NombreProducto { get; set; } = null!;
    public string? MarcaNombre { get; set; }
    public string? ImagenUrl { get; set; }
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal => PrecioUnitario * Cantidad;
}

public class CarritoDto
{
    public int IdCarrito { get; set; }
    public int IdUsuario { get; set; }
    public DateTime FechaCreacion { get; set; }
    public List<DetalleCarritoDto> Detalles { get; set; } = new();
    
    public decimal Total => Detalles.Sum(d => d.Subtotal);
    public int TotalItems => Detalles.Sum(d => d.Cantidad);
}
