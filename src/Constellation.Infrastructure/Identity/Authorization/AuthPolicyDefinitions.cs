namespace Constellation.Infrastructure.Identity.Authorization;

using Constellation.Application.Models.Auth;
using Microsoft.AspNetCore.Authorization;

public static class AuthPolicyDefinitions
{
    public static AuthorizationOptions AddApplicationPolicies(this AuthorizationOptions options)
    {
        // Determines access to the Schools Portal
        options.AddPolicy(AuthPolicies.IsSchoolContact, policy =>
            policy.Requirements.Add(new CanAccessSchoolPortalRequirement()));

        // Determines access to the Parent Portal
        options.AddPolicy(AuthPolicies.IsParent, policy =>
            policy.Requirements.Add(new IsParentOfCurrentStudentRequirement()));

        // Determines access to the Staff Portal
        options.AddPolicy(AuthPolicies.IsStaffMember, policy =>
            policy.Requirements.Add(new IsCurrentStaffMemberRequirement()));

        // Determines access to the Student Portal
        options.AddPolicy(AuthPolicies.IsStudent, policy =>
            policy.Requirements.Add(new IsCurrentStudentRequirement()));

        options.AddPolicy(AuthPolicies.CanViewTrainingCompletionRecord, policy =>
            policy.Requirements.Add(new CanViewTrainingCompletionRecordRequirement()));
        
        options.AddPolicy(AuthPolicies.CanSubmitGroupTutorialRolls, policy =>
            policy.Requirements.Add(new CanSubmitGroupTutorialRollRequirement()));

        options.AddPolicy(AuthPolicies.IsSiteAdmin, policy =>
            policy.RequireRole(AuthRoles.Admin));

        options.AddPolicy(AuthPolicies.CanEditWorkFlowAction, policy =>
            policy.Requirements.Add(new CanEditWorkFlowActionRequirement()));
        
        return options;
    }
}
