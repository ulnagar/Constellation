namespace Constellation.Application.Domains.Families.Events;

using Abstractions.Messaging;
using Constellation.Application.Models.Identity;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.Families.Events;
using Core.Models;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ParentEmailAddressChangedDomainEvent_UpdateUser
    : IDomainEventHandler<ParentEmailAddressChangedDomainEvent>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public ParentEmailAddressChangedDomainEvent_UpdateUser(
        IFamilyRepository familyRepository,
        IStaffRepository staffRepository,
        ISchoolContactRepository contactRepository,
        UserManager<AppUser> userManager,
        ILogger logger)
    {
        _familyRepository = familyRepository;
        _staffRepository = staffRepository;
        _contactRepository = contactRepository;
        _userManager = userManager;
        _logger = logger.ForContext<ParentEmailAddressChangedDomainEvent>();
    }

    public async Task Handle(ParentEmailAddressChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        Family family = await _familyRepository.GetFamilyById(notification.FamilyId, cancellationToken);

        if (family is null)
        {
            _logger.Warning(
                "EID {eid}: Could not find family {fid} when attempting to create new user for parent {pid}",
                notification.Id.ToString(),
                notification.FamilyId.ToString(),
                notification.ParentId.ToString());

            return;
        }

        Parent parent = family.Parents.FirstOrDefault(entry => entry.Id == notification.ParentId);

        if (parent is null)
        {
            _logger.Warning(
                "EID {eid}: Could not find parent {pid} in family {fid} when attempting to create new user for parent",
                notification.Id.ToString(),
                notification.ParentId.ToString(),
                notification.FamilyId.ToString());

            return;
        }

        // If there is an AppUser with the old email address, update their properties to reflect the new state
        AppUser oldUser = await _userManager.FindByEmailAsync(notification.OldEmail);

        if (oldUser is not null)
        {
            int otherParents = await _familyRepository.CountOfParentsWithEmailAddress(notification.OldEmail, cancellationToken);

            if (otherParents == 0)
            {
                oldUser.IsParent = false;
            }

            EmailAddress oldEmailAddress = EmailAddress.FromValue(notification.OldEmail);

            StaffMember staffMember = await _staffRepository.GetCurrentByEmailAddress(oldEmailAddress, cancellationToken);

            if (staffMember is null)
            {
                oldUser.IsStaffMember = false;
                oldUser.StaffId = StaffId.Empty;
            }

            SchoolContact schoolContact = await _contactRepository.GetWithRolesByEmailAddress(notification.OldEmail, cancellationToken);

            if (schoolContact is null)
            {
                oldUser.IsSchoolContact = false;
            }

            if (!oldUser.IsSchoolContact && !oldUser.IsStaffMember && !oldUser.IsParent)
            {
                IdentityResult deleteResult = await _userManager.DeleteAsync(oldUser);

                if (!deleteResult.Succeeded)
                {
                    _logger.Warning(
                        "EID {eid}: Could not delete old user {uid} while attempting to update parent {pid} in family {fid}",
                        notification.Id.ToString(),
                        oldUser.Id.ToString(),
                        notification.ParentId.ToString(),
                        notification.FamilyId.ToString());

                    foreach (IdentityError error in deleteResult.Errors)
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
                await _userManager.UpdateAsync(oldUser);
            }
        }

        // Is there already a registered user with this email address?
        AppUser existingUser = await _userManager.FindByEmailAsync(notification.NewEmail);

        if (existingUser is not null)
        {
            existingUser.IsParent = true;
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

        AppUser user = new()
        {
            UserName = parent.EmailAddress,
            Email = parent.EmailAddress,
            FirstName = parent.FirstName,
            LastName = parent.LastName,
            IsParent = true
        };

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
