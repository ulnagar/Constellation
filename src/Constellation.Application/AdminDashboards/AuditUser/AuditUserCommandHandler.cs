namespace Constellation.Application.AdminDashboards.AuditUser;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Identity;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AuditUserCommandHandler
    : ICommandHandler<AuditUserCommand>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolContactRepository _schoolContactRepository;
    private readonly IStudentFamilyRepository _studentFamilyRepository;

    public AuditUserCommandHandler(
        UserManager<AppUser> userManager,
        IStaffRepository staffRepository,
        ISchoolContactRepository schoolContactRepository,
        IStudentFamilyRepository studentFamilyRepository)
    {
        _userManager = userManager;
        _staffRepository = staffRepository;
        _schoolContactRepository = schoolContactRepository;
        _studentFamilyRepository = studentFamilyRepository;
    }

    public async Task<Result> Handle(AuditUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (user is null)
        {
            return Result.Failure(DomainErrors.Auth.UserNotFound);
        }

        var staffMember = await _staffRepository.FromEmailForExistCheck(user.Email);

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

        var contact = await _schoolContactRepository.FromEmailForExistCheck(user.Email);

        if (contact is null)
        {
            user.IsSchoolContact = false;
            user.SchoolContactId = 0;
        }
        else
        {
            user.IsSchoolContact = true;
            user.SchoolContactId = contact.Id;
        }

        var family = await _studentFamilyRepository.DoesEmailBelongToParentOrFamily(user.Email, cancellationToken);

        user.IsParent = family;

        await _userManager.UpdateAsync(user);

        return Result.Success();
    }
}
