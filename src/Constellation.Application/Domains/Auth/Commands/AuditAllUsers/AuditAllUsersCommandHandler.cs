namespace Constellation.Application.Domains.Auth.Commands.AuditAllUsers;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Models.Families;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Identifiers;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Models.Identity;
using Serilog;
using System;
using System.Collections.Generic;
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
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public AuditAllUsersCommandHandler(
        IFamilyRepository familyRepository, 
        IStaffRepository staffRepository,
        ISchoolContactRepository contactRepository,
        IStudentRepository studentRepository,
        UserManager<AppUser> userManager,
        ILogger logger)
    {
        _familyRepository = familyRepository;
        _staffRepository = staffRepository;
        _contactRepository = contactRepository;
        _studentRepository = studentRepository;
        _userManager = userManager;
        _logger = logger
            .ForContext<AuditAllUsersCommand>();
    }

    public async Task<Result> Handle(AuditAllUsersCommand request, CancellationToken cancellationToken)
    {
        _logger.Information("Starting scan of users");
        
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

            AppUser? existingUser = users.FirstOrDefault(user => user.Email == family.FamilyEmail);

            if (existingUser is null)
            {
                await CreateUserFromFamily(family);
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
                .Information("Checking parent {name}", $"{parent.FirstName} {parent.LastName}");

            AppUser? existingUser = users.FirstOrDefault(user => user.Email == parent.EmailAddress);

            if (existingUser is null)
            {
                await CreateUserFromParent(parent);
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
                .Information("Checking school contact {name}", $"{contact.FirstName} {contact.LastName}");

            AppUser? existingUser = users.FirstOrDefault(user => user.Email == contact.EmailAddress);

            if (existingUser is null)
            {
                await CreateUserFromContact(contact);
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
                await CreateUserFromStaffMember(member);
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
                await CreateUserFromStudent(student);
            }
            else
            {
                _logger.Information("User found.");

                await CheckStudentUserDetails(existingUser, student);
            }
        }

        _logger.Information("Finished processing potential users");

        _logger.Information("{count} total users now registered", _userManager.Users.Count());

        foreach (AppUser user in users)
        {
            if (user.Email!.Contains("auroracollegeitsupport@det.nsw.edu.au") || user.Email.Contains("noemail@here.com"))
                continue;

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
            
            SchoolContact? contact = contacts.FirstOrDefault(contact => contact.EmailAddress == user.Email);

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

                await _userManager.DeleteAsync(user);
            }
        }

        _logger.Information("Finished processing registered users");

        _logger.Information("{count} registered users remaining", _userManager.Users.Count());
        
        return Result.Success();
    }

    private Task CreateUserFromFamily(Family family) =>
        CreateUser(
            family.FamilyEmail,
            string.Empty,
            family.FamilyTitle,
            isParent: true);

    private Task CreateUserFromParent(Parent parent) =>
        CreateUser(
            parent.EmailAddress,
            parent.FirstName,
            parent.LastName,
            isParent: true);

    private Task CreateUserFromStaffMember(StaffMember staffMember) =>
        CreateUser(
            staffMember.EmailAddress.Email,
            staffMember.Name.FirstName,
            staffMember.Name.LastName,
            isStaff: true,
            staffId: staffMember.Id.ToString());

    private Task CreateUserFromStudent(Student student) =>
        CreateUser(
            student.EmailAddress.Email,
            student.Name.PreferredName,
            student.Name.LastName,
            isStudent: true,
            studentId: student.Id.Value);

    private Task CreateUserFromContact(SchoolContact contact) =>
        CreateUser(
            contact.EmailAddress,
            contact.FirstName,
            contact.LastName,
            isContact: true,
            contactId: contact.Id.Value);

    private async Task CreateUser(
        string email,
        string firstName,
        string lastName,
        string? staffId = null,
        Guid? contactId = null,
        Guid? studentId = null,
        bool? isParent = null,
        bool? isStaff = null,
        bool? isContact = null,
        bool? isStudent = null)
    {
        _logger.Information("Found no matching user.");
        _logger.Information("User will be created");

        AppUser user = new()
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };

        if (isParent.HasValue)
        {
            user.IsParent = isParent.Value;
        }

        if (isStaff.HasValue && !string.IsNullOrWhiteSpace(staffId))
        {
            user.IsStaffMember = isStaff.Value;
            user.StaffId = staffId;
        }

        if (isContact.HasValue && contactId.HasValue)
        {
            user.IsSchoolContact = isContact.Value;
            user.SchoolContactId = SchoolContactId.FromValue(contactId.Value);
        }

        if (isStudent.HasValue && studentId.HasValue)
        {
            user.IsStudent = isStudent.Value;
            user.StudentId = StudentId.FromValue(studentId.Value);
        }

        IdentityResult result = await _userManager.CreateAsync(user);

        if (!result.Succeeded)
            _logger
                .ForContext("Request", user, true)
                .Warning("Failed to create user due to error {@error}", result.Errors);
    }

    private async Task CheckFamilyUserDetails(AppUser user, Family family)
    {
        if (user.FirstName != string.Empty)
        {
            _logger.Information("Updating FirstName to {firstName}", string.Empty);

            user.FirstName = string.Empty;
        }

        if (user.LastName != family.FamilyTitle)
        {
            _logger.Information("Updating LastName to {lastName}", family.FamilyTitle);
            
            user.LastName = family.FamilyTitle;
        }

        if (user.IsParent != true)
        {
            _logger.Information("Updating IsParent to {isParent}", true);
            
            user.IsParent = true;
        }

        await _userManager.UpdateAsync(user);
    }

    private async Task CheckParentUserDetails(AppUser user, Parent parent)
    {
        if (user.FirstName != parent.FirstName)
        {
            _logger.Information("Updating FirstName to {firstName}", parent.FirstName);

            user.FirstName = parent.FirstName;
        }

        if (user.LastName != parent.LastName)
        {
            _logger.Information("Updating LastName to {lastName}", parent.LastName);

            user.LastName = parent.LastName;
        }

        if (user.IsParent != true)
        {
            _logger.Information("Updating IsParent to {isParent}", true);

            user.IsParent = true;
        }

        await _userManager.UpdateAsync(user);
    }

    private async Task CheckStaffUserDetails(AppUser user, StaffMember staffMember)
    {
        if (user.FirstName != staffMember.Name.FirstName)
        {
            _logger.Information("Updating FirstName to {firstName}", staffMember.Name.FirstName);

            user.FirstName = staffMember.Name.FirstName;
        }

        if (user.LastName != staffMember.Name.LastName)
        {
            _logger.Information("Updating LastName to {lastName}", staffMember.Name.LastName);

            user.LastName = staffMember.Name.LastName;
        }

        if (user.IsStaffMember != true)
        {
            _logger.Information("Updating IsStaffMember to {isStaffMember}", true);

            user.IsStaffMember = true;
        }

        if (user.StaffId != staffMember.Id.ToString())
        {
            _logger.Information("Updating StaffId to {staffId}", staffMember.Id);

            user.StaffId = staffMember.Id.ToString();
        }

        await _userManager.UpdateAsync(user);
    }

    private async Task CheckContactUserDetails(AppUser user, SchoolContact contact)
    {
        if (user.FirstName != contact.FirstName)
        {
            _logger.Information("Updating FirstName to {firstName}", contact.FirstName);

            user.FirstName = contact.FirstName;
        }

        if (user.LastName != contact.LastName)
        {
            _logger.Information("Updating LastName to {lastName}", contact.LastName);

            user.LastName = contact.LastName;
        }

        if (user.IsSchoolContact != true)
        {
            _logger.Information("Updating IsSchoolContact to {isSchoolContact}", true);

            user.IsSchoolContact = true;
        }

        if (user.SchoolContactId != contact.Id)
        {
            _logger.Information("Updating SchoolContactId to {schoolContactId}", contact.Id.Value);

            user.SchoolContactId = contact.Id;
        }

        await _userManager.UpdateAsync(user);
    }

    private async Task CheckStudentUserDetails(AppUser user, Student student)
    {
        if (user.FirstName != student.Name.PreferredName)
        {
            _logger.Information("Updating FirstName to {firstName}", student.Name.PreferredName);

            user.FirstName = student.Name.PreferredName;
        }

        if (user.LastName != student.Name.LastName)
        {
            _logger.Information("Updating LastName to {lastName}", student.Name.LastName);

            user.LastName = student.Name.LastName;
        }

        if (user.IsStudent != true)
        {
            _logger.Information("Updating IsStudent to {isStudent}", true);

            user.IsStudent = true;
        }

        if (user.StudentId != student.Id)
        {
            _logger.Information("Updating StudentId to {studentId}", student.Id);

            user.StudentId = student.Id;
        }

        await _userManager.UpdateAsync(user);
    }
}
