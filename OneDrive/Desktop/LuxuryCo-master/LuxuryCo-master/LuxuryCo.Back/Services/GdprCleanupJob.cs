using Hangfire;
using LuxuryCo.Database.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LuxuryCo.Back.Services;

public class GdprCleanupJob
{
    private readonly LuxuryCoDbContext _context;

    public GdprCleanupJob(LuxuryCoDbContext context)
    {
        _context = context;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ProcessDataRetentionPoliciesAsync()
    {
        // GDPR: Eliminar registros de logs de IA más antiguos de 90 días
        var cutoffDate = DateTime.UtcNow.AddDays(-90);
        
        var expiredLogs = _context.AiActionLogs.Where(l => l.Timestamp < cutoffDate);
        _context.AiActionLogs.RemoveRange(expiredLogs);

        // Aquí también se implementarían los borrados físicos de usuarios marcados como eliminados lógicos ("soft-delete")
        // después de su período de retención legal (ej. 30 días)

        await _context.SaveChangesAsync();
    }
}
