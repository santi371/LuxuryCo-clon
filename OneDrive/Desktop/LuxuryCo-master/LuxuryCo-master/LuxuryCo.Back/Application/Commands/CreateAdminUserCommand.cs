using MediatR;

namespace LuxuryCo.Back.Application.Commands;

public class CreateAdminUserCommand : IRequest<string>
{
    public int AdminId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
}
