namespace Constellation.Application.Models.Auth;

using Constellation.Core.Common;

// ReSharper disable InconsistentNaming
#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix

public sealed class AuthPermission : StringEnumeration<AuthPermission>
{

#region const_def
    public const string Partners_Contacts_View_Value = "Partners.Contacts.View";
    public const string Partners_Schools_View_Value = "Partners.Schools.View";
    public const string Partners_Schools_Edit_Value = "Partners.Schools.Edit";
    public const string Partners_SchoolContacts_View_Value = "Partners.SchoolContacts.View";
    public const string Partners_SchoolContacts_Edit_Value = "Partners.SchoolContacts.Edit";
    public const string Partners_SchoolContacts_ShowPrincipals_Value = "Partner.SchoolContacts.ShowPrincipals";
    public const string Partners_Staff_View_Value = "Partners.Staff.View";
    public const string Partners_Staff_Edit_Value = "Partners.Staff.Edit";
    public const string Partners_Faculties_View_Value = "Partners.Faculties.View";
    public const string Partners_Faculties_Edit_Value = "Partners.Faculties.Edit";
    public const string Partners_Students_View_Value = "Partners.Students.View";
    public const string Partners_Students_Edit_Value = "Partners.Students.Edit";
    public const string Partners_Families_View_Value = "Partners.Families.View";
    public const string Partners_Families_Edit_Value = "Partners.Families.Edit";

    public const string Messaging_View_Value = "Messaging.View";
    public const string Messaging_SMS_Send_Value = "Messaging.SMS.Send";
    public const string Messaging_Email_Send_Value = "Messaging.Email.Send";

    public const string Subjects_Courses_View_Value = "Subjects.Courses.View";
    public const string Subjects_Courses_Edit_Value = "Subjects.Courses.Edit";
    public const string Subjects_Offerings_View_Value = "Subjects.Offerings.View";
    public const string Subjects_Offerings_Edit_Value = "Subjects.Offerings.Edit";
    public const string Subjects_Assignments_View_Value = "Subjects.Assignments.View";
    public const string Subjects_Assignments_Edit_Value = "Subjects.Assignments.Edit";
    public const string Subjects_Assignments_Submit_Value = "Subjects.Assignments.Submit";
    public const string Subjects_Timetables_View_Value = "Subjects.Timetables.View";
    public const string Subjects_Timetables_Edit_Value = "Subjects.Timetables.Edit";
    public const string Subjects_SciencePracs_View_Value = "Subjects.SciencePracs.View";
    public const string Subjects_SciencePracs_Edit_Value = "Subjects.SciencePracs.Edit";
    public const string Subjects_GroupTutorials_View_Value = "Subjects.GroupTutorials.View";
    public const string Subjects_GroupTutorials_Edit_Value = "Subjects.GroupTutorials.Edit";
    public const string Subjects_Tutorials_View_Value = "Subjects.Tutorials.View";
    public const string Subjects_Tutorials_Edit_Value = "Subjects.Tutorials.Edit";

    public const string ShortTerm_Casuals_View_Value = "ShortTerm.Casuals.View";
    public const string ShortTerm_Casuals_Edit_Value = "ShortTerm.Casuals.Edit";
    public const string ShortTerm_Covers_View_Value = "ShortTerm.Covers.View";
    public const string ShortTerm_Covers_Edit_Value = "ShortTerm.Covers.Edit";

