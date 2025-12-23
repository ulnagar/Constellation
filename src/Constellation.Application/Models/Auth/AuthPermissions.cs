namespace Constellation.Application.Models.Auth;

using Constellation.Core.Common;

// ReSharper disable InconsistentNaming
#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix

public sealed class AuthPermission : StringEnumeration<AuthPermission>
{
    public static readonly AuthPermission Partners_Contacts_View = new("Partners.Contacts.View", "Partners: Contacts: View");
    public static readonly AuthPermission Partners_Schools_View = new("Partners.Schools.View", "Partners: Schools: View");
    public static readonly AuthPermission Partners_Schools_Edit = new("Partners.Schools.Edit", "Partners: Schools: Edit");
    public static readonly AuthPermission Partners_SchoolContacts_View = new("Partners.SchoolContacts.View", "Partners: School Contacts: View");
    public static readonly AuthPermission Partners_SchoolContacts_Edit = new("Partners.SchoolContacts.Edit", "Partners: School Contacts: Edit");
    public static readonly AuthPermission Partners_SchoolContacts_ShowPrincipals = new("Partner.SchoolContacts.ShowPrincipals", "Partners: School Contacts: Show Principals");
    public static readonly AuthPermission Partners_Staff_View = new("Partners.Staff.View", "Partners: Staff: View");
    public static readonly AuthPermission Partners_Staff_Edit = new("Partners.Staff.Edit", "Partners: Staff: Edit");
    public static readonly AuthPermission Partners_Faculties_View = new("Partners.Faculties.View", "Partners: Faculties: View");
    public static readonly AuthPermission Partners_Faculties_Edit = new("Partners.Faculties.Edit", "Partners: Faculties: Edit");
    public static readonly AuthPermission Partners_Students_View = new("Partners.Students.View", "Partners: Students: View");
    public static readonly AuthPermission Partners_Students_Edit = new("Partners.Students.Edit", "Partners: Students: Edit");
    public static readonly AuthPermission Partners_Families_View = new("Partners.Families.View", "Partners: Families: View");
    public static readonly AuthPermission Partners_Families_Edit = new("Partners.Families.Edit", "Partners: Families: Edit");

    public static readonly AuthPermission Subjects_Courses_View = new("Subjects.Courses.View", "Subjects: Courses: View");
    public static readonly AuthPermission Subjects_Courses_Edit = new("Subjects.Courses.Edit", "Subjects: Courses: Edit");
    public static readonly AuthPermission Subjects_Offerings_View = new("Subjects.Offerings.View", "Subjects: Offerings: View");
    public static readonly AuthPermission Subjects_Offerings_Edit = new("Subjects.Offerings.Edit", "Subjects: Offerings: Edit");
    public static readonly AuthPermission Subjects_Assignments_View = new("Subjects.Assignments.View", "Subjects: Assignments: View");
    public static readonly AuthPermission Subjects_Assignments_Edit = new("Subjects.Assignments.Edit", "Subjects: Assignments: Edit");
    public static readonly AuthPermission Subjects_Assignments_Submit = new("Subjects.Assignments.Submit", "Subjects: Assignments: Submit");
    public static readonly AuthPermission Subjects_Timetables_View = new("Subjects.Timetables.View", "Subjects: Timetables: View");
    public static readonly AuthPermission Subjects_Timetables_Edit = new("Subjects.Timetables.Edit", "Subjects: Timetables: Edit");
    public static readonly AuthPermission Subjects_SciencePracs_View = new("Subjects.SciencePracs.View", "Subjects: Science Pracs: View");
    public static readonly AuthPermission Subjects_SciencePracs_Edit = new("Subjects.SciencePracs.Edit", "Subjects: Science Pracs: Edit");
    public static readonly AuthPermission Subjects_GroupTutorials_View = new("Subjects.GroupTutorials.View", "Subjects: Group Tutorials: View");
    public static readonly AuthPermission Subjects_GroupTutorials_Edit = new("Subjects.GroupTutorials.Edit", "Subjects: Group Tutorials: Edit");
    public static readonly AuthPermission Subjects_Tutorials_View = new("Subjects.Tutorials.View", "Subjects: Tutorials: View");
    public static readonly AuthPermission Subjects_Tutorials_Edit = new("Subjects.Tutorials.Edit", "Subjects: Tutorials: Edit");

