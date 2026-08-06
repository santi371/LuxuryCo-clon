using System;

namespace LuxuryCo.Database.Models
{
    public interface IMultiTenantEntity
    {
        Guid TenantId { get; set; }
    }
}
