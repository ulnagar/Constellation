namespace Constellation.Application.Domains.Families.Events;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Models.Identity;
using Constellation.Application.Models.Identity.Enums;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Families.Events;
using Core.Models.Families;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ParentAddedToFamilyDomainEvent_CreateUser
    : IDomainEventHandler<ParentAddedToFamilyDomainEvent>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public ParentAddedToFamilyDomainEvent_CreateUser(
        IFamilyRepository familyRepository,
        UserManager<AppUser> userManager,
        ILogger logger)
    {
        _familyRepository = familyRepository;
        _userManager = userManager;
        _logger = logger.ForContext<ParentAddedToFamilyDomainEvent>();
    }

    public async Task Handle(ParentAddedToFamilyDomainEvent notification, CancellationToken cancellationToken)
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

        AppUser? existingUser = await _userManager.FindByEmailAsync(parent.EmailAddress);

        if (existingUser is not null)
        {
            bool existingLink = existingUser.Links.Any(link =>
                !link.IsDeleted && link.Type == LinkType.Parent && link.LinkId == parent.Id.Value);

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

        AppUser user = new AppUser
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
