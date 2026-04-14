namespace Constellation.Application.Domains.SchoolContacts.Events.SchoolContactRoleDeleted;

using Abstractions.Messaging;
using Constellation.Application.Models.Identity;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Core.Models.SchoolContacts.Enums;
using Constellation.Core.Models.SchoolContacts.Errors;
using Constellation.Core.Models.SchoolContacts.Repositories;
using Constellation.Core.Shared;
using Core.Models.Auth;
using Core.Models.SchoolContacts.Events;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Threading.Tasks;

internal sealed class UpdateAppUser 
: IDomainEventHandler<SchoolContactRoleDeletedDomainEvent>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public UpdateAppUser(
        ISchoolContactRepository contactRepository,
        UserManager<AppUser> userManager,
        ILogger logger)
    {
        _contactRepository = contactRepository;
        _userManager = userManager;
        _logger = logger
            .ForContext<SchoolContactRoleDeletedDomainEvent>();
    }
    public async Task Handle(SchoolContactRoleDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        SchoolContact? contact = await _contactRepository.GetById(notification.ContactId, cancellationToken);

        if (contact is null)
        {
            _logger
                .ForContext(nameof(SchoolContactRoleDeletedDomainEvent), notification, true)
                .ForContext(nameof(Error), SchoolContactErrors.NotFound(notification.ContactId), true)
                .Warning("Failed to update School Contact AppUser");

            return;
        }

        AppUser? user = await _userManager.FindByEmailAsync(contact.EmailAddress.Email);

        if (user is null)
        {
            _logger
                .ForContext(nameof(SchoolContactRoleDeletedDomainEvent), notification, true)
                .ForContext(nameof(Error), SchoolContactErrors.NotFound(notification.ContactId), true)
                .Warning("Failed to update School Contact AppUser");

            return;
        }

        // Reset all Contact AppRoles
        List<Position> possibleRoles = Position.GetOptions.ToList();

        foreach (Position role in possibleRoles)
            await _userManager.RemoveFromRoleAsync(user, role.Value);

        // Add to current Contact AppRoles
        List<SchoolContactRole> roles = contact.Assignments
            .Where(role => !role.IsDeleted)
            .ToList();

        foreach (SchoolContactRole role in roles)
            await _userManager.AddToRoleAsync(user, role.Role.Value);
    }
}
