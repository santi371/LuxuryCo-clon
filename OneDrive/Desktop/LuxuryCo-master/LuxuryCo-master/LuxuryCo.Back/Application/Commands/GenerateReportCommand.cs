using MediatR;
using System;

namespace LuxuryCo.Back.Application.Commands;

public class GenerateReportCommand : IRequest<string>
{
    public int AdminId { get; set; }
    public string ReportType { get; set; }
    public string Parameters { get; set; }
}