    public static readonly AuthPermission ShortTerm_Casuals_View = new("ShortTerm.Casuals.View", "ShortTerm: Casuals: View");
    public static readonly AuthPermission ShortTerm_Casuals_Edit = new("ShortTerm.Casuals.Edit", "ShortTerm: Casuals: Edit");
    public static readonly AuthPermission ShortTerm_Covers_View = new("ShortTerm.Covers.View", "ShortTerm: Covers: View");
    public static readonly AuthPermission ShortTerm_Covers_Edit = new("ShortTerm.Covers.Edit", "ShortTerm: Covers: Edit");

    public static readonly AuthPermission StudentAdmin_AttendanceList_View = new("StudentAdmin.AttendanceList.View", "StudentAdmin: Attendance List: View");
    public static readonly AuthPermission StudentAdmin_AttendanceList_Notify = new("StudentAdmin.AttendanceList.Notify", "StudentAdmin: Attendance List: Notify");
    public static readonly AuthPermission StudentAdmin_AttendancePlans_View = new("StudentAdmin.AttendancePlans.View", "StudentAdmin: Attendance Plans: View");
    public static readonly AuthPermission StudentAdmin_AttendancePlans_Edit = new("StudentAdmin.AttendancePlans.Edit", "StudentAdmin: Attendance Plans: Edit");
    public static readonly AuthPermission StudentAdmin_AttendancePlans_Approve = new("StudentAdmin.AttendancePlans.Approve", "StudentAdmin: Attendance Plans: Approve");
    public static readonly AuthPermission StudentAdmin_AttendanceSettings_View = new("StudentAdmin.AttendanceSettings.View", "StudentAdmin: Attendance Settings: View");
    public static readonly AuthPermission StudentAdmin_AttendanceSettings_Edit = new("StudentAdmin.AttendanceSettings.Edit", "StudentAdmin: Attendance Settings: Edit");
    public static readonly AuthPermission StudentAdmin_AttendanceReports_View = new("StudentAdmin.AttendanceReports.View", "StudentAdmin: Attendance Reports: View");
    public static readonly AuthPermission StudentAdmin_Awards_View = new("StudentAdmin.Awards.View", "StudentAdmin: Awards: View");
    public static readonly AuthPermission StudentAdmin_Awards_Edit = new("StudentAdmin.Awards.Edit", "StudentAdmin: Awards: Edit");
    public static readonly AuthPermission StudentAdmin_Consent_View = new("StudentAdmin.Consent.View", "StudentAdmin: Consent: View");
    public static readonly AuthPermission StudentAdmin_Consent_Edit = new("StudentAdmin.Consent.Edit", "StudentAdmin: Consent: Edit");
    public static readonly AuthPermission StudentAdmin_Reports_View = new("StudentAdmin.Reports.View", "StudentAdmin: Reports: View");
    public static readonly AuthPermission StudentAdmin_Reports_Edit = new("StudentAdmin.Reports.Edit", "StudentAdmin: Reports: Edit");

    public static readonly AuthPermission SchoolAdmin_AwardNominations_View = new("SchoolAdmin.AwardNominations.View", "SchoolAdmin: Award Nominations: View");
    public static readonly AuthPermission SchoolAdmin_AwardNominations_Edit = new("SchoolAdmin.AwardNominations.Edit", "SchoolAdmin: Award Nominations: Edit");
    public static readonly AuthPermission SchoolAdmin_AwardNominations_Submit = new("SchoolAdmin.AwardNominations.Submit", "SchoolAdmin: Award Nominations: Submit");
    public static readonly AuthPermission SchoolAdmin_AssessmentProvisions_Edit = new("SchoolAdmin.AssessmentProvisions.Edit", "SchoolAdmin: Assessment Provisions: Edit");
    public static readonly AuthPermission SchoolAdmin_AttendancePercentages_View = new("SchoolAdmin.AttendancePercentages.View", "SchoolAdmin: Attendance Percentages: View");
    public static readonly AuthPermission SchoolAdmin_AttendancePercentages_Edit = new("SchoolAdmin.AttendancePercentages.Edit", "SchoolAdmin: Attendance Percentages: Edit");
    public static readonly AuthPermission SchoolAdmin_Compliance_View = new("SchoolAdmin.Compliance.View", "SchoolAdmin: N-Award Compliance: View");
    public static readonly AuthPermission SchoolAdmin_MasterFile_View = new("SchoolAdmin.MasterFile.View", "SchoolAdmin: MasterFile: View");
    public static readonly AuthPermission SchoolAdmin_Training_ViewAll = new("SchoolAdmin.Training.ViewAll", "SchoolAdmin: Mandatory Training: View All");
    public static readonly AuthPermission SchoolAdmin_Training_Edit = new("SchoolAdmin.Training.Edit", "SchoolAdmin: Mandatory Training: Edit");
    public static readonly AuthPermission SchoolAdmin_WorkFlow_View = new("SchoolAdmin.WorkFlow.View", "SchoolAdmin: WorkFlow: View");
    public static readonly AuthPermission SchoolAdmin_WorkFlow_Edit = new("SchoolAdmin.WorkFlow.Edit", "SchoolAdmin: WorkFlow: Edit");

