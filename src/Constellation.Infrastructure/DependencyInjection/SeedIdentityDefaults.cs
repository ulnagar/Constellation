using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.DependencyInjection
{
    public static class IdentityDefaults
    {
        public static async Task SeedRoles(RoleManager<AppRole> roleManager)
        {
            await CreateRoleWithPermission(roleManager, AuthRoles.Admin, 
                new[] { 
                    AuthPermissions.AssignmentsEdit,
                    AuthPermissions.AssignmentsSubmit,
                    AuthPermissions.ContactsEdit,
                    AuthPermissions.EquipmentEdit,
                    AuthPermissions.EquipmentView,
                    AuthPermissions.LessonsEdit,
                    AuthPermissions.LessonsView,
                    AuthPermissions.MandatoryTrainingBulkDownload,
                    AuthPermissions.MandatoryTrainingDetailsView,
                    AuthPermissions.MandatoryTrainingEdit,
                    AuthPermissions.MandatoryTrainingReportsRun,
                    AuthPermissions.MandatoryTrainingView,
                    AuthPermissions.PartnerDetailsView,
                    AuthPermissions.PartnerEdit,
                    AuthPermissions.PartnerView,
                    AuthPermissions.PortalParentsAdmin,
                    AuthPermissions.PortalSchoolsAdmin,
                    AuthPermissions.ReportsAbsencesNotify,
                    AuthPermissions.ReportsAbsencesRun,
                    AuthPermissions.ReportsAwardsRun,
                    AuthPermissions.ReportsAwardsView,
                    AuthPermissions.ReportsFTERun,
                    AuthPermissions.ReportsPTOSetupRun,
                    AuthPermissions.ReportsStudentAttendanceRun,
                    AuthPermissions.ShortTermCasualsEdit,
                    AuthPermissions.ShortTermCoversEdit,
                    AuthPermissions.ShortTermDetailsView,
                    AuthPermissions.ShortTermEdit,
                    AuthPermissions.ShortTermOperationsRun,
                    AuthPermissions.ShortTermView,
                    AuthPermissions.StocktakeEdit,
                    AuthPermissions.StocktakeView,
                    AuthPermissions.SubjectsDetailsView,
                    AuthPermissions.SubjectsEdit,
                    AuthPermissions.SubjectsView,
                    AuthPermissions.UtilityAdmin });

            await CreateRoleWithPermission(roleManager, AuthRoles.Editor,
                new[] {
                    AuthPermissions.AssignmentsEdit,
                    AuthPermissions.AssignmentsSubmit,
                    AuthPermissions.ContactsEdit,
                    AuthPermissions.EquipmentEdit,
                    AuthPermissions.EquipmentView,
                    AuthPermissions.LessonsEdit,
                    AuthPermissions.LessonsView,
                    AuthPermissions.MandatoryTrainingBulkDownload,
                    AuthPermissions.MandatoryTrainingDetailsView,
                    AuthPermissions.MandatoryTrainingEdit,
                    AuthPermissions.MandatoryTrainingReportsRun,
                    AuthPermissions.MandatoryTrainingView,
                    AuthPermissions.PartnerDetailsView,
                    AuthPermissions.PartnerEdit,
                    AuthPermissions.PartnerView,
                    AuthPermissions.ReportsAbsencesRun,
                    AuthPermissions.ReportsAwardsRun,
                    AuthPermissions.ReportsAwardsView,
                    AuthPermissions.ReportsFTERun,
                    AuthPermissions.ReportsStudentAttendanceRun,
                    AuthPermissions.ShortTermCasualsEdit,
                    AuthPermissions.ShortTermCoversEdit,
                    AuthPermissions.ShortTermDetailsView,
                    AuthPermissions.ShortTermEdit,
                    AuthPermissions.ShortTermOperationsRun,
                    AuthPermissions.ShortTermView,
                    AuthPermissions.StocktakeEdit,
                    AuthPermissions.StocktakeView,
                    AuthPermissions.SubjectsDetailsView,
                    AuthPermissions.SubjectsEdit,
                    AuthPermissions.SubjectsView});

            await CreateRoleWithPermission(roleManager, AuthRoles.StaffMember,
                new[] {
                    AuthPermissions.AssignmentsEdit,
                    AuthPermissions.LessonsView,
                    AuthPermissions.MandatoryTrainingView,
                    AuthPermissions.PartnerDetailsView,
                    AuthPermissions.PartnerView,
                    AuthPermissions.ShortTermDetailsView,
                    AuthPermissions.ShortTermView,
                    AuthPermissions.StocktakeView,
                    AuthPermissions.SubjectsDetailsView,
                    AuthPermissions.SubjectsView });

            await CreateRoleWithPermission(roleManager, AuthRoles.CoverEditor, 
                new[] { 
                    AuthPermissions.ShortTermCoversEdit,
                    AuthPermissions.ShortTermCasualsEdit,
                    AuthPermissions.ShortTermOperationsRun });

            await CreateRoleWithPermission(roleManager, AuthRoles.AbsencesEditor, 
                new[] { 
                    AuthPermissions.ReportsAbsencesRun,
                    AuthPermissions.ReportsAbsencesNotify,
                    AuthPermissions.ReportsStudentAttendanceRun });

            await CreateRoleWithPermission(roleManager, AuthRoles.LessonsEditor, 
                new[] { 
                    AuthPermissions.LessonsEdit,
                    AuthPermissions.ContactsEdit });

            await CreateRoleWithPermission(roleManager, AuthRoles.EquipmentEditor, 
                new[] { 
                    AuthPermissions.EquipmentEdit,
                    AuthPermissions.StocktakeEdit });

            await CreateRoleWithPermission(roleManager, AuthRoles.MandatoryTrainingEditor, 
                new[] { 
                    AuthPermissions.MandatoryTrainingBulkDownload,
                    AuthPermissions.MandatoryTrainingDetailsView,
                    AuthPermissions.MandatoryTrainingEdit,
                    AuthPermissions.MandatoryTrainingReportsRun,
                    AuthPermissions.MandatoryTrainingView });

            await CreateRole(roleManager, AuthRoles.LessonsUser);
            await CreateRole(roleManager, AuthRoles.CoverRecipient);
        }

        private static async Task CreateRole(RoleManager<AppRole> roleManager, string roleName)
        {
            var existing = await roleManager.FindByNameAsync(roleName);

            if (existing is null)
            {
                await roleManager.CreateAsync(new AppRole { Name = roleName });
            }
        }
        
        private static async Task CreateRoleWithPermission(RoleManager<AppRole> roleManager, string roleName, string[] permissions)
        {
            await CreateRole(roleManager, roleName);

            var role = await roleManager.FindByNameAsync(roleName);

            var claims = await roleManager.GetClaimsAsync(role);

            foreach (var permission in permissions)
            {
                if (!claims.Any(claim => claim.Type == AuthClaimType.Permission && claim.Value == permission))
                {
                    await roleManager.AddClaimAsync(role, new Claim(AuthClaimType.Permission, permission));
                }
            }
        }

        public static async Task SeedTestUsers(UserManager<AppUser> userManager)
        {
            await CreateUser(userManager, new AppUser
                {
                    Email = "admin@test.co",
                    UserName = "admin@test.co",
                    FirstName = "Admin",
                    LastName = "Test",
                    IsStaffMember = true,
                    StaffId = "1"
                }, 
                new[] { AuthRoles.Admin });
            
            await CreateUser(userManager, new AppUser
            {
                Email = "editor@test.co",
                UserName = "editor@test.co",
                FirstName = "Editor",
                LastName = "Test",
                IsStaffMember = true,
                StaffId = "2"
            },
                new[] { AuthRoles.Editor });

            await CreateUser(userManager, new AppUser
            {
                Email = "teacher@test.co",
                UserName = "teacher@test.co",
                FirstName = "Teacher",
                LastName = "Test",
                IsStaffMember = true,
                StaffId = "3"
            },
                new[] { AuthRoles.StaffMember });

            await CreateUser(userManager, new AppUser
            {
                Email = "cover@test.co",
                UserName = "cover@test.co",
                FirstName = "Cover Editor",
                LastName = "Test",
                IsStaffMember = true,
                StaffId = "4"
            },
                new[] { AuthRoles.StaffMember, AuthRoles.CoverEditor });

            await CreateUser(userManager, new AppUser
            {
                Email = "devices@test.co",
                UserName = "devices@test.co",
                FirstName = "Device Editor",
                LastName = "Test",
                IsStaffMember = true,
                StaffId = "5"
            },
                new[] { AuthRoles.StaffMember, AuthRoles.EquipmentEditor });

            await CreateUser(userManager, new AppUser
            {
                Email = "lessons@test.co",
                UserName = "lessons@test.co",
                FirstName = "Lessons Editor",
                LastName = "Test",
                IsStaffMember = true,
                StaffId = "6"
            },
                new[] { AuthRoles.StaffMember, AuthRoles.LessonsEditor });

            await CreateUser(userManager, new AppUser
            {
                Email = "absences@test.co",
                UserName = "absences@test.co",
                FirstName = "Absences Editor",
                LastName = "Test",
                IsStaffMember = true,
                StaffId = "7"
            },
                new[] { AuthRoles.StaffMember, AuthRoles.AbsencesEditor });

            await CreateUser(userManager, new AppUser
            {
                Email = "training@test.co",
                UserName = "training@test.co",
                FirstName = "Training Editor",
                LastName = "Test",
                IsStaffMember = true,
                StaffId = "8"
            },
                new[] { AuthRoles.StaffMember, AuthRoles.MandatoryTrainingEditor });
        }

        private static async Task CreateUser(UserManager<AppUser> userManager, AppUser user, string[] roles)
        {
            var exists = await userManager.FindByEmailAsync(user.Email);
            if (exists is not null)
            {
                await userManager.DeleteAsync(exists);
            }

            await userManager.CreateAsync(user);
            await userManager.AddToRolesAsync(user, roles);

            // Set password
            var passwordToken = await userManager.GeneratePasswordResetTokenAsync(user);
            await userManager.ResetPasswordAsync(user, passwordToken, "Pa$$w.rd");

            // Confirm email
            var emailToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await userManager.ConfirmEmailAsync(user, emailToken);
        }

        public static async Task SeedUsers(UserManager<AppUser> userManager)
        {
            var checkUser = await userManager.FindByEmailAsync("auroracollegeitsupport@det.nsw.edu.au");

            if (checkUser == null)
            {
                var adminUser = new AppUser
                {
                    Email = "auroracollegeitsupport@det.nsw.edu.au",
                    UserName = "auroracollegeitsupport@det.nsw.edu.au",
                    FirstName = "Admin",
                    LastName = "User",
                };
                var adminSuccess = await userManager.CreateAsync(adminUser);

                if (adminSuccess.Succeeded)
                {
                    // Add to role
                    await userManager.AddToRoleAsync(adminUser, AuthRoles.Admin);

                    // Set password
                    var adminPasswordToken = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                    await userManager.ResetPasswordAsync(adminUser, adminPasswordToken, "P@ssw0rd");
                    
                    // Confirm email
                    var adminEmailToken = await userManager.GenerateEmailConfirmationTokenAsync(adminUser);
                    await userManager.ConfirmEmailAsync(adminUser, adminEmailToken);
                }
            }

            checkUser = await userManager.FindByEmailAsync("noemail@here.com");

            if (checkUser == null)
            {
                var guestUser = new AppUser
                {
                    Email = "noemail@here.com",
                    UserName = "noemail@here.com",
                    FirstName = "Guest",
                    LastName = "User",
                };

                var guestSuccess = await userManager.CreateAsync(guestUser);

                if (guestSuccess.Succeeded)
                {
                    // Add to role
                    await userManager.AddToRoleAsync(guestUser, AuthRoles.StaffMember);

                    // Set password
                    var guestPasswordToken = await userManager.GeneratePasswordResetTokenAsync(guestUser);
                    await userManager.ResetPasswordAsync(guestUser, guestPasswordToken, "P@ssw0rd");
                    
                    // Confirm email
                    var guestEmailToken = await userManager.GenerateEmailConfirmationTokenAsync(guestUser);
                    await userManager.ConfirmEmailAsync(guestUser, guestEmailToken);
                }
            }
        }
    }
}
