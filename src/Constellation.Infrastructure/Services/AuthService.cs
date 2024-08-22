namespace Constellation.Infrastructure.Services;

using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Models.Auth;
using Constellation.Application.Features.Auth.Command;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Core.Abstractions.Repositories;
using Core.Models;
using Core.Models.Families;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Identifiers;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.StaffMembers.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Linq;
using System.Threading.Tasks;

public class AuthService : IAuthService
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFamilyRepository _familyRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly ILogger _logger;

    public AuthService(
        IFamilyRepository familyRepository,
        ISchoolContactRepository contactRepository,
        IStaffRepository staffRepository,
        IMediator mediator,
        IUnitOfWork unitOfWork,
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        ILogger logger)
    {
        _familyRepository = familyRepository;
        _contactRepository = contactRepository;
        _staffRepository = staffRepository;
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger.ForContext<IAuthService>();
    }

    public async Task<IdentityResult> CreateUser(UserTemplateDto userDetails)
    {
        if (_userManager.Users.Any(u => u.UserName == userDetails.Username))
        {
            return await UpdateUser(userDetails.Email, userDetails);
        }

        var user = new AppUser
        {
            UserName = userDetails.Username,
            Email = userDetails.Email,
            FirstName = userDetails.FirstName,
            LastName = userDetails.LastName,
            StaffId = userDetails.StaffId
        };

        if (userDetails.IsSchoolContact is not null && userDetails.IsSchoolContact.Value)
        {
            user.IsSchoolContact = userDetails.IsSchoolContact.Value;
            user.SchoolContactId = userDetails.SchoolContactId;
        }
        else
        {
            user.IsSchoolContact = false;
        }

        if (userDetails.IsStaffMember is not null && userDetails.IsStaffMember.Value)
        {
            user.IsStaffMember = userDetails.IsStaffMember.Value;
            user.StaffId = userDetails.StaffId;
        }
        else
        {
            user.IsStaffMember = false;
        }

        var result = await _userManager.CreateAsync(user);

        if (result != IdentityResult.Success)
        {
            return result;
        }
        else
        {
            if (user.IsSchoolContact)
            {
                await AddUserToRole(userDetails, AuthRoles.SchoolContact);
            }

            if (user.IsStaffMember)
            {
                await AddUserToRole(userDetails, AuthRoles.StaffMember);
            }

            return result;
        }
    }

    public async Task<IdentityResult> UpdateUser(string email, UserTemplateDto newUser)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
            return await CreateUser(newUser);

        user.UserName = newUser.Username;
        user.Email = newUser.Email;
        user.FirstName = newUser.FirstName;
        user.LastName = newUser.LastName;

        if (newUser.IsSchoolContact.HasValue)
        {
            if (user.IsSchoolContact && !newUser.IsSchoolContact.Value)
            {
                // Remove user from School Contacts!
                user.IsSchoolContact = newUser.IsSchoolContact.Value;
                user.SchoolContactId = SchoolContactId.Empty;
                await RemoveUserFromRole(newUser, AuthRoles.SchoolContact);
            }
            else if (!user.IsSchoolContact && newUser.IsSchoolContact.Value)
            {
                // Add user to School Contacts!
                user.IsSchoolContact = newUser.IsSchoolContact.Value;
                user.SchoolContactId = newUser.SchoolContactId;
                await AddUserToRole(newUser, AuthRoles.SchoolContact);
            }
        }

        if (newUser.IsStaffMember.HasValue)
        {
            if (user.IsStaffMember && !newUser.IsStaffMember.Value)
            {
                // Remove user from Staff Members
                user.IsStaffMember = newUser.IsStaffMember.Value;
                user.StaffId = "";
                await RemoveUserFromRole(newUser, AuthRoles.StaffMember);
            }
            else if (!user.IsStaffMember && newUser.IsStaffMember.Value)
            {
                // Add user to Users role
                user.IsStaffMember = newUser.IsStaffMember.Value;
                user.StaffId = newUser.StaffId;
                await AddUserToRole(newUser, AuthRoles.StaffMember);
            }
        }

        if (!user.IsStaffMember && !user.IsSchoolContact)
        {
            // Remove access to all roles and delete account
            return await RemoveUser(newUser);
        }
        else
        {
            return await _userManager.UpdateAsync(user);
        }
    }

    public async Task<IdentityResult> AddUserToRole(UserTemplateDto user, string group)
    {
        var dbUser = await _userManager.FindByEmailAsync(user.Email);
        var dbRole = await _roleManager.FindByNameAsync(group);

        if (!await _userManager.IsInRoleAsync(dbUser, dbRole.Name))
        {
            return await _userManager.AddToRoleAsync(dbUser, dbRole.Name);
        }

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> RemoveUserFromRole(UserTemplateDto user, string group)
    {
        var dbUser = await _userManager.FindByEmailAsync(user.Email);
        var dbRole = await _roleManager.FindByNameAsync(group);

        if (await _userManager.IsInRoleAsync(dbUser, dbRole.Name))
        {
            return await _userManager.RemoveFromRoleAsync(dbUser, dbRole.Name);
        }

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> RemoveUser(UserTemplateDto userDto)
    {
        var user = await _userManager.FindByEmailAsync(userDto.Email);

        if (user != null)
            return await _userManager.DeleteAsync(user);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> ChangeUserPassword(string userId, string password, string newPassword)
    {
        var dbUser = await _userManager.FindByNameAsync(userId);

        return await _userManager.ChangePasswordAsync(dbUser, password, newPassword);
    }

    public async Task<IdentityResult> ChangeUserPassword(string userId, string newPassword)
    {
        var user = await _userManager.FindByNameAsync(userId);

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        return await _userManager.ResetPasswordAsync(user, token, newPassword);
    }

    public async Task RepairStaffUserAccounts()
    {
        var staff = await _staffRepository.GetAllActive();

        var users = await _userManager.Users.ToListAsync();

        foreach (var member in staff)
        {
            var user = users.FirstOrDefault(user => user.Email == member.EmailAddress);

            if (user == null)
            {
                // Create new user
                user = new AppUser
                {
                    UserName = member.EmailAddress,
                    Email = member.EmailAddress,
                    FirstName = member.FirstName,
                    LastName = member.LastName,
                    IsSchoolContact = false,
                    IsStaffMember = true,
                    StaffId = member.StaffId
                };

                await _userManager.CreateAsync(user);

                await _userManager.AddToRoleAsync(user, AuthRoles.StaffMember);
                await _userManager.UpdateAsync(user);
            } else
            {
                user.IsStaffMember = true;
                user.StaffId = member.StaffId;

                var inRole = await _userManager.IsInRoleAsync(user, AuthRoles.StaffMember);

                if (!inRole)
                    await _userManager.AddToRoleAsync(user, AuthRoles.StaffMember);
                await _userManager.UpdateAsync(user);
            }
        }

        foreach (var user in users)
        {
            var member = staff.FirstOrDefault(staffMember => staffMember.EmailAddress == user.Email);

            if (member == null)
            {
                user.IsStaffMember = false;
                user.StaffId = null;

                var inRole = await _userManager.IsInRoleAsync(user, AuthRoles.StaffMember);

                if (inRole)
                    await _userManager.RemoveFromRoleAsync(user, AuthRoles.StaffMember);
                await _userManager.UpdateAsync(user);
            } else
            {
                user.IsStaffMember = true;
                user.StaffId = member.StaffId;

                await _userManager.UpdateAsync(user);

                var inRole = await _userManager.IsInRoleAsync(user, AuthRoles.StaffMember);

                if (!inRole)
                    await _userManager.AddToRoleAsync(user, AuthRoles.StaffMember);
                await _userManager.UpdateAsync(user);
            }
        }
    }

    private async Task RepairSchoolContactUser(SchoolContact contact)
    {
        if (contact.Assignments.All(assignment => assignment.IsDeleted))
        {
            contact.Delete();

            await _unitOfWork.CompleteAsync();
            return;
        }

        List<AppUser> users = await _userManager.Users.ToListAsync();
        AppRole role = await _roleManager.FindByNameAsync(AuthRoles.SchoolContact);

        AppUser matchingUser = users.FirstOrDefault(user => user.Email == contact.EmailAddress);

        if (matchingUser == null)
        {
            // Create a new user

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

            if (result.Succeeded)
            {
                // Succeeded. Add to Role
                await _userManager.AddToRoleAsync(user, role.Name);
                await _userManager.UpdateAsync(user);
            }

        }
        else if (matchingUser.IsSchoolContact == false)
        {
            // Link existing user to contact
            matchingUser.IsSchoolContact = true;
            matchingUser.SchoolContactId = contact.Id;

            // Add to Role
            await _userManager.AddToRoleAsync(matchingUser, role.Name);
            await _userManager.UpdateAsync(matchingUser);
        }
    }

    public async Task RepairSchoolContactUser(SchoolContactId schoolContactId)
    {
        SchoolContact contact = await _contactRepository.GetById(schoolContactId);

        if (contact != null)
            await RepairSchoolContactUser(contact);
    }

    public async Task AuditSchoolContactUsers()
    {
        // Get list of SchoolContacts
        List<SchoolContact> contacts = await _contactRepository.GetAllActive();

        // Find each SchoolContact in AppUsers
        foreach (SchoolContact contact in contacts)
        {
            await RepairSchoolContactUser(contact);
        }
    }

    public async Task AuditParentUsers(CancellationToken cancellationToken = default)
    {
        // Get list of Parents
        List<Family> families = await _familyRepository.GetAllCurrent(cancellationToken);
        List<Parent> parents = families.SelectMany(family => family.Parents).ToList();

        // Find each Parent in AppUsers
        foreach (Parent parent in parents)
        {
            await _mediator.Send(new RegisterParentContactAsUserCommand
            {
                FirstName = parent.FirstName,
                LastName = parent.LastName,
                EmailAddress = parent.EmailAddress
            }, cancellationToken);
        }
    }

    public async Task<UserAuditDto> VerifyContactAccess(string email)
    {
        UserAuditDto result = new UserAuditDto();

        SchoolContact contact = await _contactRepository.GetWithRolesByEmailAddress(email);

        if (contact is not null)
        {
            result.ContactPresent = true;
            result.Contact = contact;

            List<SchoolContactRole> roles = result.Contact.Assignments
                .Where(assignment => !assignment.IsDeleted)
                .ToList();

            if (roles.Any())
            {
                result.RolesPresent = true;
                result.Roles = roles;
            }
        }

        List<AppUser> users = await _userManager.Users.Where(account => account.Email == email).ToListAsync();

        if (users.Any() && users.Count == 1)
        {
            result.UserPresent = true;
            result.User = users.First();

            if (result.User.IsSchoolContact)
            {
                result.UserPropertiesPresent = true;
            }

            if (result.User.SchoolContactId == result.Contact.Id)
            {
                result.UserContactLinkPresent = true;
            }

            if (await _userManager.IsInRoleAsync(result.User, AuthRoles.SchoolContact))
            {
                result.UserRolePresent = true;
            }
        }

        return result;
    }
    
    public async Task AuditAllUsers(CancellationToken cancellationToken)
    {
        _logger.Information("Starting scan of users");

        // Get all users
        List<AppUser> users = _userManager.Users.ToList();

        _logger.Information("Found {count} users currently registered", users.Count);

        // Get all family/parent details
        List<Family> families = await _familyRepository.GetAllCurrent(cancellationToken);

        _logger.Information("Found {count} families currently registered", families.Count);

        List<Parent> parents = families
            .SelectMany(family => family.Parents)
            .ToList();

        _logger.Information("Found {count} parents currently registered", parents.Count);

        // Get all staff details
        List<Staff> staff = await _staffRepository.GetAllActive(cancellationToken);

        _logger.Information("Found {count} staff members currently registered", staff.Count);

        // Get all school contact details
        List<SchoolContact> contacts = await _contactRepository.GetAllActive(cancellationToken);

        _logger.Information("Found {count} school contacts currently registered", contacts.Count);

        foreach (AppUser user in users)
        {
            if (user.Email.Contains("auroracollegeitsupport@det.nsw.edu.au") || user.Email.Contains("noemail@here.com"))
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

            if (!matchingParents.Any() &&
                !matchingFamilies.Any() &&
                matchingStaff is null &&
                contact is null)
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

                var user = new AppUser
                {
                    UserName = parent.EmailAddress,
                    Email = parent.EmailAddress,
                    FirstName = parent.FirstName,
                    LastName = parent.LastName,
                    IsParent = true
                };

                var result = await _userManager.CreateAsync(user);

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

                var user = new AppUser
                {
                    UserName = member.EmailAddress,
                    Email = member.EmailAddress,
                    FirstName = member.FirstName,
                    LastName = member.LastName,
                    IsStaffMember = true,
                    StaffId = member.StaffId
                };

                var result = await _userManager.CreateAsync(user);

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
            {
                continue;
            }

            _logger
                .ForContext(nameof(SchoolContact), contact, true)
                .Information("Checking school contact {name}", $"{contact.FirstName} {contact.LastName}");

            if (users.All(user => user.Email != contact.EmailAddress))
            {
                _logger.Information("Found no matching user.");
                _logger.Information("User will be created");

                var user = new AppUser
                {
                    UserName = contact.EmailAddress,
                    Email = contact.EmailAddress,
                    FirstName = contact.FirstName,
                    LastName = contact.LastName,
                    IsSchoolContact = true,
                    SchoolContactId = contact.Id
                };

                var result = await _userManager.CreateAsync(user);

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
    }
}
