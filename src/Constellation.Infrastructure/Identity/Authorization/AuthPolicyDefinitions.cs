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

        return options;
    }
}
