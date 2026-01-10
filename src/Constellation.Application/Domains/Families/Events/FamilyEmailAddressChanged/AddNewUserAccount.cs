namespace Constellation.Application.Domains.Families.Events.FamilyEmailAddressChanged;

using Abstractions.Messaging;
using Constellation.Application.Models.Identity;
using Constellation.Application.Models.Identity.Enums;
using Constellation.Core.Abstractions.Repositories;
using Core.Models.Families;
using Core.Models.Families.Errors;
using Core.Models.Families.Events;
using Core.Shared;
using Core.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddNewUserAccount
: IDomainEventHandler<FamilyEmailAddressChangedDomainEvent>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public AddNewUserAccount(
        IFamilyRepository familyRepository,
        UserManager<AppUser> userManager,
        ILogger logger)
    {
        _familyRepository = familyRepository;
        _userManager = userManager;
        _logger = logger.ForContext<FamilyEmailAddressChangedDomainEvent>();
    }

    public async Task Handle(FamilyEmailAddressChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        Family? family = await _familyRepository.GetFamilyById(notification.FamilyId, cancellationToken);

        if (family is null)
        {
            _logger
                .ForContext(nameof(FamilyEmailAddressChangedDomainEvent), notification, true)
                .ForContext(nameof(Error), FamilyErrors.NotFound(notification.FamilyId), true)
                .Warning("Failed to create new user account for changed family email address");

            return;
        }

        AppUser? oldUser = await _userManager.FindByEmailAsync(family.FamilyEmail);

        if (oldUser is not null)
        {
            int otherParents = await _familyRepository.CountOfParentsWithEmailAddress(notification.OldEmail, cancellationToken);

            if (otherParents == 0)
            {
                IEnumerable<AppUserLink> links =
                    oldUser.Links.Where(link => link.Type == LinkType.Family && !link.IsDeleted);

                foreach (AppUserLink link in links)
                    link.Delete();

                await _userManager.UpdateAsync(oldUser);
            }

            if (oldUser.Links.All(link => link.IsDeleted))
            {
                await _userManager.DeleteAsync(oldUser);
            }
        }

        AppUser? existingUser = await _userManager.FindByEmailAsync(notification.NewEmail);

        if (existingUser is not null)
        {
            bool existingLink = existingUser.Links.Any(link =>
                !link.IsDeleted && link.Type == LinkType.Family && link.LinkId == family.Id.Value);

            if (existingLink)
                return;

            existingUser.AddFamilyLink(family.Id);

            IdentityResult updateResult = await _userManager.UpdateAsync(existingUser);

            if (updateResult.Succeeded)
                return;

            foreach (IdentityError error in updateResult.Errors)
            {
                _logger
                    .ForContext(nameof(FamilyEmailAddressChangedDomainEvent), notification, true)
                    .ForContext(nameof(Error), error, true)
                    .Warning("Failed to create new user account for changed family email address");
            }

            return;
        }
        
        Result<Name> name = Name.Create(family.FamilyTitle);

        if (name.IsFailure)
        {
            _logger
                .ForContext(nameof(FamilyEmailAddressChangedDomainEvent), notification, true)
                .ForContext(nameof(Error), name.Error, true)
                .Warning("Failed to create new user account for changed family email address");

            return;
        }

        AppUser user = new()
        {
            UserName = family.FamilyEmail,
            Email = family.FamilyEmail,
            Name = name.Value
        };

        user.AddFamilyLink(family.Id);

        IdentityResult result = await _userManager.CreateAsync(user);

        if (result.Succeeded)
            return;

        _logger
            .ForContext(nameof(FamilyEmailAddressChangedDomainEvent), notification, true)
            .ForContext(nameof(Error), result.Errors, true)
            .Warning("Failed to create new user account for changed family email address");
    }
}
