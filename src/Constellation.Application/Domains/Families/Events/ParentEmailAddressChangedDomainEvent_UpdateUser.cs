namespace Constellation.Application.Domains.Families.Events;

using Abstractions.Messaging;
using Application.Models.Identity.Enums;
using Constellation.Application.Models.Identity;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.Families.Events;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ParentEmailAddressChangedDomainEvent_UpdateUser
    : IDomainEventHandler<ParentEmailAddressChangedDomainEvent>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public ParentEmailAddressChangedDomainEvent_UpdateUser(
        IFamilyRepository familyRepository,
        UserManager<AppUser> userManager,
        ILogger logger)
    {
        _familyRepository = familyRepository;
        _userManager = userManager;
        _logger = logger.ForContext<ParentEmailAddressChangedDomainEvent>();
    }

    public async Task Handle(ParentEmailAddressChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        Family? family = await _familyRepository.GetFamilyById(notification.FamilyId, cancellationToken);

        if (family is null)
        {
            _logger.Warning(
                "EID {eid}: Could not find family {fid} when attempting to create new user for parent {pid}",
                notification.Id.ToString(),
                notification.FamilyId.ToString(),
                notification.ParentId.ToString());

            return;
        }

        Parent? parent = family.Parents.FirstOrDefault(entry => entry.Id == notification.ParentId);

        if (parent is null)
        {
            _logger.Warning(
                "EID {eid}: Could not find parent {pid} in family {fid} when attempting to create new user for parent",
                notification.Id.ToString(),
                notification.ParentId.ToString(),
                notification.FamilyId.ToString());

            return;
        }

        // If there is an AppUser with the old email address, update their properties to reflect the new state
        AppUser? oldUser = await _userManager.FindByEmailAsync(notification.OldEmail);

        if (oldUser is not null)
        {
            int otherParents = await _familyRepository.CountOfParentsWithEmailAddress(notification.OldEmail, cancellationToken);

            if (otherParents == 0)
            {
                IEnumerable<AppUserLink> links = oldUser.Links.Where(link => link.Type == LinkType.Parent && !link.IsDeleted);

                foreach (AppUserLink link in links)
                    link.Delete();

                await _userManager.UpdateAsync(oldUser);
            }

            if (oldUser.Links.All(link => link.IsDeleted))
            {
                await _userManager.DeleteAsync(oldUser);
            }
        }

        // Is there already a registered user with this email address?
        AppUser? existingUser = await _userManager.FindByEmailAsync(notification.NewEmail);

        if (existingUser is not null)
        {
            bool existingLink = existingUser.Links.Any(link => !link.IsDeleted && link.Type == LinkType.Parent && link.LinkId == parent.Id.Value);

            if (existingLink)
                return;

            existingUser.AddParentLink(parent.Id);

            IdentityResult updateResult = await _userManager.UpdateAsync(existingUser);

            if (updateResult.Succeeded)
                return;

            foreach (IdentityError error in updateResult.Errors)
            {
                _logger.Warning(
                    "EID {eid}: Could not update user for parent {pid} in family {fid} due to error {error}",
                    notification.Id.ToString(),
                    notification.ParentId.ToString(),
                    notification.FamilyId.ToString(),
                    error);
            }

            return;
        }

        AppUser user = new()
        {
            UserName = parent.EmailAddress,
            Email = parent.EmailAddress,
            Name = parent.Name
        };

        user.AddParentLink(parent.Id);

        IdentityResult result = await _userManager.CreateAsync(user);

        if (result.Succeeded)
            return;

        foreach (IdentityError error in result.Errors)
        {
            _logger.Warning(
                "EID {eid}: Could not create user for parent {pid} in family {fid} due to error {error}",
                notification.Id.ToString(),
                notification.ParentId.ToString(),
                notification.FamilyId.ToString(),
                error);
        }
    }
}
