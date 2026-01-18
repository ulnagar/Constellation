namespace Constellation.Application.Domains.Auth.Events.AppIdentityCodeUpdatedIntegrationEvent;

using Abstractions.Messaging;
using Commands.AuditAllUsers;
using Core.IntegrationEvents;
using Core.ValueObjects;
using Interfaces.Configuration;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Models.Identity;
using System.Threading.Tasks;

internal sealed class RemediateUsers
: IIntegrationEventHandler<AppIdentityCodeUpdatedIntegrationEvent>
{
    private readonly ISender _mediator;
    private readonly UserManager<AppUser> _userManager;
    private readonly AppConfiguration _configuration;

    public RemediateUsers(
        ISender mediator,
        UserManager<AppUser> userManager,
        IOptions<AppConfiguration> configuration)
    {
        _mediator = mediator;
        _userManager = userManager;
        _configuration = configuration.Value;
    }

    public async Task Handle(AppIdentityCodeUpdatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        // Delete all users
        foreach (var user in _userManager.Users)
            await _userManager.DeleteAsync(user);

        // Recreate users
        await _mediator.Send(new AuditAllUsersCommand(), cancellationToken);

        // Ensure that the master admin user has permissions
        EmailAddress adminEmail = _configuration.AdminUser;

        AppUser? adminUser = await _userManager.FindByEmailAsync(adminEmail.ToString());
        if (adminUser is not null)
        {
            bool isInRole = await _userManager.IsInRoleAsync(adminUser, AppRole.SuperAdminRole);
            if (!isInRole)
                await _userManager.AddToRoleAsync(adminUser, AppRole.SuperAdminRole);
        }
    }
}
