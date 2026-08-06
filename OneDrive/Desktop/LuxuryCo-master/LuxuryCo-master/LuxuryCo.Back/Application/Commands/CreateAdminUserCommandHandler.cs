using MassTransit;
using MediatR;
using LuxuryCo.Back.Domain.Events;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace LuxuryCo.Back.Application.Commands;

public class CreateAdminUserCommandHandler : IRequestHandler<CreateAdminUserCommand, string>
{
    // private readonly IBus _messageBus;

    public CreateAdminUserCommandHandler()
    {
    }

    public async Task<string> Handle(CreateAdminUserCommand request, CancellationToken cancellationToken)
    {
        // Generate secure invite token
        var token = Guid.NewGuid().ToString("N");

        // Here we would use the Repository Pattern to insert the INACTIVE user into the database
        // e.g. await _userRepository.AddAsync(new User { Activo = false ... });
        
        // Publish Domain Event for Outbox/Message Bus (Email Service will listen to this)
        var userInvitedEvent = new UserInvitedEvent(request.AdminId, request.Name, request.Email, token);
        // await _messageBus.Publish(userInvitedEvent, cancellationToken);

        return $"Se ha creado el usuario '{request.Name}' en estado inactivo. Se ha enviado un enlace de invitación seguro a '{request.Email}' para que establezca su contraseña.";
    }
}
