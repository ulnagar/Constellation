namespace Constellation.Application.Domains.Auth.Commands.AuditAllUsers;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Models.Families;
using Core.Models.Identifiers;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Identifiers;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using Models.Identity;
using Models.Identity.Enums;
using Models.Identity.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AuditAllUsersCommandHandler
: ICommandHandler<AuditAllUsersCommand>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IIdentityRepository _identityRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AuditAllUsersCommandHandler(
        IFamilyRepository familyRepository, 
        IStaffRepository staffRepository,
        ISchoolContactRepository contactRepository,
        IStudentRepository studentRepository,
        IIdentityRepository identityRepository,
        UserManager<AppUser> userManager,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _familyRepository = familyRepository;
        _staffRepository = staffRepository;
        _contactRepository = contactRepository;
        _studentRepository = studentRepository;
        _identityRepository = identityRepository;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<AuditAllUsersCommand>();
    }

    public async Task<Result> Handle(AuditAllUsersCommand request, CancellationToken cancellationToken)
    {
        _logger.Information("Starting scan of users");
        
        //List<AppUser> users = await _identityRepository.GetUsers(cancellationToken);
        List<AppUser> users = _userManager.Users.ToList();

        _logger.Information("Found {count} users currently registered", users.Count);

        List<Family> families = await _familyRepository.GetAllCurrent(cancellationToken);

        _logger.Information("Found {count} families currently registered", families.Count);
        
        List<Parent> parents = families
            .SelectMany(family => family.Parents)
            .ToList();

        _logger.Information("Found {count} parents currently registered", parents.Count);
        
        List<StaffMember> staff = await _staffRepository.GetAllActive(cancellationToken);

        _logger.Information("Found {count} staff members currently registered", staff.Count);
        
        List<SchoolContact> contacts = await _contactRepository.GetAllActive(cancellationToken);

        _logger.Information("Found {count} school contacts currently registered", contacts.Count);
        
        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);

        _logger.Information("Found {count} students currently registered", students.Count);

        foreach (Family family in families)
        {
            _logger
                .ForContext(nameof(Family), family, true)
                .Information("Checking Family {name}", family.FamilyTitle);

            AppUser? existingUser = users.FirstOrDefault(user => string.Equals(user.Email!, family.FamilyEmail, StringComparison.OrdinalIgnoreCase));

            if (existingUser is null)
            {
                AppUser? newUser = await CreateUserFromFamily(family);

                if (newUser is not null)
                    users.Add(newUser);
            }
            else
            {
                _logger.Information("User found.");

                await CheckFamilyUserDetails(existingUser, family);
            }
        }

        foreach (Parent parent in parents)
        {
            _logger
                .ForContext(nameof(Parent), parent, true)
                .Information("Checking parent {name}", parent.Name);

            AppUser? existingUser = users.FirstOrDefault(user => user.Email == parent.EmailAddress);

            if (existingUser is null)
            {
                AppUser? newUser = await CreateUserFromParent(parent);

                if (newUser is not null)
                    users.Add(newUser);
            }
            else
            {
                _logger.Information("User found.");

                await CheckParentUserDetails(existingUser, parent);
            }
        }

        foreach (SchoolContact contact in contacts)
        {
            if (contact.Assignments.All(role => role.IsDeleted))
                continue;

            _logger
                .ForContext(nameof(SchoolContact), contact, true)
                .Information("Checking school contact {name}", contact.Name.DisplayName);

            AppUser? existingUser = users.FirstOrDefault(user => user.Email == contact.EmailAddress.Email);

            if (existingUser is null)
            {
                AppUser? newUser = await CreateUserFromContact(contact);

                if (newUser is not null)
                    users.Add(newUser);
            }
            else
            {
                _logger.Information("User found.");

                await CheckContactUserDetails(existingUser, contact);
            }
        }

        foreach (StaffMember member in staff)
        {
            _logger
                .ForContext(nameof(StaffMember), member, true)
                .Information("Checking staff member {name}", $"{member.Name.FirstName} {member.Name.LastName}");

            AppUser? existingUser = users.FirstOrDefault(user => user.Email == member.EmailAddress.Email);

            if (existingUser is null)
            {
                AppUser? newUser = await CreateUserFromStaffMember(member);

                if (newUser is not null)
                    users.Add(newUser);
            }
            else
            {
                _logger.Information("User found.");

                await CheckStaffUserDetails(existingUser, member);
            }
        }

        foreach (Student student in students)
        {
            if (student.IsDeleted)
                continue;

            if (student.EmailAddress == EmailAddress.None)
                continue;

            _logger
                .ForContext(nameof(Student), student, true)
                .Information("Checking student {name}", student.Name.DisplayName);

            AppUser? existingUser = users.FirstOrDefault(user => user.Email == student.EmailAddress.Email);

            if (existingUser is null)
            {
                AppUser? newUser = await CreateUserFromStudent(student);

                if (newUser is not null)
                    users.Add(newUser);
            }
            else
            {
                _logger.Information("User found.");

                await CheckStudentUserDetails(existingUser, student);
            }
        }

        _logger.Information("Finished processing potential users");

        users = await _identityRepository.GetUsers(cancellationToken);

        _logger.Information("{count} total users now registered", users.Count);

        foreach (AppUser user in users)
        {
            _logger
                .ForContext(nameof(AppUser), user, true)
                .Information("Checking user {email}", user.Email);

            List<Family> matchingFamilies = families
                .Where(family => family.FamilyEmail == user.Email)
                .ToList();

            List<Parent> matchingParents = parents
                .Where(parent => parent.EmailAddress == user.Email)
                .ToList();

            StaffMember? matchingStaff = staff
                .FirstOrDefault(member => member.EmailAddress.Email == user.Email);
            
            SchoolContact? contact = contacts.FirstOrDefault(contact => contact.EmailAddress.Email == user.Email);

            Student? student = students.FirstOrDefault(student => student.EmailAddress.Email == user.Email);

            if (matchingParents.Count == 0 &&
                matchingFamilies.Count == 0 &&
                matchingStaff is null &&
                contact is null &&
                student is null)
            {
                // User is not linked to any known account!

                _logger.Information("Found no matching user types.");
                _logger
                    .ForContext(nameof(AppUser), user, true)
                    .Information("User will be deleted");

                await _identityRepository.DeleteUser(user);
                await _unitOfWork.CompleteAsync(cancellationToken);
            }
        }

        _logger.Information("Finished processing registered users");

        users = await _identityRepository.GetUsers(cancellationToken);

        _logger.Information("{count} registered users remaining", users.Count);
        
        return Result.Success();
    }

    private async Task<AppUser?> CreateUserFromFamily(Family family)
    {
        AppUser? user = await CreateUser(
            family.FamilyEmail,
            string.Empty,
            family.FamilyTitle,
            familyId: family.Id);

        if (user is not null)
        {
            IdentityResult addRole = await _identityRepository.AddUserToRole(user, AppRole.Parent);

            if (!addRole.Succeeded)
            {
                _logger
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(AppRole), AppRole.Parent)
                    .ForContext(nameof(Error), addRole.Errors, true)
                    .Warning("Failed to add user to Role");
            }
        }

        return user;
    }

    private async Task<AppUser?> CreateUserFromParent(Parent parent)
    {
        AppUser? user = await CreateUser(
            parent.EmailAddress,
            parent.Name.FirstName,
            parent.Name.LastName,
            parentId: parent.Id);

        if (user is not null)
        {
            IdentityResult addRole = await _identityRepository.AddUserToRole(user, AppRole.Parent);

            if (!addRole.Succeeded)
            {
                _logger
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(AppRole), AppRole.Parent)
                    .ForContext(nameof(Error), addRole.Errors, true)
                    .Warning("Failed to add user to Role");
            }
        }

        return user;
    }

    private async Task<AppUser?> CreateUserFromStaffMember(StaffMember staffMember)
    {
        AppUser? user = await CreateUser(
            staffMember.EmailAddress.Email,
            staffMember.Name.FirstName,
            staffMember.Name.LastName,
            staffId: staffMember.Id);

        if (user is not null)
        {
            IdentityResult addRole = await _identityRepository.AddUserToRole(user, AppRole.Staff);

            if (!addRole.Succeeded)
            {
                _logger
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(AppRole), AppRole.Staff)
                    .ForContext(nameof(Error), addRole.Errors, true)
                    .Warning("Failed to add user to Role");
            }
        }

        return user;
    }

    private async Task<AppUser?> CreateUserFromStudent(Student student)
    {
        AppUser? user = await CreateUser(
            student.EmailAddress.Email,
            student.Name.PreferredName,
            student.Name.LastName,
            studentId: student.Id);

        if (user is not null)
        {
            IdentityResult addRole = await _identityRepository.AddUserToRole(user, AppRole.Student);

            if (!addRole.Succeeded)
            {
                _logger
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(AppRole), AppRole.Student)
                    .ForContext(nameof(Error), addRole.Errors, true)
                    .Warning("Failed to add user to Role");
            }
        }

        return user;
    }

    private async Task<AppUser?> CreateUserFromContact(SchoolContact contact)
    {
        List<string> roles = contact.Assignments
            .Where(role => !role.IsDeleted)
            .Select(role => role.Role.Value)
            .Distinct()
            .ToList();

        AppUser? user = await CreateUser(
            contact.EmailAddress.Email,
            contact.Name.FirstName,
            contact.Name.LastName,
            contactId: contact.Id);

        if (user is not null)
        {
            foreach (string role in roles)
            {
                IdentityResult addRole = await _identityRepository.AddUserToRole(user, role);

                if (addRole.Succeeded)
                    continue;

                _logger
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(AppRole), role)
                    .ForContext(nameof(Error), addRole.Errors, true)
                    .Warning("Failed to add user to Role");
            }
        }

        return user;
    }

    private async Task<AppUser?> CreateUser(
        string email,
        string firstName,
        string lastName,
        StaffId? staffId = null,
        SchoolContactId? contactId = null,
        StudentId? studentId = null,
        ParentId? parentId = null,
        FamilyId? familyId = null)
    {
        _logger.Information("Found no matching user.");
        _logger.Information("User will be created");

        Result<Name> name = string.IsNullOrEmpty(firstName) 
            ? Name.Create(lastName)
            : Name.Create(firstName, string.Empty, lastName);

        if (name.IsFailure)
        {
            _logger
                .Warning("Failed to create user for email {email}", email);

            return null;
        }

        AppUser user = new()
        {
            UserName = email,
            Email = email,
            Name = name.Value
        };

        AppUser? result = await _identityRepository.CreateUser(user);

        if (result is null)
        {
            _logger
                .ForContext("Request", user, true)
                .Warning("Failed to create user due to error");

            return null;
        }

        if (parentId.HasValue)
            result.AddParentLink(parentId.Value);

        if (familyId.HasValue)
            result.AddFamilyLink(familyId.Value);

        if (staffId.HasValue)
            result.AddStaffLink(staffId.Value);

        if (contactId.HasValue)
            result.AddContactLink(contactId.Value);

        if (studentId.HasValue)
            result.AddStudentLink(studentId.Value);
        
        await _unitOfWork.CompleteAsync();

        return result;
    }

    private async Task CheckFamilyUserDetails(AppUser user, Family family)
    {
        if (user.Name.DisplayName != family.FamilyTitle)
        {
            _logger.Information("Updating Name to {name}", family.FamilyTitle);

            Result<Name> name = Name.Create(family.FamilyTitle);

            if (name.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), name.Error, true)
                    .Warning("Failed to update Name to {name}", family.FamilyTitle);
            }

            user.Name = name.Value;
        }

        if (!user.Links.Any(link => !link.IsDeleted && link.Type == LinkType.Family))
        {
            _logger.Information("Updating IsFamily to {isFamily}", true);

            user.AddFamilyLink(family.Id);
        }

        if (user.Links.Where(link => !link.IsDeleted && link.Type == LinkType.Family).All(link => link.LinkId != family.Id.Value))
        {
            _logger.Information("Updating FamilyId to {familyId}", family.Id);

            user.AddFamilyLink(family.Id);
        }

        IdentityResult addRole = await _identityRepository.AddUserToRole(user, AppRole.Parent);

        if (!addRole.Succeeded)
        {
            _logger
                .ForContext(nameof(AppUser), user, true)
                .ForContext(nameof(AppRole), AppRole.Parent)
                .ForContext(nameof(Error), addRole.Errors, true)
                .Warning("Failed to add user to Role");
        }

        await _unitOfWork.CompleteAsync();
    }

    private async Task CheckParentUserDetails(AppUser user, Parent parent)
    {
        if (user.Name != parent.Name.FirstName)
        {
            _logger.Information("Updating Name to {Name}", parent.Name.DisplayName);

            user.Name = parent.Name;
        }

        if (!user.Links.Any(link => !link.IsDeleted && link.Type == LinkType.Parent))
        {
            _logger.Information("Updating IsParent to {isParent}", true);

            user.AddParentLink(parent.Id);
        }

        if (user.Links.Where(link => !link.IsDeleted && link.Type == LinkType.Parent).All(link => link.LinkId != parent.Id.Value))
        {
            _logger.Information("Updating ParentId to {parentId}", parent.Id);

            user.AddParentLink(parent.Id);
        }

        IdentityResult addRole = await _identityRepository.AddUserToRole(user, AppRole.Parent);

        if (!addRole.Succeeded)
        {
            _logger
                .ForContext(nameof(AppUser), user, true)
                .ForContext(nameof(AppRole), AppRole.Parent)
                .ForContext(nameof(Error), addRole.Errors, true)
                .Warning("Failed to add user to Role");
        }

        await _unitOfWork.CompleteAsync();
    }

    private async Task CheckStaffUserDetails(AppUser user, StaffMember staffMember)
    {
        if (user.Name != staffMember.Name)
        {
            _logger.Information("Updating Name to {Name}", staffMember.Name.DisplayName);

            user.Name = staffMember.Name;
        }

        if (!user.Links.Any(link => !link.IsDeleted && link.Type == LinkType.Staff))
        {
            _logger.Information("Updating IsStaff to {isStaff}", true);

            user.AddStaffLink(staffMember.Id);
        }

        if (user.Links.Where(link => !link.IsDeleted && link.Type == LinkType.Staff).All(link => link.LinkId != staffMember.Id.Value))
        {
            _logger.Information("Updating StaffId to {staffId}", staffMember.Id);

            user.AddStaffLink(staffMember.Id);
        }
        
        IdentityResult addRole = await _identityRepository.AddUserToRole(user, AppRole.Staff);

        if (!addRole.Succeeded)
        {
            _logger
                .ForContext(nameof(AppUser), user, true)
                .ForContext(nameof(AppRole), AppRole.Staff)
                .ForContext(nameof(Error), addRole.Errors, true)
                .Warning("Failed to add user to Role");
        }

        await _unitOfWork.CompleteAsync();
    }

    private async Task CheckContactUserDetails(AppUser user, SchoolContact contact)
    {
        if (user.Name != contact.Name)
        {
            _logger.Information("Updating Name to {Name}", contact.Name.DisplayName);

            user.Name = contact.Name;
        }
        
        if (!user.Links.Any(link => !link.IsDeleted && link.Type == LinkType.Contact))
        {
            _logger.Information("Updating IsContact to {isContact}", true);

            user.AddContactLink(contact.Id);
        }

        if (user.Links.Where(link => !link.IsDeleted && link.Type == LinkType.Contact).All(link => link.LinkId != contact.Id.Value))
        {
            _logger.Information("Updating ContactId to {contactId}", contact.Id);

            user.AddContactLink(contact.Id);
        }

        List<string> roles = contact.Assignments
            .Where(role => !role.IsDeleted)
            .Select(role => role.Role.Value)
            .Distinct()
            .ToList();

        foreach (string role in roles)
        {
            IdentityResult addRole = await _identityRepository.AddUserToRole(user, role);

            if (addRole.Succeeded)
                continue;

            _logger
                .ForContext(nameof(AppUser), user, true)
                .ForContext(nameof(AppRole), role)
                .ForContext(nameof(Error), addRole.Errors, true)
                .Warning("Failed to add user to Role");
        }

        await _unitOfWork.CompleteAsync();
    }

    private async Task CheckStudentUserDetails(AppUser user, Student student)
    {
        if (user.Name != student.Name)
        {
            _logger.Information("Updating Name to {Name}", student.Name);

            user.Name = student.Name;
        }

        if (!user.Links.Any(link => !link.IsDeleted && link.Type == LinkType.Student))
        {
            _logger.Information("Updating IsStudent to {isStudent}", true);

            user.AddStudentLink(student.Id);
        }

        if (user.Links.Where(link => !link.IsDeleted && link.Type == LinkType.Student).All(link => link.LinkId != student.Id.Value))
        {
            _logger.Information("Updating StudentId to {studentId}", student.Id);

            user.AddStudentLink(student.Id);
        }

        IdentityResult addRole = await _identityRepository.AddUserToRole(user, AppRole.Student);

        if (!addRole.Succeeded)
        {
            _logger
                .ForContext(nameof(AppUser), user, true)
                .ForContext(nameof(AppRole), AppRole.Student)
                .ForContext(nameof(Error), addRole.Errors, true)
                .Warning("Failed to add user to Role");
        }

        await _unitOfWork.CompleteAsync();
    }
}
