namespace Constellation.Application.Models.Auth;

public static class AuthPolicies
{
    public const string HasPermission = "HasPermission";
    public const string IsSchoolContact = "IsSchoolContact";
    public const string IsParent = "IsParent";
    public const string IsStudent = "IsStudent";
    public const string IsStaffMember = "IsStaffMember";
    public const string CanViewTrainingCompletionRecord = "CanViewTrainingCompletionRecord";
    public const string CanSubmitGroupTutorialRolls = "CanSubmitGroupTutorialRolls";
    public const string IsSiteAdmin = "IsSiteAdmin";
    public const string CanEditWorkFlowAction = "CanEditWorkFlowAction";
}