    public const string StudentAdmin_AttendanceList_View_Value = "StudentAdmin.AttendanceList.View";
    public const string StudentAdmin_AttendanceList_Notify_Value = "StudentAdmin.AttendanceList.Notify";
    public const string StudentAdmin_AttendancePlans_View_Value = "StudentAdmin.AttendancePlans.View";
    public const string StudentAdmin_AttendancePlans_Edit_Value = "StudentAdmin.AttendancePlans.Edit";
    public const string StudentAdmin_AttendancePlans_Approve_Value = "StudentAdmin.AttendancePlans.Approve";
    public const string StudentAdmin_AttendanceSettings_View_Value = "StudentAdmin.AttendanceSettings.View";
    public const string StudentAdmin_AttendanceSettings_Edit_Value = "StudentAdmin.AttendanceSettings.Edit";
    public const string StudentAdmin_AttendanceReports_View_Value = "StudentAdmin.AttendanceReports.View";
    public const string StudentAdmin_Awards_View_Value = "StudentAdmin.Awards.View";
    public const string StudentAdmin_Awards_Edit_Value = "StudentAdmin.Awards.Edit";
    public const string StudentAdmin_Consent_View_Value = "StudentAdmin.Consent.View";
    public const string StudentAdmin_Consent_Edit_Value = "StudentAdmin.Consent.Edit";
    public const string StudentAdmin_Reports_View_Value = "StudentAdmin.Reports.View";
    public const string StudentAdmin_Reports_Edit_Value = "StudentAdmin.Reports.Edit";

    public const string SchoolAdmin_AwardNominations_View_Value = "SchoolAdmin.AwardNominations.View";
    public const string SchoolAdmin_AwardNominations_Edit_Value = "SchoolAdmin.AwardNominations.Edit";
    public const string SchoolAdmin_AwardNominations_Submit_Value = "SchoolAdmin.AwardNominations.Submit";
    public const string SchoolAdmin_AssessmentProvisions_Edit_Value = "SchoolAdmin.AssessmentProvisions.Edit";
    public const string SchoolAdmin_AttendancePercentages_View_Value = "SchoolAdmin.AttendancePercentages.View";
    public const string SchoolAdmin_AttendancePercentages_Edit_Value = "SchoolAdmin.AttendancePercentages.Edit";
    public const string SchoolAdmin_Compliance_View_Value = "SchoolAdmin.Compliance.View";
    public const string SchoolAdmin_MasterFile_View_Value = "SchoolAdmin.MasterFile.View";
    public const string SchoolAdmin_Training_ViewAll_Value = "SchoolAdmin.Training.ViewAll";
    public const string SchoolAdmin_Training_Edit_Value = "SchoolAdmin.Training.Edit";
    public const string SchoolAdmin_WorkFlow_View_Value = "SchoolAdmin.WorkFlow.View";
    public const string SchoolAdmin_WorkFlow_Edit_Value = "SchoolAdmin.WorkFlow.Edit";

    public const string Equipment_Assets_View_Value = "Equipment.Assets.View";
    public const string Equipment_Assets_Edit_Value = "Equipment.Assets.Edit";
    public const string Equipment_Stocktake_View_Value = "Equipment.Stocktake.View";
    public const string Equipment_Stocktake_Edit_Value = "Equipment.Stocktake.Edit";
    public const string Equipment_Stocktake_Submit_Value = "Equipment.Stocktake.Submit";

    public const string Admin_EmergencyConsole_Edit_Value = "Admin.EmergencyConsole.Edit";
    public const string Admin_Hosting_View_Value = "Admin.Hosting.View";
    public const string Admin_Hosting_Edit_Value = "Admin.Hosting.Edit";
    public const string Admin_Authentication_View_Value = "Admin.Authentication.View";
    public const string Admin_Authentication_Edit_Value = "Admin.Authentication.Edit";
    public const string Admin_Rollover_Edit_Value = "Admin.Rollover.Edit";
    public const string Admin_Automation_Edit_Value = "Admin.Automation.Edit";
    public const string Admin_Configuration_Edit_Value = "Admin.Configuration.Edit";

