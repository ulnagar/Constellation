namespace Constellation.Infrastructure.DependencyInjection;

using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

public static class IdentityDefaults
{
    public static async Task SeedRoles(RoleManager<AppRole> roleManager)
    {
        await CreateRole(roleManager, AuthRoles.SchoolContact);

        await CreateRoleWithPermission(roleManager, AuthRoles.ExecStaffMember,
            new[]
            {
                AuthPermissions.SchoolAdmin.Awards.Add,
                AuthPermissions.StudentAdmin.Reports.Manage
            });

        await CreateRoleWithPermission(roleManager, AuthRoles.Admin, 
            new[] { 
                AuthPermissions.SchoolAdmin.Awards.Add,
                AuthPermissions.SchoolAdmin.Awards.View,
                AuthPermissions.SchoolAdmin.Awards.Manage,
                AuthPermissions.SchoolAdmin.Compliance.Manage,
                AuthPermissions.StudentAdmin.Reports.Manage,
                AuthPermissions.AssignmentsEdit,
                AuthPermissions.AssignmentsSubmit,
                AuthPermissions.ContactsEdit,
                AuthPermissions.EquipmentEdit,
                AuthPermissions.EquipmentView,
                AuthPermissions.GroupTutorialsEdit,
                AuthPermissions.GroupTutorialsView,
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
                AuthPermissions.SchoolAdmin.Awards.Add,
                AuthPermissions.SchoolAdmin.Awards.View,
                AuthPermissions.SchoolAdmin.Awards.Manage,
                AuthPermissions.AssignmentsEdit,
                AuthPermissions.AssignmentsSubmit,
                AuthPermissions.ContactsEdit,
                AuthPermissions.EquipmentEdit,
                AuthPermissions.EquipmentView,
                AuthPermissions.GroupTutorialsEdit,
                AuthPermissions.GroupTutorialsView,
                AuthPermissions.LessonsEdit,
                AuthPermissions.LessonsView,
                //AuthPermissions.MandatoryTrainingBulkDownload,
                //AuthPermissions.MandatoryTrainingDetailsView,
                //AuthPermissions.MandatoryTrainingEdit,
                //AuthPermissions.MandatoryTrainingReportsRun,
                //AuthPermissions.MandatoryTrainingView,
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
                AuthPermissions.SchoolAdmin.Awards.View,
                AuthPermissions.AssignmentsEdit,
                AuthPermissions.GroupTutorialsView,
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

        await CreateRoleWithPermission(roleManager, AuthRoles.ComplianceManager,
            new [] {
                AuthPermissions.SchoolAdmin.Compliance.Manage });

        await CreateRoleWithPermission(roleManager, AuthRoles.GroupTutorialsEditor,
            new[]
            {
                AuthPermissions.GroupTutorialsEdit });

        await CreateRoleWithPermission(roleManager, AuthRoles.AwardsManager,
            new[]
            {
                AuthPermissions.SchoolAdmin.Awards.Add,
                AuthPermissions.SchoolAdmin.Awards.View,
                AuthPermissions.SchoolAdmin.Awards.Manage,
            });

        await CreateRole(roleManager, AuthRoles.SchoolContact);
        await CreateRole(roleManager, AuthRoles.CoverRecipient);
    }

    private static async Task CreateRole(RoleManager<AppRole> roleManager, string roleName)
    {
        AppRole existing = await roleManager.FindByNameAsync(roleName);

        if (existing is null)
        {
            await roleManager.CreateAsync(new AppRole { Name = roleName });
        }
    }
    
    private static async Task CreateRoleWithPermission(RoleManager<AppRole> roleManager, string roleName, string[] permissions)
    {
        await CreateRole(roleManager, roleName);

        AppRole role = await roleManager.FindByNameAsync(roleName);

        IList<Claim> claims = await roleManager.GetClaimsAsync(role);

        List<Claim> permissionClaims = claims
            .Where(claim => claim.Type == AuthClaimType.Permission)
            .ToList();

        foreach (Claim claim in permissionClaims)
        {
            if (permissions.Contains(claim.Value))
                continue;

            await roleManager.RemoveClaimAsync(role, claim);
        }

        foreach (string permission in permissions)
        {
            if (permissionClaims.All(claim => claim.Value != permission))
                await roleManager.AddClaimAsync(role, new Claim(AuthClaimType.Permission, permission));
        }
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
