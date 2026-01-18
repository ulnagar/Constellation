namespace Constellation.Application.Domains.Families.Events.FamilyEmailAddressChanged;

using Abstractions.Messaging;
using Application.Models.Identity.Repositories;
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

internal sealed class CreateOrUpdateUserAccount
    : IDomainEventHandler<FamilyEmailAddressChangedDomainEvent>
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

        AppUser? user = await _userManager.FindByEmailAsync(notification.OldEmail);

        List<AppRole> oldUserRoles = [];

        if (user is not null)
        {
            AppUserLink? link = user.Links
                .FirstOrDefault(link =>
                    !link.IsDeleted &&
                    link.Type == LinkType.Family &&
                    link.LinkId == family.Id.Value);

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
                        .ForContext(nameof(FamilyEmailAddressChangedDomainEvent), notification, true)
                        .ForContext(nameof(AppUser), user, true)
                        .ForContext(nameof(IdentityResult.Errors), update.Errors, true)
                        .Warning("Failed to update Family AppUser email address");
                }
            }
        }

        user = await _userManager.FindByEmailAsync(notification.NewEmail);

        if (user is null)
        {
            Result<Name> name = Name.Create(family.FamilyTitle);

            if (name.IsFailure)
            {
                _logger
                    .ForContext(nameof(FamilyEmailAddressChangedDomainEvent), notification, true)
                    .ForContext(nameof(Error), name.Error, true)
                    .Warning("Failed to create new user account for changed family email address");

                return;
            }

            user = new()
            {
                UserName = family.FamilyEmail, 
                Email = family.FamilyEmail, 
                Name = name.Value
            };

            IdentityResult result = await _userManager.CreateAsync(user);

            if (result.Succeeded)
            {
                _logger
                    .ForContext(nameof(FamilyEmailAddressChangedDomainEvent), notification, true)
                    .ForContext(nameof(Error), result.Errors, true)
                    .Warning("Failed to create new user account for changed family email address");

            }
        }

        List<AppUserLink> links = user.Links.Where(link => !link.IsDeleted && link.Type == LinkType.Family).ToList();

        if (links.All(link => link.LinkId != family.Id.Value))
        {
            user.AddFamilyLink(family.Id);

            IdentityResult update = await _userManager.UpdateAsync(user);

            if (!update.Succeeded)
            {
                _logger
                    .ForContext(nameof(FamilyEmailAddressChangedDomainEvent), notification, true)
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(IdentityResult.Errors), update.Errors, true)
                    .Warning("Failed to update Family AppUser email address");

                return;
            }
        }

        await _userManager.AddToRoleAsync(user, AppRole.Parent);

        foreach (AppRole role in oldUserRoles)
            await _userManager.AddToRoleAsync(user, role.Name);
    }
}