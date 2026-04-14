namespace Constellation.Application.Domains.Families.Events.ParentRemovedFromFamily;

using Abstractions.Messaging;
using Constellation.Application.Models.Identity;
using Constellation.Application.Models.Identity.Enums;
using Constellation.Application.Models.Identity.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.Families.Errors;
using Constellation.Core.Models.Families.Events;
using Core.Models.Auth;
using Core.Models.Auth.Enums;
using Core.Shared;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveAppUser
    : IDomainEventHandler<ParentRemovedFromFamilyDomainEvent>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IIdentityRepository _identityRepository;
    private readonly ILogger _logger;

    public RemoveAppUser(
        IFamilyRepository familyRepository,
        UserManager<AppUser> userManager,
        IIdentityRepository identityRepository,
        ILogger logger)
    {
        _familyRepository = familyRepository;
        _userManager = userManager;
        _identityRepository = identityRepository;
        _logger = logger
            .ForContext<ParentRemovedFromFamilyDomainEvent>();
    }

    public async Task Handle(ParentRemovedFromFamilyDomainEvent notification, CancellationToken cancellationToken)
    {
        Family? family = await _familyRepository.GetFamilyById(notification.FamilyId, cancellationToken);

        if (family is null)
        {
            _logger
                .ForContext(nameof(ParentEmailAddressChangedDomainEvent), notification, true)
                .ForContext(nameof(Error), FamilyErrors.NotFound(notification.FamilyId), true)
                .Warning("Failed to update Parent AppUser");

            return;
        }

        Parent? parent = family.Parents.FirstOrDefault(entry => entry.Id == notification.ParentId);

        if (parent is null)
        {
            _logger
                .ForContext(nameof(ParentEmailAddressChangedDomainEvent), notification, true)
                .ForContext(nameof(Family), family, true)
                .ForContext(nameof(Error), ParentErrors.NotFoundInFamily(notification.ParentId, family.Id), true)
                .Warning("Failed to update Parent AppUser");

            return;
        }

        AppUser? user = await _userManager.FindByEmailAsync(parent.EmailAddress.Email);

        if (user is null)
        {
            _logger
                .ForContext(nameof(ParentEmailAddressChangedDomainEvent), notification, true)
                .ForContext(nameof(Family), family, true)
                .ForContext(nameof(Parent), parent, true)
                .ForContext(nameof(Error), DomainErrors.Auth.UserNotFound, true)
                .Warning("Failed to update Parent AppUser");

            return;
        }

        // Check if user has any other links first
        AppUserLink? exactLink = user.Links.FirstOrDefault(link =>
            !link.IsDeleted &&
            link.Type == LinkType.Parent &&
            link.LinkId == parent.Id.Value);

        if (exactLink is not null)
        {
            exactLink.Delete();

            IdentityResult updateLink = await _userManager.UpdateAsync(user);

            if (!updateLink.Succeeded)
            {
                _logger
                    .ForContext(nameof(ParentEmailAddressChangedDomainEvent), notification, true)
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(IdentityResult.Errors), updateLink.Errors, true)
                    .Warning("Failed to update Parent AppUser");

                return;
            }
        }

        if (user.Links.All(link => link.IsDeleted))
        {
            IdentityResult update = await _userManager.DeleteAsync(user);

            if (!update.Succeeded)
            {
                _logger
                    .ForContext(nameof(ParentEmailAddressChangedDomainEvent), notification, true)
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(IdentityResult.Errors), update.Errors, true)
                    .Warning("Failed to update Parent AppUser");
            }
        }
        else if (user.Links.Where(link => link.Type == LinkType.Parent || link.Type == LinkType.Family).All(link => link.IsDeleted))
        {
            List<AppRole> roles = await _identityRepository.GetRolesForUser(user, cancellationToken);

            foreach (var role in roles.Where(role => role.Type == AppRoleType.Parent))
                await _userManager.RemoveFromRoleAsync(user, role.Name);
        }
    }
}
