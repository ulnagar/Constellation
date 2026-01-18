namespace Constellation.Infrastructure.Identity.Authorization;

using Application.Models.Identity;
using Constellation.Application.Models.Auth;
using DependencyInjection;
using Microsoft.AspNetCore.Authorization;

public static class AuthPolicyDefinitions
{
    public static AuthorizationOptions AddApplicationPolicies(this AuthorizationOptions options)
    {
        // Site Super Admin
        options.AddPolicy(AuthPolicies.IsSiteAdmin, policy =>
            policy.RequireRole(AppRole.SuperAdminRole));
        
        // Has active School Contact Role
        options.AddPolicy(AuthPolicies.IsSchoolContact, policy =>
            policy.Requirements.Add(new CanAccessSchoolPortalRequirement()));

        // Has active Student linked to Family
        options.AddPolicy(AuthPolicies.IsParent, policy =>
            policy.Requirements.Add(new IsParentOfCurrentStudentRequirement()));

        // Has active Staff Member record
        options.AddPolicy(AuthPolicies.IsStaffMember, policy =>
            policy.Requirements.Add(new IsCurrentStaffMemberRequirement()));

        // Has active Student record
        options.AddPolicy(AuthPolicies.IsStudent, policy =>
            policy.Requirements.Add(new IsCurrentStudentRequirement()));

        options.AddPolicy(AuthPolicies.CanViewTrainingCompletionRecord, policy =>
            policy.Requirements.Add(new CanViewTrainingCompletionRecordRequirement()));
        
        options.AddPolicy(AuthPolicies.CanSubmitGroupTutorialRolls, policy =>
            policy.Requirements.Add(new CanSubmitGroupTutorialRollRequirement()));
        
        options.AddPolicy(AuthPolicies.CanEditWorkFlowAction, policy =>
            policy.Requirements.Add(new CanEditWorkFlowActionRequirement()));
       
        return options;
    }
}
