namespace Constellation.Application.Models.Auth;
// ReSharper disable InconsistentNaming

public static class AuthClaimType
{
    public const string Permission = "Permission";
    public const string StaffEmployeeId = "StaffId";
    public const string SchoolCode = "SchoolCode";
    public const string UserName = "UserName";
    public const string StudentId = "StudentId";

    public const string IsImpersonating = "impersonation:active";
    public const string OriginalUserId = "impersonation:original_user_id";
    public const string OriginalUserName = "impersonation:original_user_name";
}