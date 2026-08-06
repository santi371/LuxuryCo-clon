using Hangfire;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace LuxuryCo.Back.Application.Commands;

public class GenerateReportCommandHandler : IRequestHandler<GenerateReportCommand, string>
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public GenerateReportCommandHandler(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public Task<string> Handle(GenerateReportCommand request, CancellationToken cancellationToken)
    {
        // Enqueue the job in Hangfire for async processing.
        // We simulate a job here since the actual DocumentGenerationService logic will be refactored to be a Hangfire background job
        _backgroundJobClient.Enqueue(() => ProcessReportAsync(request.AdminId, request.ReportType));

        return Task.FromResult($"Su reporte de '{request.ReportType}' se está generando en segundo plano. Recibirá una notificación cuando esté listo.");
    }

    // Public method required for Hangfire expression tree
    public Task ProcessReportAsync(int adminId, string reportType)
    {
        // Actual report generation logic will be here, then publish ReportGeneratedEvent via MassTransit
        return Task.CompletedTask;
    }
}
