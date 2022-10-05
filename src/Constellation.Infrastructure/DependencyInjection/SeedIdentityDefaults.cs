using Constellation.Application.Models.Identity;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;

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

            var existingRoles = roleManager.Roles.ToList();

            foreach (var roleName in roleList)
            {
                if (!existingRoles.Any(role => role.Name == roleName))
                {
                    var role = new AppRole
                    {
                        Name = roleName
                    };

                    var result = roleManager.CreateAsync(role).Result;
                }
            }
        }

        public static void SeedUsers(UserManager<AppUser> userManager)
        {
            var checkUser = userManager.FindByEmailAsync("auroracollegeitsupport@det.nsw.edu.au").Result;

            if (checkUser == null)
            {
                var adminUser = new AppUser
                {
                    Email = "auroracollegeitsupport@det.nsw.edu.au",
                    UserName = "Aurora College Admin",
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
            }

            checkUser = userManager.FindByEmailAsync("noemail@here.com").Result;

            if (checkUser == null)
            {
                var guestUser = new AppUser
                {
                    Email = "noemail@here.com",
                    UserName = "Guest Account",
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
}
