namespace Constellation.Application.Domains.Families.Events.ParentAddedToFamily;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Models.Identity;
using Constellation.Application.Models.Identity.Enums;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Families.Events;
using Core.Models.Auth;
using Core.Models.Auth.Enums;
using Core.Models.Families;
using Core.Models.Families.Errors;
using Core.Shared;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateOrUpdateUserAccount
    : IDomainEventHandler<ParentAddedToFamilyDomainEvent>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public CreateOrUpdateUserAccount(
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
            _logger
                .ForContext(nameof(ParentAddedToFamilyDomainEvent), notification, true)
                .ForContext(nameof(Error), FamilyErrors.NotFound(notification.FamilyId), true)
                .Warning("Failed to create AppUser for Parent");

            return;
        }

        Parent? parent = family.Parents.FirstOrDefault(entry => entry.Id == notification.ParentId);

        if (parent is null)
        {
            _logger
                .ForContext(nameof(ParentAddedToFamilyDomainEvent), notification, true)
                .ForContext(nameof(Error), ParentErrors.NotFoundInFamily(notification.ParentId, notification.FamilyId), true)
                .Warning("Failed to create AppUser for Parent");

            return;
        }

        AppUser? user = await _userManager.FindByEmailAsync(parent.EmailAddress);

        if (user is null)
        {
            user = new AppUser { UserName = parent.EmailAddress, Email = parent.EmailAddress, Name = parent.Name };

            IdentityResult result = await _userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                _logger
                    .ForContext(nameof(ParentAddedToFamilyDomainEvent), notification, true)
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(IdentityResult.Errors), result.Errors, true)
                    .Warning("Failed to create AppUser for Parent");

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
                    .ForContext(nameof(ParentAddedToFamilyDomainEvent), notification, true)
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(IdentityResult.Errors), update.Errors, true)
                    .Warning("Failed to create AppUser for Parent");

                return;
            }
        }

        await _userManager.AddToRoleAsync(user, AppRole.Parent);
    }
}
