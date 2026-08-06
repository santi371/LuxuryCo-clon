using System;

namespace LuxuryCo.Back.Services;

public interface ITenantProvider
{
    Guid GetTenantId();
}
