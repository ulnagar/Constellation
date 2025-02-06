namespace Constellation.Application.Families.Events.FamilyDeletedDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Models.Identity;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.Families.Events;
using Core.Errors;
using Core.Models.Families.Errors;
using Core.Shared;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveUserAccount
    : IDomainEventHandler<FamilyDeletedDomainEvent>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public RemoveUserAccount(
        IFamilyRepository familyRepository,
        UserManager<AppUser> userManager,
        ILogger logger)
    {
        _familyRepository = familyRepository;
        _userManager = userManager;
        _logger = logger.ForContext<FamilyDeletedDomainEvent>();
    }

    public async Task Handle(FamilyDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        Family family = await _familyRepository.GetFamilyById(notification.FamilyId, cancellationToken);

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

        AppUser existingUser = await _userManager.FindByEmailAsync(family.FamilyEmail);

        if (existingUser is null)
            return;

        if (existingUser.IsSchoolContact || existingUser.IsStaffMember)
            return;

        IdentityResult result = await _userManager.DeleteAsync(existingUser);

        if (result.Succeeded)
            return;

        _logger
            .ForContext(nameof(FamilyDeletedDomainEvent), notification, true)
            .ForContext(nameof(Error), result.Errors, true)
            .Warning("Failed to remove user account for deleted family");
    }
}
