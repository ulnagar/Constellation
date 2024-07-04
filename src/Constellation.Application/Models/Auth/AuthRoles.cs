namespace Constellation.Application.Models.Auth;

public static class AuthRoles
{
    public const string Admin = "Administrator";
    
    // Staff Portal roles
    public const string Editor = "Editor";
    public const string StaffMember = "User";
    public const string CoverEditor = "Covers Editor";
    public const string EquipmentEditor = "Equipment Editor";
    public const string LessonsEditor = "Lessons Editor";
    public const string AbsencesEditor = "Absences Editor";
    public const string CoverRecipient = "Cover Email Recipient";
    public const string MandatoryTrainingEditor = "Mandatory Training Editor";
    public const string GroupTutorialsEditor = "Group Tutorials Editor";
    public const string AwardsManager = "Awards Manager";
    public const string ComplianceManager = "Compliance Manager";
    public const string ExecStaffMember = "Exec Staff Member";
    
    // School Portal roles
    public const string SchoolContact = "Lessons User";
}