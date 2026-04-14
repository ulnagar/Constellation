namespace Constellation.Application.Domains.SchoolContacts.Events.SchoolContactEmailAddressChanged;

using Abstractions.Messaging;
using Application.Models.Auth;
using Application.Models.Identity;
using Application.Models.Identity.Enums;
using Core.Models.Auth;
using Core.Models.Auth.Enums;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Errors;
using Core.Models.SchoolContacts.Events;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateOrUpdateAppUser
: IDomainEventHandler<SchoolContactEmailAddressChangedDomainEvent>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public CreateOrUpdateAppUser(
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
        SchoolContact? contact = await _contactRepository.GetById(notification.ContactId, cancellationToken);

        if (contact is null)
        {
            _logger
                .ForContext(nameof(SchoolContactEmailAddressChangedDomainEvent), notification, true)
                .ForContext(nameof(Error), SchoolContactErrors.NotFound(notification.ContactId), true)
                .Warning("Failed to create new School Contact AppUser");
            return;
        }

        AppUser? user = await _userManager.FindByEmailAsync(contact.EmailAddress.Email);

        if (user is not null)
        {
            List<AppUserLink> links = user.Links.Where(link => !link.IsDeleted && link.Type == LinkType.Contact).ToList();

            if (links.All(link => link.LinkId != contact.Id.Value))
            {
                user.AddContactLink(contact.Id);

                IdentityResult update = await _userManager.UpdateAsync(user);

                if (!update.Succeeded)
                {
                    _logger
                        .ForContext(nameof(SchoolContactEmailAddressChangedDomainEvent), notification, true)
                        .ForContext(nameof(AppUser), user, true)
                        .ForContext(nameof(IdentityResult.Errors), update.Errors, true)
                        .Warning("Failed to update School Contact AppUser");

                    return;
                }
            }
        }
        else
        {
            user = new()
            {
                UserName = contact.EmailAddress.Email,
                Email = contact.EmailAddress.Email,
                Name = contact.Name
            };

            user.AddContactLink(contact.Id);

            IdentityResult create = await _userManager.CreateAsync(user);

            if (!create.Succeeded)
            {
                _logger
                    .ForContext(nameof(SchoolContactEmailAddressChangedDomainEvent), notification, true)
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(IdentityResult.Errors), create.Errors, true)
                    .Warning("Failed to create new School Contact AppUser");

                return;
            }
        }
            
        List<SchoolContactRole> roles = contact.Assignments
            .Where(role => !role.IsDeleted)
            .ToList();

        foreach (SchoolContactRole role in roles)
            await _userManager.AddToRoleAsync(user, role.Role.Value);
    }
}
