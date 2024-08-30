namespace Constellation.Application.Users.AuditAllUsers;

using Abstractions.Messaging;
using Constellation.Application.Models.Identity;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Models;
using Core.Models.Families;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Microsoft.AspNetCore.Identity;
using Serilog;
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
        
        List<Staff> staff = await _staffRepository.GetAllActive(cancellationToken);
        _logger.Information("Found {count} staff members currently registered", staff.Count);
        
        List<SchoolContact> contacts = await _contactRepository.GetAllActive(cancellationToken);
        _logger.Information("Found {count} school contacts currently registered", contacts.Count);
        
        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);
        _logger.Information("Found {count} students currently registered", students.Count);

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

            if (matchingParents.Any() || matchingFamilies.Any())
            {
                _logger.Information("Found matching parent and/or family");

                user.IsParent = true;
            }

            Staff matchingStaff = staff
                .FirstOrDefault(member => member.EmailAddress == user.Email);

            if (matchingStaff is not null)
            {
                _logger.Information("Found matching staff member");

                user.IsStaffMember = true;
                user.StaffId = matchingStaff.StaffId;
            }

            SchoolContact contact = contacts.FirstOrDefault(contact => contact.EmailAddress == user.Email);

            if (contact is not null)
            {
                _logger.Information("Found matching school contact");

                user.IsSchoolContact = true;
                user.SchoolContactId = contact.Id;
            }

            Student student = students.FirstOrDefault(student => student.EmailAddress.Email == user.Email);

            if (student is not null)
            {
                _logger.Information("Found matching student");

                user.IsStudent = true;
                user.StudentId = student.Id;
            }

            if (!matchingParents.Any() &&
                !matchingFamilies.Any() &&
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
            else
            {
                await _userManager.UpdateAsync(user);
            }
        }

        _logger.Information("Finished processing registered users");

        users = _userManager.Users.ToList();

        _logger.Information("{count} registered users remaining", users.Count);

        foreach (Family family in families)
        {
            _logger
                .ForContext(nameof(Family), family, true)
                .Information("Checking Family {name}", family.FamilyTitle);

            if (users.All(user => user.Email != family.FamilyEmail))
            {
                _logger.Information("Found no matching user.");
                _logger.Information("User will be created");

                AppUser user = new()
                {
                    UserName = family.FamilyEmail,
                    Email = family.FamilyEmail,
                    FirstName = string.Empty,
                    LastName = family.FamilyEmail,
                    IsParent = true
                };

                IdentityResult result = await _userManager.CreateAsync(user);

                if (!result.Succeeded)
                    _logger
                        .ForContext("Request", user, true)
                        .Warning("Failed to create user due to error {@error}", result.Errors);
            }
            else
            {
                _logger.Information("User found.");
            }
        }

        foreach (Parent parent in parents)
        {
            _logger
                .ForContext(nameof(Parent), parent, true)
                .Information("Checking parent {name}", $"{parent.FirstName} {parent.LastName}");

            if (users.All(user => user.Email != parent.EmailAddress))
            {
                _logger.Information("Found no matching user.");
                _logger.Information("User will be created");

                AppUser user = new()
                {
                    UserName = parent.EmailAddress,
                    Email = parent.EmailAddress,
                    FirstName = parent.FirstName,
                    LastName = parent.LastName,
                    IsParent = true
                };

                IdentityResult result = await _userManager.CreateAsync(user);

                if (!result.Succeeded)
                    _logger
                        .ForContext("Request", user, true)
                        .Warning("Failed to create user due to error {@error}", result.Errors);
            }
            else
            {
                _logger.Information("User found.");
            }
        }

        foreach (Staff member in staff)
        {
            _logger
                .ForContext(nameof(Staff), member, true)
                .Information("Checking staff member {name}", $"{member.FirstName} {member.LastName}");

            if (users.All(user => user.Email != member.EmailAddress))
            {
                _logger.Information("Found no matching user.");
                _logger.Information("User will be created");

                AppUser user = new()
                {
                    UserName = member.EmailAddress,
                    Email = member.EmailAddress,
                    FirstName = member.FirstName,
                    LastName = member.LastName,
                    IsStaffMember = true,
                    StaffId = member.StaffId
                };

                IdentityResult result = await _userManager.CreateAsync(user);

                if (!result.Succeeded)
                    _logger
                        .ForContext("Request", user, true)
                        .Warning("Failed to create user due to error {@error}", result.Errors);
            }
            else
            {
                _logger.Information("User found.");
            }
        }

        foreach (SchoolContact contact in contacts)
        {
            if (contact.Assignments.All(role => role.IsDeleted))
                continue;

            _logger
                .ForContext(nameof(SchoolContact), contact, true)
                .Information("Checking school contact {name}", $"{contact.FirstName} {contact.LastName}");

            if (users.All(user => user.Email != contact.EmailAddress))
            {
                _logger.Information("Found no matching user.");
                _logger.Information("User will be created");

                AppUser user = new()
                {
                    UserName = contact.EmailAddress,
                    Email = contact.EmailAddress,
                    FirstName = contact.FirstName,
                    LastName = contact.LastName,
                    IsSchoolContact = true,
                    SchoolContactId = contact.Id
                };

                IdentityResult result = await _userManager.CreateAsync(user);

                if (!result.Succeeded)
                    _logger
                        .ForContext("Request", user, true)
                        .Warning("Failed to create user due to error {@error}", result.Errors);
            }
            else
            {
                _logger.Information("User found.");
            }
        }

        foreach (Student student in students)
        {
            if (student.IsDeleted)
                continue;

            _logger
                .ForContext(nameof(Student), student, true)
                .Information("Checking student {name}", student.Name.DisplayName);

            if (users.All(user => user.Email != student.EmailAddress.Email))
            {
                _logger.Information("Found no matching user.");
                _logger.Information("User will be created");

                AppUser user = new()
                {
                    UserName = student.EmailAddress.Email,
                    Email = student.EmailAddress.Email,
                    FirstName = student.Name.PreferredName,
                    LastName = student.Name.LastName,
                    IsStudent = true,
                    StudentId = student.Id
                };

                IdentityResult result = await _userManager.CreateAsync(user);

                if (!result.Succeeded)
                    _logger
                    .ForContext("Request", user, true)
                        .Warning("Failed to create user due to error {@error}", result.Errors);
            }
            else
            {
                _logger.Information("User found.");
            }
        }

        _logger.Information("Finished processing potential users");

        users = _userManager.Users.ToList();

        _logger.Information("{count} total users now registered", users.Count);

        return Result.Success();
    }
}
