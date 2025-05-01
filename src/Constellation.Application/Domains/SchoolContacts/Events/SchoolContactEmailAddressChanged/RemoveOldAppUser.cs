namespace Constellation.Application.Domains.SchoolContacts.Events.SchoolContactEmailAddressChanged;

using Abstractions.Messaging;
using Application.Models.Identity;
using Core.Errors;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Errors;
using Core.Models.SchoolContacts.Events;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveOldAppUser
    : IDomainEventHandler<SchoolContactEmailAddressChangedDomainEvent>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public RemoveOldAppUser(
        ISchoolContactRepository contactRepository,
        UserManager<AppUser> userManager,
        ILogger logger)
    {
        _contactRepository = contactRepository;
        _userManager = userManager;
        _logger = logger.ForContext<SchoolContactEmailAddressChangedDomainEvent>();
    }

    public async Task Handle(SchoolContactEmailAddressChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        SchoolContact contact = await _contactRepository.GetById(notification.ContactId, cancellationToken);

        if (contact is null)
        {
            _logger
                .ForContext(nameof(SchoolContactEmailAddressChangedDomainEvent), notification, true)
                .ForContext(nameof(Error), SchoolContactErrors.NotFound(notification.ContactId), true)
                .Warning("Failed to delete old School Contact AppUser");

            return;
        }

        AppUser user = await _userManager.FindByEmailAsync(notification.OldEmailAddress);

        if (user is null)
        {
            _logger
                .ForContext(nameof(SchoolContactEmailAddressChangedDomainEvent), notification, true)
                .ForContext("OldEmailAddress", notification.OldEmailAddress)
                .ForContext(nameof(Error), DomainErrors.Auth.UserNotFound, true)
                .Warning("Failed to delete old School Contact AppUser");

            return;
        }

        IdentityResult update = await _userManager.DeleteAsync(user);

        if (!update.Succeeded)
        {
            _logger
                .ForContext(nameof(SchoolContactEmailAddressChangedDomainEvent), notification, true)
                .ForContext(nameof(AppUser), user, true)
                .ForContext(nameof(IdentityResult.Errors), update.Errors, true)
                .Warning("Failed to delete old School Contact AppUser");
        }
    }
}
