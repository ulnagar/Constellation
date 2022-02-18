using Constellation.Application.Models.Identity;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

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
