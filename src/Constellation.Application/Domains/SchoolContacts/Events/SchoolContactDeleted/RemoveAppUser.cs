namespace Constellation.Application.Domains.SchoolContacts.Events.SchoolContactDeleted;

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

internal sealed class RemoveAppUser
    : IDomainEventHandler<SchoolContactDeletedDomainEvent>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public RemoveAppUser(
        ISchoolContactRepository contactRepository,
        UserManager<AppUser> userManager,
        ILogger logger)
    {
        _contactRepository = contactRepository;
        _userManager = userManager;
        _logger = logger.ForContext<SchoolContactDeletedDomainEvent>();
    }

    public async Task Handle(SchoolContactDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        SchoolContact contact = await _contactRepository.GetById(notification.ContactId, cancellationToken);

        if (contact is null)
        {
            _logger
                .ForContext(nameof(SchoolContactDeletedDomainEvent), notification, true)
                .ForContext(nameof(Error), SchoolContactErrors.NotFound(notification.ContactId), true)
                .Warning("Failed to delete AppUser for deleted School Contact");

            return;
        }

        AppUser user = await _userManager.FindByEmailAsync(contact.EmailAddress);

        if (user is null)
        {
            _logger
                .ForContext(nameof(SchoolContactDeletedDomainEvent), notification, true)
                .ForContext(nameof(contact.EmailAddress), contact.EmailAddress)
                .ForContext(nameof(Error), DomainErrors.Auth.UserNotFound, true)
                .Warning("Failed to delete AppUser for deleted School Contact");

            return;
        }

        IdentityResult update = await _userManager.DeleteAsync(user);

        if (!update.Succeeded)
        {
            _logger
                .ForContext(nameof(SchoolContactDeletedDomainEvent), notification, true)
                .ForContext(nameof(AppUser), user, true)
                .ForContext(nameof(IdentityResult.Errors), update.Errors, true)
                .Warning("FFailed to delete AppUser for deleted School Contact");
        }
    }
}