    public const string SchoolsPortal_View_Value = "SchoolsPortal.View";
    public const string SchoolsPortal_Contacts_View_Value = "SchoolsPortal.Contacts.View";
    public const string SchoolsPortal_Contacts_Edit_Value = "SchoolsPortal.Contacts.Edit";
    public const string SchoolsPortal_Absences_View_Value = "SchoolsPortal.Absences.View";
    public const string SchoolsPortal_Absences_Edit_Value = "SchoolsPortal.Absences.Edit";
    public const string SchoolsPortal_SciencePracs_View_Value = "SchoolsPortal.SciencePracs.View";
    public const string SchoolsPortal_SciencePracs_Edit_Value = "SchoolsPortal.SciencePracs.Edit";
    public const string SchoolsPortal_Reports_View_Value = "SchoolsPortal.Reports.View";
    public const string SchoolsPortal_Awards_View_Value = "SchoolsPortal.Awards.View";
    public const string SchoolsPortal_Exams_View_Value = "SchoolsPortal.Exams.View";
    public const string SchoolsPortal_Exams_Edit_Value = "SchoolsPortal.Exams.Edit";
    public const string SchoolsPortal_Timetables_View_Value = "SchoolsPortal.Timetables.View";
    public const string SchoolsPortal_Stocktake_View_Value = "SchoolsPortal.Stocktake.View";
    public const string SchoolsPortal_Stocktake_Edit_Value = "SchoolsPortal.Stocktake.Edit";

    public const string ParentPortal_View_Value = "ParentPortal.View";

    public const string StudentPortal_View_Value = "StudentPortal.View";
    #endregion

    public static readonly AuthPermission Partners_Schools_View = new(Partners_Schools_View_Value, "Partners: Schools: View");
    public static readonly AuthPermission Partners_Schools_Edit = new(Partners_Schools_Edit_Value, "Partners: Schools: Edit");
    public static readonly AuthPermission Partners_SchoolContacts_View = new(Partners_SchoolContacts_View_Value, "Partners: School Contacts: View");
    public static readonly AuthPermission Partners_SchoolContacts_Edit = new(Partners_SchoolContacts_Edit_Value, "Partners: School Contacts: Edit");
    public static readonly AuthPermission Partners_SchoolContacts_ShowPrincipals = new(Partners_SchoolContacts_ShowPrincipals_Value, "Partners: School Contacts: Show Principals");
    public static readonly AuthPermission Partners_Staff_View = new(Partners_Staff_View_Value, "Partners: Staff: View");
    public static readonly AuthPermission Partners_Staff_Edit = new(Partners_Staff_Edit_Value, "Partners: Staff: Edit");
    public static readonly AuthPermission Partners_Faculties_View = new(Partners_Faculties_View_Value, "Partners: Faculties: View");
    public static readonly AuthPermission Partners_Faculties_Edit = new(Partners_Faculties_Edit_Value, "Partners: Faculties: Edit");
    public static readonly AuthPermission Partners_Students_View = new(Partners_Students_View_Value, "Partners: Students: View");
    public static readonly AuthPermission Partners_Students_Edit = new(Partners_Students_Edit_Value, "Partners: Students: Edit");
    public static readonly AuthPermission Partners_Families_View = new(Partners_Families_View_Value, "Partners: Families: View");
    public static readonly AuthPermission Partners_Families_Edit = new(Partners_Families_Edit_Value, "Partners: Families: Edit");

    public static readonly AuthPermission Partners_Contacts_View = new(Partners_Contacts_View_Value, "Messaging: Contacts: View");
    public static readonly AuthPermission Messaging_View = new(Messaging_View_Value, "Messaging: View");
    public static readonly AuthPermission Messaging_SMS_Send = new(Messaging_SMS_Send_Value, "Messaging: SMS: Send");
    public static readonly AuthPermission Messaging_EMAIL_Send = new(Messaging_Email_Send_Value, "Messaging: Email: Send");

