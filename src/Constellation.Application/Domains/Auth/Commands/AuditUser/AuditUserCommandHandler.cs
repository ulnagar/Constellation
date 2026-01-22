namespace Constellation.Application.Domains.Auth.Commands.AuditUser;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Errors;
using Core.Models.Families;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Models.Identity;
using Models.Identity.Enums;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AuditUserCommandHandler
    : ICommandHandler<AuditUserCommand>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolContactRepository _schoolContactRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly IStudentRepository _studentRepository;

    public AuditUserCommandHandler(
        UserManager<AppUser> userManager,
        IStaffRepository staffRepository,
        ISchoolContactRepository schoolContactRepository,
        IFamilyRepository familyRepository,
        IStudentRepository studentRepository)
    {
        _userManager = userManager;
        _staffRepository = staffRepository;
        _schoolContactRepository = schoolContactRepository;
        _familyRepository = familyRepository;
        _studentRepository = studentRepository;
    }

    public async Task<Result> Handle(AuditUserCommand request, CancellationToken cancellationToken)
    {
        AppUser? user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (user is null)
            return Result.Failure(DomainErrors.Auth.UserNotFound);

        EmailAddress emailAddress = EmailAddress.FromValue(user.Email!);

        StaffMember? staffMember = await _staffRepository.GetCurrentByEmailAddress(emailAddress, cancellationToken);

        if (staffMember is null)
        {
            List<AppUserLink> links = user.Links.Where(link => !link.IsDeleted && link.Type == LinkType.Staff).ToList();

            foreach (AppUserLink link in links)
                link.Delete();
        } 
        else
        {
            List<AppUserLink> links = user.Links.Where(link => !link.IsDeleted && link.Type == LinkType.Staff).ToList();

            if (links.All(link => link.LinkId != staffMember.Id.Value))
                user.AddStaffLink(staffMember.Id);
        }

        SchoolContact? contact = await _schoolContactRepository.GetWithRolesByEmailAddress(emailAddress, cancellationToken);

        if (contact is null)
        {
            List<AppUserLink> links = user.Links.Where(link => !link.IsDeleted && link.Type == LinkType.Contact).ToList();

            foreach (AppUserLink link in links)
                link.Delete();
        }
        else
        {
            List<AppUserLink> links = user.Links.Where(link => !link.IsDeleted && link.Type == LinkType.Contact).ToList();

            if (links.All(link => link.LinkId != contact.Id.Value))
                user.AddContactLink(contact.Id);

            foreach (SchoolContactRole assignment in contact.Assignments.Where(assignment => !assignment.IsDeleted))
                await _userManager.AddToRoleAsync(user, assignment.Role.Value);
        }

        Family? family = await _familyRepository.GetFamilyByEmail(emailAddress, cancellationToken);

        if (family is null)
        {
            List<AppUserLink> links = user.Links.Where(link => !link.IsDeleted && link.Type == LinkType.Family).ToList();

            foreach (AppUserLink link in links)
                link.Delete();
        }
        else
        {
            List<AppUserLink> links = user.Links.Where(link => !link.IsDeleted && link.Type == LinkType.Family).ToList();

            if (links.All(link => link.LinkId != family.Id.Value))
                user.AddFamilyLink(family.Id);
        }

        Parent? parent = await _familyRepository.GetParentByEmail(emailAddress, cancellationToken);

        if (parent is null)
        {
            List<AppUserLink> links = user.Links.Where(link => !link.IsDeleted && link.Type == LinkType.Parent).ToList();

            foreach (AppUserLink link in links)
                link.Delete();
        }
        else
        {
            List<AppUserLink> links = user.Links.Where(link => !link.IsDeleted && link.Type == LinkType.Parent).ToList();

            if (links.All(link => link.LinkId != parent.Id.Value))
                user.AddParentLink(parent.Id);
        }

        Student? student = await _studentRepository.GetCurrentByEmailAddress(emailAddress, cancellationToken);

        if (student is null)
        {
            List<AppUserLink> links = user.Links.Where(link => !link.IsDeleted && link.Type == LinkType.Student).ToList();

            foreach (AppUserLink link in links)
                link.Delete();
        }
        else
        {
            List<AppUserLink> links = user.Links.Where(link => !link.IsDeleted && link.Type == LinkType.Student).ToList();

            if (links.All(link => link.LinkId != student.Id.Value))
                user.AddStudentLink(student.Id);
        }

        await _userManager.UpdateAsync(user);

        return Result.Success();
    }
}
