namespace Constellation.Application.Domains.Families.Events.ParentEmailAddressChanged;

using Abstractions.Messaging;
using Application.Models.Identity.Enums;
using Constellation.Application.Models.Identity;
using Constellation.Application.Models.Identity.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.Families.Events;
using Core.Models.Auth;
using Core.Models.Auth.Enums;
using Core.Models.Families.Errors;
using Core.Shared;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateOrUpdateUserAccount
    : IDomainEventHandler<ParentEmailAddressChangedDomainEvent>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IIdentityRepository _identityRepository;
    private readonly ILogger _logger;

    public CreateOrUpdateUserAccount(
        IFamilyRepository familyRepository,
        UserManager<AppUser> userManager,
        IIdentityRepository identityRepository,
        ILogger logger)
    {
        _familyRepository = familyRepository;
        _userManager = userManager;
        _identityRepository = identityRepository;
        _logger = logger.ForContext<ParentEmailAddressChangedDomainEvent>();
    }

    public async Task Handle(ParentEmailAddressChangedDomainEvent notification, CancellationToken cancellationToken)
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

        AppUser? user = await _userManager.FindByEmailAsync(notification.OldEmail);

        List<AppRole> oldUserRoles = [];

        if (user is not null)
        {
            AppUserLink? link = user.Links
                .FirstOrDefault(link =>
                    !link.IsDeleted &&
                    link.Type == LinkType.Parent &&
                    link.LinkId == parent.Id.Value);

            if (link is not null)
                link.Delete();

            await _userManager.UpdateAsync(user);

            if (user.Links.Where(link => link.Type == LinkType.Family || link.Type == LinkType.Parent).All(link => link.IsDeleted))
            {
                List<AppRole> roles = await _identityRepository.GetRolesForUser(user, cancellationToken);

                foreach (AppRole role in roles.Where(role => role.Type == AppRoleType.Parent))
                {
                    await _userManager.RemoveFromRoleAsync(user, role.Name);

                    oldUserRoles.Add(role);
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
        }

        user = await _userManager.FindByEmailAsync(notification.NewEmail);

        if (user is null)
        {
            user = new()
            {
                UserName = parent.EmailAddress, 
                Email = parent.EmailAddress, 
                Name = parent.Name
            };

            IdentityResult result = await _userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                _logger
                    .ForContext(nameof(ParentEmailAddressChangedDomainEvent), notification, true)
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(IdentityResult.Errors), result.Errors, true)
                    .Warning("Failed to update Parent AppUser");

                return;
            }
        }

        List<AppUserLink> links = user.Links.Where(link => !link.IsDeleted && link.Type == LinkType.Parent).ToList();

        if (links.All(link => link.LinkId != parent.Id.Value))
        {
            user.AddParentLink(parent.Id);

            IdentityResult update = await _userManager.UpdateAsync(user);

            if (!update.Succeeded)
            {
                _logger
                    .ForContext(nameof(ParentEmailAddressChangedDomainEvent), notification, true)
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(IdentityResult.Errors), update.Errors, true)
                    .Warning("Failed to update Parent AppUser");

                return;
            }
        }

        await _userManager.AddToRoleAsync(user, AppRole.Parent);

        foreach (AppRole role in oldUserRoles)
            await _userManager.AddToRoleAsync(user, role.Name);
    }
}