    public static readonly AuthPermission Subjects_Courses_View = new(Subjects_Courses_View_Value, "Subjects: Courses: View");
    public static readonly AuthPermission Subjects_Courses_Edit = new(Subjects_Courses_Edit_Value, "Subjects: Courses: Edit");
    public static readonly AuthPermission Subjects_Offerings_View = new(Subjects_Offerings_View_Value, "Subjects: Offerings: View");
    public static readonly AuthPermission Subjects_Offerings_Edit = new(Subjects_Offerings_Edit_Value, "Subjects: Offerings: Edit");
    public static readonly AuthPermission Subjects_Assignments_View = new(Subjects_Assignments_View_Value, "Subjects: Assignments: View");
    public static readonly AuthPermission Subjects_Assignments_Edit = new(Subjects_Assignments_Edit_Value, "Subjects: Assignments: Edit");
    public static readonly AuthPermission Subjects_Assignments_Submit = new(Subjects_Assignments_Submit_Value, "Subjects: Assignments: Submit");
    public static readonly AuthPermission Subjects_Timetables_View = new(Subjects_Timetables_View_Value, "Subjects: Timetables: View");
    public static readonly AuthPermission Subjects_Timetables_Edit = new(Subjects_Timetables_Edit_Value, "Subjects: Timetables: Edit");
    public static readonly AuthPermission Subjects_SciencePracs_View = new(Subjects_SciencePracs_View_Value, "Subjects: Science Pracs: View");
    public static readonly AuthPermission Subjects_SciencePracs_Edit = new(Subjects_SciencePracs_Edit_Value, "Subjects: Science Pracs: Edit");
    public static readonly AuthPermission Subjects_GroupTutorials_View = new(Subjects_GroupTutorials_View_Value, "Subjects: Group Tutorials: View");
    public static readonly AuthPermission Subjects_GroupTutorials_Edit = new(Subjects_GroupTutorials_Edit_Value, "Subjects: Group Tutorials: Edit");
    public static readonly AuthPermission Subjects_Tutorials_View = new(Subjects_Tutorials_View_Value, "Subjects: Tutorials: View");
    public static readonly AuthPermission Subjects_Tutorials_Edit = new(Subjects_Tutorials_Edit_Value, "Subjects: Tutorials: Edit");

    public static readonly AuthPermission ShortTerm_Casuals_View = new(ShortTerm_Casuals_View_Value, "ShortTerm: Casuals: View");
    public static readonly AuthPermission ShortTerm_Casuals_Edit = new(ShortTerm_Casuals_Edit_Value, "ShortTerm: Casuals: Edit");
    public static readonly AuthPermission ShortTerm_Covers_View = new(ShortTerm_Covers_View_Value, "ShortTerm: Covers: View");
    public static readonly AuthPermission ShortTerm_Covers_Edit = new(ShortTerm_Covers_Edit_Value, "ShortTerm: Covers: Edit");

    public static readonly AuthPermission StudentAdmin_AttendanceList_View = new(StudentAdmin_AttendanceList_View_Value, "StudentAdmin: Attendance List: View");
    public static readonly AuthPermission StudentAdmin_AttendanceList_Notify = new(StudentAdmin_AttendanceList_Notify_Value, "StudentAdmin: Attendance List: Notify");
    public static readonly AuthPermission StudentAdmin_AttendancePlans_View = new(StudentAdmin_AttendancePlans_View_Value, "StudentAdmin: Attendance Plans: View");
    public static readonly AuthPermission StudentAdmin_AttendancePlans_Edit = new(StudentAdmin_AttendancePlans_Edit_Value, "StudentAdmin: Attendance Plans: Edit");
    public static readonly AuthPermission StudentAdmin_AttendancePlans_Approve = new(StudentAdmin_AttendancePlans_Approve_Value, "StudentAdmin: Attendance Plans: Approve");
    public static readonly AuthPermission StudentAdmin_AttendanceSettings_View = new(StudentAdmin_AttendanceSettings_View_Value, "StudentAdmin: Attendance Settings: View");
    public static readonly AuthPermission StudentAdmin_AttendanceSettings_Edit = new(StudentAdmin_AttendanceSettings_Edit_Value, "StudentAdmin: Attendance Settings: Edit");
    public static readonly AuthPermission StudentAdmin_AttendanceReports_View = new(StudentAdmin_AttendanceReports_View_Value, "StudentAdmin: Attendance Reports: View");
    public static readonly AuthPermission StudentAdmin_Awards_View = new(StudentAdmin_Awards_View_Value, "StudentAdmin: Awards: View");
    public static readonly AuthPermission StudentAdmin_Awards_Edit = new(StudentAdmin_Awards_Edit_Value, "StudentAdmin: Awards: Edit");
    public static readonly AuthPermission StudentAdmin_Consent_View = new(StudentAdmin_Consent_View_Value, "StudentAdmin: Consent: View");
    public static readonly AuthPermission StudentAdmin_Consent_Edit = new(StudentAdmin_Consent_Edit_Value, "StudentAdmin: Consent: Edit");
    public static readonly AuthPermission StudentAdmin_Reports_View = new(StudentAdmin_Reports_View_Value, "StudentAdmin: Reports: View");
    public static readonly AuthPermission StudentAdmin_Reports_Edit = new(StudentAdmin_Reports_Edit_Value, "StudentAdmin: Reports: Edit");

