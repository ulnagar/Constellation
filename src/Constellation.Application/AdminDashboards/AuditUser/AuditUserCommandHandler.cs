namespace Constellation.Application.AdminDashboards.AuditUser;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using Core.Models;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.StaffMembers.Repositories;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AuditUserCommandHandler
    : ICommandHandler<AuditUserCommand>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolContactRepository _schoolContactRepository;
    private readonly IFamilyRepository _studentFamilyRepository;

    public AuditUserCommandHandler(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        IStaffRepository staffRepository,
        ISchoolContactRepository schoolContactRepository,
        IFamilyRepository studentFamilyRepository)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _staffRepository = staffRepository;
        _schoolContactRepository = schoolContactRepository;
        _studentFamilyRepository = studentFamilyRepository;
    }

    public async Task<Result> Handle(AuditUserCommand request, CancellationToken cancellationToken)
    {
        AppUser user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (user is null)
            return Result.Failure(DomainErrors.Auth.UserNotFound);

        Staff staffMember = await _staffRepository.FromEmailForExistCheck(user.Email);

        if (staffMember is null)
        {
            user.IsStaffMember = false;
            user.StaffId = string.Empty;
        } 
        else
        {
            user.IsStaffMember = true;
            user.StaffId = staffMember.StaffId;
        }

        SchoolContact contact = await _schoolContactRepository.GetWithRolesByEmailAddress(user.Email, cancellationToken);

        if (contact is null)
        {
            user.IsSchoolContact = false;
        }
        else
        {
            user.IsSchoolContact = true;
            user.SchoolContactId = contact.Id;

            await _userManager.AddToRoleAsync(user, AuthRoles.SchoolContact);
        }

        bool family = await _studentFamilyRepository.DoesEmailBelongToParentOrFamily(user.Email, cancellationToken);

        user.IsParent = family;

        await _userManager.UpdateAsync(user);

        return Result.Success();
    }
}
