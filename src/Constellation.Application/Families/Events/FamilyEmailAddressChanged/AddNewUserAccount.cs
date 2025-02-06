namespace Constellation.Application.Families.Events.FamilyEmailAddressChanged;

using Abstractions.Messaging;
using Constellation.Application.Models.Identity;
using Constellation.Core.Abstractions.Repositories;
using Core.Errors;
using Core.Models.Families;
using Core.Models.Families.Errors;
using Core.Models.Families.Events;
using Core.Shared;
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
        Family family = await _familyRepository.GetFamilyById(notification.FamilyId, cancellationToken);

        if (family is null)
        {
            _logger
                .ForContext(nameof(FamilyEmailAddressChangedDomainEvent), notification, true)
                .ForContext(nameof(Error), FamilyErrors.NotFound(notification.FamilyId), true)
                .Warning("Failed to create new user account for changed family email address");

            return;
        }

        AppUser existingUser = await _userManager.FindByEmailAsync(family.FamilyEmail);

        if (existingUser is not null)
        {
            existingUser.IsParent = true;
            IdentityResult updateResult = await _userManager.UpdateAsync(existingUser);

            if (updateResult.Succeeded)
                return;

            _logger
                .ForContext(nameof(FamilyEmailAddressChangedDomainEvent), notification, true)
                .ForContext(nameof(Error), updateResult.Errors, true)
                .Warning("Failed to create new user account for changed family email address");

            return;
        }

        AppUser user = new()
        {
            UserName = family.FamilyEmail,
            Email = family.FamilyEmail,
            FirstName = string.Empty,
            LastName = family.FamilyTitle,
            IsParent = true
        };

        IdentityResult result = await _userManager.CreateAsync(user);

        if (result.Succeeded)
            return;

        _logger
            .ForContext(nameof(FamilyEmailAddressChangedDomainEvent), notification, true)
            .ForContext(nameof(Error), result.Errors, true)
            .Warning("Failed to create new user account for changed family email address");

        return;
    }
}
