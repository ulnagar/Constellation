namespace Constellation.Application.Domains.SchoolContacts.Events.SchoolContactDeleted;

using Abstractions.Messaging;
using Application.Models.Identity;
using Constellation.Application.Models.Identity.Enums;
using Core.Errors;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Enums;
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
        SchoolContact? contact = await _contactRepository.GetById(notification.ContactId, cancellationToken);

        if (contact is null)
        {
            _logger
                .ForContext(nameof(SchoolContactDeletedDomainEvent), notification, true)
                .ForContext(nameof(Error), SchoolContactErrors.NotFound(notification.ContactId), true)
                .Warning("Failed to update AppUser for deleted School Contact");

            return;
        }

        AppUser? user = await _userManager.FindByEmailAsync(contact.EmailAddress.Email);

        if (user is null)
        {
            _logger
                .ForContext(nameof(SchoolContactDeletedDomainEvent), notification, true)
                .ForContext(nameof(contact.EmailAddress), contact.EmailAddress)
                .ForContext(nameof(Error), DomainErrors.Auth.UserNotFound, true)
                .Warning("Failed to update AppUser for deleted School Contact");

            return;
        }

        // Check if user has any other links first
        AppUserLink? exactLink = user.Links.FirstOrDefault(link => 
            !link.IsDeleted && 
            link.Type == LinkType.Contact && 
            link.LinkId == contact.Id.Value);

        if (exactLink is not null)
        {
            exactLink.Delete();

            IdentityResult updateLink = await _userManager.UpdateAsync(user);

            if (!updateLink.Succeeded)
            {
                _logger
                    .ForContext(nameof(SchoolContactDeletedDomainEvent), notification, true)
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(IdentityResult.Errors), updateLink.Errors, true)
                    .Warning("Failed to update AppUser for deleted School Contact");

                return;
            }
        }

        if (user.Links.All(link => link.IsDeleted))
        {
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
        else if (user.Links.Where(link => link.Type == LinkType.Contact).All(link => link.IsDeleted))
        {
            List<Position> roles = Position.GetOptions.ToList();

            foreach (Position role in roles)
                await _userManager.RemoveFromRoleAsync(user, role.Value);
        }
    }
}
