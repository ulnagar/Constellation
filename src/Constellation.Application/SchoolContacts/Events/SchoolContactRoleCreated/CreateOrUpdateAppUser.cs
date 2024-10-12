namespace Constellation.Application.SchoolContacts.Events.SchoolContactRoleCreated;

using Abstractions.Messaging;
using Application.Models.Auth;
using Application.Models.Identity;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Core.Models.SchoolContacts.Errors;
using Constellation.Core.Models.SchoolContacts.Events;
using Constellation.Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateOrUpdateAppUser 
    : IDomainEventHandler<SchoolContactRoleCreatedDomainEvent>
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
        _logger = logger.ForContext<SchoolContactRoleCreatedDomainEvent>();
    }

    public async Task Handle(SchoolContactRoleCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        SchoolContact contact = await _contactRepository.GetById(notification.ContactId, cancellationToken);

        if (contact is null)
        {
            _logger
                .ForContext(nameof(SchoolContactRoleCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), SchoolContactErrors.NotFound(notification.ContactId), true)
                .Warning("Failed to create new School Contact AppUser");

            return;
        }

        AppUser user = await _userManager.FindByEmailAsync(contact.EmailAddress);

        if (user is not null)
        {
            user.IsSchoolContact = true;
            user.SchoolContactId = contact.Id;

            IdentityResult update = await _userManager.UpdateAsync(user);

            if (!update.Succeeded)
            {
                _logger
                    .ForContext(nameof(SchoolContactRoleCreatedDomainEvent), notification, true)
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(IdentityResult.Errors), update.Errors, true)
                    .Warning("Failed to update School Contact AppUser");

                return;
            }
                
            IdentityResult addRole = await _userManager.AddToRoleAsync(user, AuthRoles.SchoolContact);

            if (!addRole.Succeeded)
            {
                _logger
                    .ForContext(nameof(SchoolContactRoleCreatedDomainEvent), notification, true)
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(IdentityResult.Errors), addRole.Errors, true)
                    .Warning("Failed to update School Contact AppUser");
            }

            return;
        }

        user = new()
        {
            UserName = contact.EmailAddress,
            Email = contact.EmailAddress,
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            IsSchoolContact = true,
            SchoolContactId = contact.Id
        };

        IdentityResult create = await _userManager.CreateAsync(user);

        if (create.Succeeded)
            return;

        _logger
            .ForContext(nameof(SchoolContactRoleCreatedDomainEvent), notification, true)
            .ForContext(nameof(AppUser), user, true)
            .ForContext(nameof(IdentityResult.Errors), create.Errors, true)
            .Warning("Failed to create new School Contact AppUser");

        IdentityResult addNewRole = await _userManager.AddToRoleAsync(user, AuthRoles.SchoolContact);

        if (!addNewRole.Succeeded)
        {
            _logger
                .ForContext(nameof(SchoolContactRoleCreatedDomainEvent), notification, true)
                .ForContext(nameof(AppUser), user, true)
                .ForContext(nameof(IdentityResult.Errors), addNewRole.Errors, true)
                .Warning("Failed to create new School Contact AppUser");
        }
    }
}