    public static readonly AuthPermission SchoolAdmin_AwardNominations_View = new(SchoolAdmin_AwardNominations_View_Value, "SchoolAdmin: Award Nominations: View");
    public static readonly AuthPermission SchoolAdmin_AwardNominations_Edit = new(SchoolAdmin_AwardNominations_Edit_Value, "SchoolAdmin: Award Nominations: Edit");
    public static readonly AuthPermission SchoolAdmin_AwardNominations_Submit = new(SchoolAdmin_AwardNominations_Submit_Value, "SchoolAdmin: Award Nominations: Submit");
    public static readonly AuthPermission SchoolAdmin_AssessmentProvisions_Edit = new(SchoolAdmin_AssessmentProvisions_Edit_Value, "SchoolAdmin: Assessment Provisions: Edit");
    public static readonly AuthPermission SchoolAdmin_AttendancePercentages_View = new(SchoolAdmin_AttendancePercentages_View_Value, "SchoolAdmin: Attendance Percentages: View");
    public static readonly AuthPermission SchoolAdmin_AttendancePercentages_Edit = new(SchoolAdmin_AttendancePercentages_Edit_Value, "SchoolAdmin: Attendance Percentages: Edit");
    public static readonly AuthPermission SchoolAdmin_Compliance_View = new(SchoolAdmin_Compliance_View_Value, "SchoolAdmin: N-Award Compliance: View");
    public static readonly AuthPermission SchoolAdmin_MasterFile_View = new(SchoolAdmin_MasterFile_View_Value, "SchoolAdmin: MasterFile: View");
    public static readonly AuthPermission SchoolAdmin_Training_ViewAll = new(SchoolAdmin_Training_ViewAll_Value, "SchoolAdmin: Mandatory Training: View All");
    public static readonly AuthPermission SchoolAdmin_Training_Edit = new(SchoolAdmin_Training_Edit_Value, "SchoolAdmin: Mandatory Training: Edit");
    public static readonly AuthPermission SchoolAdmin_WorkFlow_View = new(SchoolAdmin_WorkFlow_View_Value, "SchoolAdmin: WorkFlow: View");
    public static readonly AuthPermission SchoolAdmin_WorkFlow_Edit = new(SchoolAdmin_WorkFlow_Edit_Value, "SchoolAdmin: WorkFlow: Edit");

    public static readonly AuthPermission Equipment_Assets_View = new(Equipment_Assets_View_Value, "Equipment: Assets: View");
    public static readonly AuthPermission Equipment_Assets_Edit = new(Equipment_Assets_Edit_Value, "Equipment: Assets: Edit");
    public static readonly AuthPermission Equipment_Stocktake_View = new(Equipment_Stocktake_View_Value, "Equipment: Stocktake: View");
    public static readonly AuthPermission Equipment_Stocktake_Edit = new(Equipment_Stocktake_Edit_Value, "Equipment: Stocktake: Edit");
    public static readonly AuthPermission Equipment_Stocktake_Submit = new(Equipment_Stocktake_Submit_Value, "Equipment: Stocktake: Submit");