    public static readonly AuthPermission Equipment_Assets_View = new("Equipment.Assets.View", "Equipment: Assets: View");
    public static readonly AuthPermission Equipment_Assets_Edit = new("Equipment.Assets.Edit", "Equipment: Assets: Edit");
    public static readonly AuthPermission Equipment_Stocktake_View = new("Equipment.Stocktake.View", "Equipment: Stocktake: View");
    public static readonly AuthPermission Equipment_Stocktake_Edit = new("Equipment.Stocktake.Edit", "Equipment: Stocktake: Edit");

    public static readonly AuthPermission Admin_EmergencyConsole_Edit = new("Admin.EmergencyConsole.Edit", "Admin: Emergency Console: Edit");
    public static readonly AuthPermission Admin_Hosting_View = new("Admin.Hosting.View", "Admin: Hosting: View");
    public static readonly AuthPermission Admin_Hosting_Edit = new("Admin.Hosting.Edit", "Admin: Hosting: Edit");
    public static readonly AuthPermission Admin_Authentication_View = new("Admin.Authentication.View", "Admin: Authentication: View");
    public static readonly AuthPermission Admin_Authentication_Edit = new("Admin.Authentication.Edit", "Admin: Authentication: Edit");
    public static readonly AuthPermission Admin_Rollover_Edit = new("Admin.Rollover.Edit", "Admin: Rollover: Edit");
    public static readonly AuthPermission Admin_Automation_Edit = new("Admin.Automation.Edit", "Admin: Automation: Edit");

    public static readonly AuthPermission SchoolsPortal_View = new("SchoolsPortal.View", "SchoolsPortal: View");
    public static readonly AuthPermission SchoolsPortal_Contacts_View = new("SchoolsPortal.Contacts.View", "SchoolsPortal: Contacts: View");
    public static readonly AuthPermission SchoolsPortal_Contacts_Edit = new("SchoolsPortal.Contacts.Edit", "SchoolsPortal: Contacts: Edit");
    public static readonly AuthPermission SchoolsPortal_Absences_View = new("SchoolsPortal.Absences.View", "SchoolsPortal: Absences: View");
    public static readonly AuthPermission SchoolsPortal_Absences_Edit = new("SchoolsPortal.Absences.Edit", "SchoolsPortal: Absences: Edit");
    public static readonly AuthPermission SchoolsPortal_SciencePracs_View = new("SchoolsPortal.SciencePracs.View", "SchoolsPortal: Science Pracs: View");
    public static readonly AuthPermission SchoolsPortal_SciencePracs_Edit = new("SchoolsPortal.SciencePracs.Edit", "SchoolsPortal: Science Pracs: Edit");
    public static readonly AuthPermission SchoolsPortal_Reports_View = new("SchoolsPortal.Reports.View", "SchoolsPortal: Reports: View");
    public static readonly AuthPermission SchoolsPortal_Awards_View = new("SchoolsPortal.Awards.View", "SchoolsPortal: Awards: View");
    public static readonly AuthPermission SchoolsPortal_Exams_View = new("SchoolsPortal.Exams.View", "SchoolsPortal: Exams: View");
    public static readonly AuthPermission SchoolsPortal_Exams_Edit = new("SchoolsPortal.Exams.Edit", "SchoolsPortal: Exams: Edit");
    public static readonly AuthPermission SchoolsPortal_Timetables_View = new("SchoolsPortal.Timetables.View", "SchoolsPortal: Timetables: View");
    public static readonly AuthPermission SchoolsPortal_Stocktake_View = new("SchoolsPortal.Stocktake.View", "SchoolsPortal: Stocktake: View");
    public static readonly AuthPermission SchoolsPortal_Stocktake_Edit = new("SchoolsPortal.Stocktake.Edit", "SchoolsPortal: Stocktake: Edit");

    public static readonly AuthPermission ParentPortal_View = new("ParentPortal.View", "ParentPortal: View");

    public static readonly AuthPermission StudentPortal_View = new("StudentPortal.View", "StudentPortal: View");


    private AuthPermission(string value, string name)
        : base(value, name) { }

    public static IEnumerable<AuthPermission> GetOptions => GetEnumerable;

    public override string ToString() => Value;

    public static implicit operator string(AuthPermission permission) => permission.ToString();
}
