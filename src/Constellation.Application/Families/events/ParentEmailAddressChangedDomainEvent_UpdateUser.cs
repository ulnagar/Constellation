namespace Constellation.Application.Families.Events;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Identity;
using Constellation.Core.Abstractions;
using Constellation.Core.DomainEvents;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ParentEmailAddressChangedDomainEvent_UpdateUser
    : IDomainEventHandler<ParentEmailAddressChangedDomainEvent>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IParentRepository _parentRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public ParentEmailAddressChangedDomainEvent_UpdateUser(
        IFamilyRepository familyRepository,
        IParentRepository parentRepository,
        IStaffRepository staffRepository,
        ISchoolContactRepository contactRepository,
        UserManager<AppUser> userManager,
        Serilog.ILogger logger)
    {
        _familyRepository = familyRepository;
        _parentRepository = parentRepository;
        _staffRepository = staffRepository;
        _contactRepository = contactRepository;
        _userManager = userManager;
        _logger = logger.ForContext<ParentEmailAddressChangedDomainEvent>();
    }

    public async Task Handle(ParentEmailAddressChangedDomainEvent notification, CancellationToken cancellationToken)
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

        var oldUser = await _userManager.FindByEmailAsync(notification.OldEmail);

        var otherParents = await _parentRepository.GetParentsByEmail(notification.OldEmail, cancellationToken);

        if (otherParents is null || otherParents.Count == 0)
        {
            oldUser.IsParent = false;
        }

        var staffMember = await _staffRepository.GetByEmailAddress(notification.OldEmail, cancellationToken);

        if (staffMember is null)
        {
            oldUser.IsStaffMember = false;
            oldUser.StaffId = null;
        }

        var schoolContact = await _contactRepository.GetWithRolesByEmailAddress(notification.OldEmail, cancellationToken);

        if (schoolContact is null)
        {
            oldUser.IsSchoolContact = false;
            oldUser.SchoolContactId = 0;
        }

        if (!oldUser.IsSchoolContact && !oldUser.IsStaffMember && !oldUser.IsParent)
        {
            var deleteResult = await _userManager.DeleteAsync(oldUser);

            if (!deleteResult.Succeeded)
            {
                _logger.Warning(
                    "EID {eid}: Could not delete old user {uid} while attempting to update parent {pid} in family {fid}",
                    notification.Id.ToString(),
                    oldUser.Id.ToString(),
                    notification.ParentId.ToString(),
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
            await _userManager.UpdateAsync(oldUser);
        }

        var existingUser = await _userManager.FindByEmailAsync(notification.NewEmail);

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
