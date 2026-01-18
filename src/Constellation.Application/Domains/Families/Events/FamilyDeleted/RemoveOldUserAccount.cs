namespace Constellation.Application.Domains.Families.Events.FamilyDeleted;

using Abstractions.Messaging;
using Application.Models.Identity;
using Application.Models.Identity.Enums;
using Application.Models.Identity.Repositories;
using Core.Abstractions.Repositories;
using Core.Errors;
using Core.Models.Families;
using Core.Models.Families.Errors;
using Core.Models.Families.Events;
using Core.Shared;
using Microsoft.AspNetCore.Identity;
using Serilog;

internal sealed class RemoveUserAccount
    : IDomainEventHandler<FamilyDeletedDomainEvent>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IIdentityRepository _identityRepository;
    private readonly ILogger _logger;

    public RemoveUserAccount(
        IFamilyRepository familyRepository,
        UserManager<AppUser> userManager,
        IIdentityRepository identityRepository,
        ILogger logger)
    {
        _familyRepository = familyRepository;
        _userManager = userManager;
        _identityRepository = identityRepository;
        _logger = logger.ForContext<FamilyDeletedDomainEvent>();
    }

    public async Task Handle(FamilyDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        Family? family = await _familyRepository.GetFamilyById(notification.FamilyId, cancellationToken);

        if (family is null)
        {
            _logger
                .ForContext(nameof(FamilyDeletedDomainEvent), notification, true)
                .ForContext(nameof(Error), FamilyErrors.NotFound(notification.FamilyId), true)
                .Warning("Failed to remove user account for deleted family");

            return;
        }

        bool otherUser = await _familyRepository.DoesEmailBelongToParentOrFamily(family.FamilyEmail, cancellationToken);

        if (otherUser)
            return;

        AppUser? user = await _userManager.FindByEmailAsync(family.FamilyEmail);

        if (user is null)
        {
            _logger
                .ForContext(nameof(FamilyDeletedDomainEvent), notification, true)
                .ForContext(nameof(Error), DomainErrors.Auth.UserNotFound, true)
                .Warning("Failed to remove user account for deleted family");

            return;
        }

        AppUserLink? link = user.Links
            .FirstOrDefault(link =>
                !link.IsDeleted &&
                link.Type == LinkType.Family &&
                link.LinkId == family.Id.Value);

        if (link is not null)
            link.Delete();

        await _userManager.UpdateAsync(user);

        if (user.Links.All(link => link.IsDeleted))
        {
            IdentityResult update = await _userManager.DeleteAsync(user);

            if (!update.Succeeded)
            {
                _logger
                    .ForContext(nameof(FamilyDeletedDomainEvent), notification, true)
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(IdentityResult.Errors), update.Errors, true)
                    .Warning("Failed to remove user account for deleted family");
            }

            return;
        }

        if (user.Links.Where(link => link.Type == LinkType.Family || link.Type == LinkType.Parent)
            .All(link => link.IsDeleted))
        {
            List<AppRole> roles = await _identityRepository.GetRolesForUser(user, cancellationToken);

            foreach (var role in roles.Where(role => role.Type == AppRoleType.Parent))
                await _userManager.RemoveFromRoleAsync(user, role.Name);
        }
    }
}