    public static readonly AuthPermission Admin_EmergencyConsole_Edit = new(Admin_EmergencyConsole_Edit_Value, "Admin: Emergency Console: Edit");
    public static readonly AuthPermission Admin_Hosting_View = new(Admin_Hosting_View_Value, "Admin: Hosting: View");
    public static readonly AuthPermission Admin_Hosting_Edit = new(Admin_Hosting_Edit_Value, "Admin: Hosting: Edit");
    public static readonly AuthPermission Admin_Authentication_View = new(Admin_Authentication_View_Value, "Admin: Authentication: View");
    public static readonly AuthPermission Admin_Authentication_Edit = new(Admin_Authentication_Edit_Value, "Admin: Authentication: Edit");
    public static readonly AuthPermission Admin_Rollover_Edit = new(Admin_Rollover_Edit_Value, "Admin: Rollover: Edit");
    public static readonly AuthPermission Admin_Automation_Edit = new(Admin_Automation_Edit_Value, "Admin: Automation: Edit");
    public static readonly AuthPermission Admin_Configuration_Edit = new(Admin_Configuration_Edit_Value, "Admin: Configuration: Edit");

    public static readonly AuthPermission SchoolsPortal_View = new(SchoolsPortal_View_Value, "SchoolsPortal: View");
    public static readonly AuthPermission SchoolsPortal_Contacts_View = new(SchoolsPortal_Contacts_View_Value, "SchoolsPortal: Contacts: View");
    public static readonly AuthPermission SchoolsPortal_Contacts_Edit = new(SchoolsPortal_Contacts_Edit_Value, "SchoolsPortal: Contacts: Edit");
    public static readonly AuthPermission SchoolsPortal_Absences_View = new(SchoolsPortal_Absences_View_Value, "SchoolsPortal: Absences: View");
    public static readonly AuthPermission SchoolsPortal_Absences_Edit = new(SchoolsPortal_Absences_Edit_Value, "SchoolsPortal: Absences: Edit");
    public static readonly AuthPermission SchoolsPortal_SciencePracs_View = new(SchoolsPortal_SciencePracs_View_Value, "SchoolsPortal: Science Pracs: View");
    public static readonly AuthPermission SchoolsPortal_SciencePracs_Edit = new(SchoolsPortal_SciencePracs_Edit_Value, "SchoolsPortal: Science Pracs: Edit");
    public static readonly AuthPermission SchoolsPortal_Reports_View = new(SchoolsPortal_Reports_View_Value, "SchoolsPortal: Reports: View");
    public static readonly AuthPermission SchoolsPortal_Awards_View = new(SchoolsPortal_Awards_View_Value, "SchoolsPortal: Awards: View");
    public static readonly AuthPermission SchoolsPortal_Exams_View = new(SchoolsPortal_Exams_View_Value, "SchoolsPortal: Exams: View");
    public static readonly AuthPermission SchoolsPortal_Exams_Edit = new(SchoolsPortal_Exams_Edit_Value, "SchoolsPortal: Exams: Edit");
    public static readonly AuthPermission SchoolsPortal_Timetables_View = new(SchoolsPortal_Timetables_View_Value, "SchoolsPortal: Timetables: View");
    public static readonly AuthPermission SchoolsPortal_Stocktake_View = new(SchoolsPortal_Stocktake_View_Value, "SchoolsPortal: Stocktake: View");
    public static readonly AuthPermission SchoolsPortal_Stocktake_Edit = new(SchoolsPortal_Stocktake_Edit_Value, "SchoolsPortal: Stocktake: Edit");

    public static readonly AuthPermission ParentPortal_View = new(ParentPortal_View_Value, "ParentPortal: View");

    public static readonly AuthPermission StudentPortal_View = new(StudentPortal_View_Value, "StudentPortal: View");


    private AuthPermission(string value, string name)
        : base(value, name) { }

    public static IEnumerable<AuthPermission> GetOptions => GetEnumerable;

    public override string ToString() => Value;

    public static implicit operator string(AuthPermission permission) => permission.ToString();
}
