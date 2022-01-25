using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using Constellation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Services
{
    // Reviewed for ASYNC Operationss
    public class AuthService : IAuthService, IScopedService
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        private AppDbContext _context { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        private UserManager<AppUser> _userManager { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        private RoleManager<AppRole> _roleManager { get; set; }

        public AuthService(AppDbContext context, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IdentityResult> CreateUser(UserTemplateDto userDetails)
        {
            if (_userManager.Users.Any(u => u.UserName == userDetails.Username))
            {
                return await UpdateUser(userDetails.Username, userDetails);
            }

            var user = new AppUser
            {
                UserName = userDetails.Username,
                Email = userDetails.Email,
                FirstName = userDetails.FirstName,
                LastName = userDetails.LastName,
                StaffId = userDetails.StaffId
            };

            if (userDetails.IsSchoolContact != null && userDetails.IsSchoolContact.HasValue && userDetails.IsSchoolContact.Value)
            {
                user.IsSchoolContact = userDetails.IsSchoolContact.Value;
                user.SchoolContactId = userDetails.SchoolContactId;
            }
            else
            {
                user.IsSchoolContact = false;
            }

            if (userDetails.IsStaffMember != null && userDetails.IsStaffMember.HasValue && userDetails.IsStaffMember.Value)
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
                    await AddUserToRole(userDetails, AuthRoles.LessonsUser);
                }

                if (user.IsStaffMember)
                {
                    await AddUserToRole(userDetails, AuthRoles.User);
                }

                return result;
            }
        }

        public async Task<IdentityResult> UpdateUser(string username, UserTemplateDto newUser)
        {
            if (_userManager.Users.Any(u => u.UserName == username))
            {
                var user = await _userManager.FindByNameAsync(username);
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
                        user.SchoolContactId = 0;
                        await RemoveUserFromRole(newUser, AuthRoles.LessonsUser);
                    }
                    else if (!user.IsSchoolContact && newUser.IsSchoolContact.Value)
                    {
                        // Add user to School Contacts!
                        user.IsSchoolContact = newUser.IsSchoolContact.Value;
                        user.SchoolContactId = newUser.SchoolContactId;
                        await AddUserToRole(newUser, AuthRoles.LessonsUser);
                    }
                }

                if (newUser.IsStaffMember.HasValue)
                {
                    if (user.IsStaffMember && !newUser.IsStaffMember.Value)
                    {
                        // Remove user from Staff Members
                        user.IsStaffMember = newUser.IsStaffMember.Value;
                        user.StaffId = "";
                        await RemoveUserFromRole(newUser, AuthRoles.User);
                    }
                    else if (!user.IsStaffMember && newUser.IsStaffMember.Value)
                    {
                        // Add user to Users role
                        user.IsStaffMember = newUser.IsStaffMember.Value;
                        user.StaffId = newUser.StaffId;
                        await AddUserToRole(newUser, AuthRoles.User);
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

            return await CreateUser(newUser);
        }

        public async Task<IdentityResult> AddUserToRole(UserTemplateDto user, string group)
        {
            var dbUser = await _userManager.FindByNameAsync(user.Username);
            var dbRole = await _roleManager.FindByNameAsync(group);

            if (!await _userManager.IsInRoleAsync(dbUser, dbRole.Name))
            {
                return await _userManager.AddToRoleAsync(dbUser, dbRole.Name);
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> RemoveUserFromRole(UserTemplateDto user, string group)
        {
            var dbUser = await _userManager.FindByNameAsync(user.Username);
            var dbRole = await _roleManager.FindByNameAsync(group);

            if (await _userManager.IsInRoleAsync(dbUser, dbRole.Name))
            {
                return await _userManager.RemoveFromRoleAsync(dbUser, dbRole.Name);
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> RemoveUser(UserTemplateDto user)
        {
            if (_userManager.Users.Any(u => u.UserName == user.Username))
            {
                var dbUser = await _userManager.FindByNameAsync(user.Username);

                return await _userManager.DeleteAsync(dbUser);
            }

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
            var staff = await _context.Staff.ToListAsync();

            var users = await _userManager.Users.Where(user => user.IsSchoolContact == false).ToListAsync();

            foreach (var user in users)
            {
                var member = staff.FirstOrDefault(staffMember => staffMember.EmailAddress == user.Email);

                if (member == null)
                    continue;

                user.IsStaffMember = true;
                user.StaffId = member.StaffId;

                await _userManager.UpdateAsync(user);
            }
        }

        private async Task RepairSchoolContactUser(SchoolContact contact)
        {
            if (contact.Assignments.All(assignment => assignment.IsDeleted))
            {
                contact.IsDeleted = true;
                contact.DateDeleted = DateTime.Today;
                return;
            }

            var users = await _context.Users.ToListAsync();
            var role = await _roleManager.FindByNameAsync(AuthRoles.LessonsUser);

            var matchingUser = users.FirstOrDefault(user => user.Email == contact.EmailAddress);

            if (matchingUser == null)
            {
                // Create a new user

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

                if (result == IdentityResult.Success)
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

        public async Task RepairSchoolContactUser(int schoolContactId)
        {
            var contact = await _context.SchoolContacts
                .Include(contact => contact.Assignments)
                .Where(contact => !contact.IsDeleted)
                .SingleOrDefaultAsync(contact => contact.Id == schoolContactId);

            if (contact != null)
                await RepairSchoolContactUser(contact);
        }

        public async Task AuditSchoolContactUsers()
        {
            // Get list of SchoolContacts
            var contacts = await _context.SchoolContacts
                .Include(contact => contact.Assignments)
                .Where(contact => !contact.IsDeleted)
                .ToListAsync();

            // Find each SchoolContact in AppUsers
            foreach (var contact in contacts)
            {
                await RepairSchoolContactUser(contact);
            }
        }

        public async Task<UserAuditDto> VerifyContactAccess(string email)
        {
            var result = new UserAuditDto();

            var contacts = await _context.SchoolContacts
                .Include(innerContact => innerContact.Assignments)
                .ThenInclude(assignment => assignment.School)
                .Where(innerContact => innerContact.EmailAddress == email && !innerContact.IsDeleted)
                .ToListAsync();

            if (contacts.Any() && contacts.Count == 1)
            {
                result.ContactPresent = true;
                result.Contact = contacts.First();

                var roles = result.Contact.Assignments
                    .Where(assignment => !assignment.IsDeleted)
                    .ToList();

                if (roles.Any())
                {
                    result.RolesPresent = true;
                    result.Roles = roles;
                }
            }

            var users = await _userManager.Users.Where(account => account.Email == email).ToListAsync();

            if (users.Any() && users.Count == 1)
            {
                result.UserPresent = true;
                result.User = users.First();

                if (result.User.IsSchoolContact)
                {
                    result.UserPropertiesPresent = true;
                }

                if (result.User.SchoolContactId != 0 && result.User.SchoolContactId == result.Contact.Id)
                {
                    result.UserContactLinkPresent = true;
                }

                if (await _userManager.IsInRoleAsync(result.User, AuthRoles.LessonsUser))
                {
                    result.UserRolePresent = true;
                }
            }

            return result;
        }
    }
}
