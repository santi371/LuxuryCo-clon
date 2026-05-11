namespace LuxuryCo.Front.ViewModels
{
    // Para el panel de stock
    public class InventarioSedeViewModel
    {
        public int IdInventario { get; set; }
        public int IdProducto { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public string? ProductoImagen { get; set; }
        public int IdSede { get; set; }
        public string SedeNombre { get; set; } = string.Empty;
        public int CantidadDisponible { get; set; }
        public int UmbralMinimo { get; set; }
        public bool BajoStock => CantidadDisponible <= UmbralMinimo;
    }

    // Para registrar una entrada de mercadería
    public class EntradaInventarioViewModel
    {
        public int IdProducto { get; set; }
        public int? IdProveedor { get; set; }
        public int IdSede { get; set; }
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public string? Notas { get; set; }
    }

    // Para el historial de abastecimiento  
    public class AbastecimientoViewModel
    {
        public int IdAbastecimiento { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public string? ProveedorNombre { get; set; }
        public string SedeNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public DateTime FechaIngreso { get; set; }
        public string? Notas { get; set; }
    }
}
