using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace LuxuryCo.Back.Services;

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetTenantId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return Guid.Empty;

        // Extract tenant ID from custom header, or JWT claim, or subdomain.
        // For now, we look for a header "X-Tenant-ID"
        if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantHeader))
        {
            if (Guid.TryParse(tenantHeader.FirstOrDefault(), out var tenantId))
            {
                return tenantId;
            }
        }

        return Guid.Empty; // Default tenant
    }
}
