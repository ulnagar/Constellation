namespace Constellation.Application.Families.Events;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Models.Identity;
using Constellation.Core.Abstractions;
using Constellation.Core.DomainEvents;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ParentAddedtoFamilyDomainEvent_CreateUser
    : IDomainEventHandler<ParentAddedToFamilyDomainEvent>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public ParentAddedtoFamilyDomainEvent_CreateUser(
        IFamilyRepository familyRepository,
        UserManager<AppUser> userManager,
        Serilog.ILogger logger)
    {
        _familyRepository = familyRepository;
        _userManager = userManager;
        _logger = logger.ForContext<ParentAddedToFamilyDomainEvent>();
    }

    public async Task Handle(ParentAddedToFamilyDomainEvent notification, CancellationToken cancellationToken)
    {
        var family = await _familyRepository.GetFamilyById(notification.FamilyId, cancellationToken);

        if (family is null)
        {
            _logger.Warning(
                "EID {eid}: Could not find family {fid} when attempting to create new user for parent {pid}",
                notification.Id.ToString(),
                notification.FamilyId.ToString(),
                notification.ParentId.ToString());

            return;
        }

        var parent = family.Parents.FirstOrDefault(entry => entry.Id == notification.ParentId);

        if (parent is null)
        {
            _logger.Warning(
                "EID {eid}: Could not find parent {pid} in family {fid} when attempting to create new user for parent",
                notification.Id.ToString(),
                notification.ParentId.ToString(),
                notification.FamilyId.ToString());

            return;
        }

        var existingUser = await _userManager.FindByEmailAsync(parent.EmailAddress);

        if (existingUser is not null)
        {
            existingUser.IsParent = true;
            var updateResult = await _userManager.UpdateAsync(existingUser);

            if (updateResult.Succeeded)
                return;

            foreach (var error in updateResult.Errors)
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

        var user = new AppUser
        {
            UserName = parent.EmailAddress,
            Email = parent.EmailAddress,
            FirstName = parent.FirstName,
            LastName = parent.LastName,
            IsParent = true
        };

        var result = await _userManager.CreateAsync(user);

        if (result.Succeeded)
            return;

        foreach (var error in result.Errors)
        {
            _logger.Warning(
                "EID {eid}: Could not create user for parent {pid} in family {fid} due to error {error}",
                notification.Id.ToString(),
                notification.ParentId.ToString(),
                notification.FamilyId.ToString(),
                error);
        }

        return;
    }
}
