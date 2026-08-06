using System;
using System.Threading.Tasks;
using LuxuryCo.Database.Data;
using Microsoft.EntityFrameworkCore;

namespace LuxuryCo.Back.Services;

public class BusinessRulesService
{
    private readonly LuxuryCoDbContext _context;

    public BusinessRulesService(LuxuryCoDbContext context)
    {
        _context = context;
    }

    public async Task<BusinessValidationResult> ValidatePriceUpdateAsync(int productId, decimal newPrice)
    {
        var result = new BusinessValidationResult();
        
        if (newPrice <= 0)
        {
            result.IsValid = false;
            result.FailureReason = "El precio de un producto de lujo no puede ser cero o negativo.";
            return result;
        }

        var product = await _context.Productos.FindAsync(productId);
        if (product == null)
        {
            result.IsValid = false;
            result.FailureReason = "El producto especificado no existe.";
            return result;
        }

        // Limit maximum price change percentage to prevent extreme errors (e.g. max 300% increase or 90% decrease)
        decimal currentPrice = product.precio;
        if (currentPrice > 0)
        {
            decimal pctChange = Math.Abs(newPrice - currentPrice) / currentPrice;
            if (pctChange > 3.0m)
            {
                result.IsValid = false;
                result.FailureReason = "El aumento de precio supera el límite permitido de 300% de una sola vez.";
                return result;
            }
            if (newPrice < currentPrice * 0.1m)
            {
                result.IsValid = false;
                result.FailureReason = "La reducción de precio supera el descuento límite de 90%.";
                return result;
            }
        }

        result.IsValid = true;
        return result;
    }

    public BusinessValidationResult ValidateStockUpdate(int stockAmount)
    {
        var result = new BusinessValidationResult();
        
        if (stockAmount < 0)
        {
            result.IsValid = false;
            result.FailureReason = "El stock del inventario no puede ser negativo.";
            return result;
        }

        if (stockAmount > 10000)
        {
            result.IsValid = false;
            result.FailureReason = "La cantidad ingresada supera el límite máximo de almacenamiento de stock unitario por lote (10,000 unidades).";
            return result;
        }

        result.IsValid = true;
        return result;
    }
}

public class BusinessValidationResult
{
    public bool IsValid { get; set; } = true;
    public string FailureReason { get; set; } = string.Empty;
}
