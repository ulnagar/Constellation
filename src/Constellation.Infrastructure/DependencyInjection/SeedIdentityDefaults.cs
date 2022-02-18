using Constellation.Application.Models.Identity;
using Constellation.Core.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.DependencyInjection
{
    public static class IdentityDefaults
    {
        public static void SeedRoles(RoleManager<AppRole> roleManager)
        {
            var roleList = new List<string>
            {
                AuthRoles.Admin,
                AuthRoles.Editor,
                AuthRoles.User,
                AuthRoles.CoverEditor,
                AuthRoles.EquipmentEditor,
                AuthRoles.LessonsEditor,
                AuthRoles.LessonsUser,
                AuthRoles.AbsencesEditor,
                AuthRoles.CoverRecipient
            };

            foreach (var role in roleList)
            {
                if (!roleManager.RoleExistsAsync(role).Result)
                {
                    var newRole = new AppRole
                    {
                        Name = role
                    };

                    var result = roleManager.CreateAsync(newRole).Result;
                }
            }
        }

        private static void CreateStaffUser(UserManager<AppUser> userManager, Staff staffMember)
        {
            var existingUser = userManager.FindByEmailAsync(staffMember.EmailAddress).Result;

            if (existingUser != null)
                return;

            var user = new AppUser
            {
                Email = staffMember.EmailAddress,
                UserName = staffMember.EmailAddress,
                FirstName = staffMember.FirstName,
                LastName = staffMember.LastName,
            };

            var result = userManager.CreateAsync(user).Result;

            if (result.Succeeded)
            {
                _ = userManager.AddToRoleAsync(user, AuthRoles.User).Result;
                var adminEmailToken = userManager.GenerateEmailConfirmationTokenAsync(user).Result;
                _ = userManager.ConfirmEmailAsync(user, adminEmailToken).Result;
            }
        }

        private static void CreateContactUser(UserManager<AppUser> userManager, SchoolContact contact)
        {
            var existingUser = userManager.FindByEmailAsync(contact.EmailAddress).Result;

            if (existingUser != null)
                return;

            var user = new AppUser
            {
                Email = contact.EmailAddress,
                UserName = contact.EmailAddress,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
            };

            var result = userManager.CreateAsync(user).Result;

            if (result.Succeeded)
            {
                _ = userManager.AddToRoleAsync(user, AuthRoles.LessonsUser).Result;
                var adminEmailToken = userManager.GenerateEmailConfirmationTokenAsync(user).Result;
                _ = userManager.ConfirmEmailAsync(user, adminEmailToken).Result;
            }
        }

        private static void CreateUser(UserManager<AppUser> userManager, string emailAddress, string firstName, string lastName, string defaultRole)
        {
            var existingUser = userManager.FindByEmailAsync(emailAddress).Result;

            if (existingUser != null)
                return;

            var user = new AppUser
            {
                Email = emailAddress,
                UserName = emailAddress,
                FirstName = firstName,
                LastName = lastName,
            };

            var result = userManager.CreateAsync(user).Result;

            if (result.Succeeded)
            {
                _ = userManager.AddToRoleAsync(user, defaultRole).Result;
                var adminEmailToken = userManager.GenerateEmailConfirmationTokenAsync(user).Result;
                _ = userManager.ConfirmEmailAsync(user, adminEmailToken).Result;
            }
        }

        public static void SeedUsers(UserManager<AppUser> userManager)
        {
            var adminUser = new AppUser
            {
                Email = "auroracollegeitsupport@det.nsw.edu.au",
                UserName = "auroracollegeitsupport@det.nsw.edu.au",
                FirstName = "Admin",
                LastName = "User",
            };

            var adminSuccess = userManager.CreateAsync(adminUser).Result;

            if (adminSuccess.Succeeded)
            {
                _ = userManager.AddToRoleAsync(adminUser, AuthRoles.Admin).Result;
                var adminPasswordToken = userManager.GeneratePasswordResetTokenAsync(adminUser).Result;
                var adminPasswordSuccess = userManager.ResetPasswordAsync(adminUser, adminPasswordToken, "P@ssw0rd").Result;
                var adminEmailToken = userManager.GenerateEmailConfirmationTokenAsync(adminUser).Result;
                _ = userManager.ConfirmEmailAsync(adminUser, adminEmailToken).Result;
            }

            var guestUser = new AppUser
            {
                Email = "noemail@here.com",
                UserName = "noemail@here.com",
                FirstName = "Guest",
                LastName = "User",
            };

            var guestSuccess = userManager.CreateAsync(guestUser).Result;

            if (guestSuccess.Succeeded)
            {
                _ = userManager.AddToRoleAsync(guestUser, AuthRoles.User).Result;
                var guestPasswordToken = userManager.GeneratePasswordResetTokenAsync(guestUser).Result;
                var guestPasswordSuccess = userManager.ResetPasswordAsync(guestUser, guestPasswordToken, "P@ssw0rd").Result;
                var guestEmailToken = userManager.GenerateEmailConfirmationTokenAsync(guestUser).Result;
                _ = userManager.ConfirmEmailAsync(guestUser, guestEmailToken).Result;
            }
        }
    }
}
