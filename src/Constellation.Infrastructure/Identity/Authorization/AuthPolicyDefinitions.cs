namespace Constellation.Infrastructure.Identity.Authorization;

using Constellation.Application.Models.Auth;
using Microsoft.AspNetCore.Authorization;

public static class AuthPolicyDefinitions
{
    public static AuthorizationOptions AddApplicationPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(AuthPolicies.CanViewTrainingCompletionRecord, policy =>
            policy.Requirements.Add(new CanViewTrainingCompletionRecordRequirement()));

        options.AddPolicy(AuthPolicies.CanEditTrainingModuleContent, policy =>
            policy.RequireClaim(AuthClaimType.Permission, AuthPermissions.MandatoryTrainingEdit));

        options.AddPolicy(AuthPolicies.CanViewTrainingModuleContent, policy =>
            policy.RequireClaim(AuthClaimType.Permission, AuthPermissions.MandatoryTrainingView));

        options.AddPolicy(AuthPolicies.CanViewTrainingModuleContentDetails, policy =>
            policy.RequireClaim(AuthClaimType.Permission, AuthPermissions.MandatoryTrainingDetailsView));

        options.AddPolicy(AuthPolicies.CanRunTrainingModuleReports, policy =>
            policy.RequireClaim(AuthClaimType.Permission, AuthPermissions.MandatoryTrainingReportsRun));

        options.AddPolicy(AuthPolicies.IsStaffMember, policy =>
            policy.RequireClaim(AuthClaimType.StaffEmployeeId));

        options.AddPolicy(AuthPolicies.CanEditFaculties, policy =>
            policy.RequireRole(AuthRoles.Editor, AuthRoles.Admin));

        options.AddPolicy(AuthPolicies.CanViewFacultyDetails, policy =>
            policy.RequireRole(AuthRoles.StaffMember, AuthRoles.Editor, AuthRoles.Admin));

        options.AddPolicy(AuthPolicies.CanViewGroupTutorials, policy =>
            policy.RequireClaim(AuthClaimType.Permission, AuthPermissions.GroupTutorialsView));

        options.AddPolicy(AuthPolicies.CanEditGroupTutorials, policy =>
            policy.RequireClaim(AuthClaimType.Permission, AuthPermissions.GroupTutorialsEdit));

        options.AddPolicy(AuthPolicies.CanSubmitGroupTutorialRolls, policy =>
            policy.Requirements.Add(new CanSubmitGroupTutorialRollRequirement()));

        options.AddPolicy(AuthPolicies.IsSiteAdmin, policy =>
            policy.RequireRole(AuthRoles.Admin));

        options.AddPolicy(AuthPolicies.CanViewCovers, policy =>
            policy.RequireClaim(AuthClaimType.Permission, AuthPermissions.ShortTermView));

        options.AddPolicy(AuthPolicies.CanEditCovers, policy =>
            policy.RequireClaim(AuthClaimType.Permission, AuthPermissions.ShortTermCoversEdit));

        options.AddPolicy(AuthPolicies.CanEditCasuals, policy =>
            policy.RequireClaim(AuthClaimType.Permission, AuthPermissions.ShortTermCasualsEdit));

        options.AddPolicy(AuthPolicies.CanEditStudents, policy =>
            policy.RequireClaim(AuthClaimType.Permission, AuthPermissions.PartnerEdit));

        options.AddPolicy(AuthPolicies.CanManageAbsences, policy =>
            policy.RequireClaim(AuthClaimType.Permission, AuthPermissions.ReportsAbsencesNotify, AuthPermissions.ReportsAbsencesRun));

        options.AddPolicy(AuthPolicies.CanAddAwards, policy =>
            policy.RequireClaim(AuthClaimType.Permission, AuthPermissions.SchoolAdmin.Awards.Add));

        options.AddPolicy(AuthPolicies.CanManageAwards, policy =>
            policy.RequireClaim(AuthClaimType.Permission, AuthPermissions.SchoolAdmin.Awards.Manage));

        options.AddPolicy(AuthPolicies.CanViewAwardNominations, policy =>
            policy.RequireClaim(AuthClaimType.Permission, AuthPermissions.SchoolAdmin.Awards.View));

        return options;
    }
}
