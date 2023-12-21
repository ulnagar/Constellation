namespace Constellation.Application.Families.Events;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Identity;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Families.Events;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ParentRemovedFromFamilyDomainEvent_RemoveUser
    : IDomainEventHandler<ParentRemovedFromFamilyDomainEvent>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public ParentRemovedFromFamilyDomainEvent_RemoveUser(
        IFamilyRepository familyRepository,
        IStaffRepository staffRepository,
        ISchoolContactRepository contactRepository,
        UserManager<AppUser> userManager,
        Serilog.ILogger logger)
    {
        _familyRepository = familyRepository;
        _staffRepository = staffRepository;
        _contactRepository = contactRepository;
        _userManager = userManager;
        _logger = logger.ForContext<ParentRemovedFromFamilyDomainEvent>();
    }

    public async Task Handle(ParentRemovedFromFamilyDomainEvent notification, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(notification.EmailAddress);

        if (existingUser is null)
            return;

        int otherParents = await _familyRepository.CountOfParentsWithEmailAddress(notification.EmailAddress, cancellationToken);

        if (otherParents == 0)
        {
            existingUser.IsParent = false;
        }

        var staffMember = await _staffRepository.GetCurrentByEmailAddress(notification.EmailAddress, cancellationToken);

        if (staffMember is null)
        {
            existingUser.IsStaffMember = false;
            existingUser.StaffId = null;
        }

        var schoolContact = await _contactRepository.GetWithRolesByEmailAddress(notification.EmailAddress, cancellationToken);

        if (schoolContact is null)
        {
            existingUser.IsSchoolContact = false;
            existingUser.SchoolContactId = 0;
        }

        if (!existingUser.IsSchoolContact && !existingUser.IsStaffMember && !existingUser.IsParent)
        {
            var deleteResult = await _userManager.DeleteAsync(existingUser);

            if (!deleteResult.Succeeded)
            {
                _logger.Warning(
                    "EID {eid}: Could not delete old user {uid} while attempting to update parent {pid} in family {fid}",
                    notification.Id.ToString(),
                    existingUser.Id.ToString(),
                    notification.EmailAddress,
                    notification.FamilyId.ToString());

                foreach (var error in deleteResult.Errors)
                {
                    _logger.Warning(
                        "EID {eid}: Failed with error {error}",
                        notification.Id.ToString(),
                        error);
                }
            }
        }
        else
        {
            await _userManager.UpdateAsync(existingUser);
        }
    }
}
