﻿namespace Constellation.Application.Models.Auth;

public class AuthPolicies
{
    public const string CanViewTrainingCompletionRecord = "CanViewTrainingCompletionRecord";
    public const string CanEditTrainingModuleContent = "CanManageTrainingModule";
    public const string CanViewTrainingModuleContent = "CanViewTrainingModuleContent";
    public const string CanViewTrainingModuleContentDetails = "CanViewTrainingModuleContentDetails";
    public const string CanRunTrainingModuleReports = "CanRunTrainingModuleReports";
    public const string IsStaffMember = "IsStaffMember";
}