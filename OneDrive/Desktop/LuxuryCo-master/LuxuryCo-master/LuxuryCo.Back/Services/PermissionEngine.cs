using System;
using System.Threading.Tasks;
using LuxuryCo.Database.Data;
using Microsoft.EntityFrameworkCore;

namespace LuxuryCo.Back.Services;

public class PermissionEngine
{
    private readonly LuxuryCoDbContext _context;

    public PermissionEngine(LuxuryCoDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasPermissionAsync(int userId, string requiredRole, int? resourceId = null, string resourceType = "")
    {
        var user = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.id_usuario == userId);

        if (user == null || !user.activo)
        {
            return false;
        }

        // Rol Validation
        var userRoleName = user.Rol?.nombre_rol?.ToUpper() ?? string.Empty;
        
        // ADMIN bypasses all security checks
        if (userRoleName == "ADMIN")
        {
            return true;
        }

        if (requiredRole.ToUpper() == "ADMIN")
        {
            return false; // Only actual ADMIN allowed
        }

        if (requiredRole.ToUpper() == "MANAGER" || requiredRole.ToUpper() == "SUPERVISOR")
        {
            return userRoleName == "SUPERVISOR" || userRoleName == "ADMIN";
        }

        if (requiredRole.ToUpper() == "SELLER" || requiredRole.ToUpper() == "VENDEDOR")
        {
            return userRoleName == "VENDEDOR" || userRoleName == "SUPERVISOR" || userRoleName == "ADMIN";
        }

        // Ownership and Tenant Isolation checks if applicable
        if (resourceId.HasValue && !string.IsNullOrEmpty(resourceType))
        {
            if (resourceType == "Pedido" || resourceType == "Carrito")
            {
                // Check if user owns the cart/order
                if (resourceType == "Carrito")
                {
                    var cart = await _context.Carritos.FindAsync(resourceId.Value);
                    return cart != null && cart.id_usuario == userId;
                }
                if (resourceType == "Pedido")
                {
                    var order = await _context.Pedidos.FindAsync(resourceId.Value);
                    return order != null && order.id_usuario == userId;
                }
            }
        }

        return userRoleName == requiredRole.ToUpper();
    }
}
