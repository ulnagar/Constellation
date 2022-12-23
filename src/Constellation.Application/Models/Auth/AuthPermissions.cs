namespace Constellation.Application.Models.Auth;

public class AuthPermissions
{
    public const string PartnerView = "Partner.View";
    public const string PartnerDetailsView = "Partner.View.Details";
    public const string PartnerEdit = "Partner.Edit";
    public const string ContactsEdit = "Partner.Edit.Contacts";

    public const string MandatoryTrainingView = "MandatoryTraining.View";
    public const string MandatoryTrainingDetailsView = "MandatoryTraining.View.Details";
    public const string MandatoryTrainingEdit = "MandatoryTraining.Edit";
    public const string MandatoryTrainingReportsRun = "MandatoryTraining.Reports";
    public const string MandatoryTrainingBulkDownload = "MandatoryTraining.Reports.Bulk";

    public const string SubjectsView = "Subjects.View";
    public const string SubjectsDetailsView = "Subjects.View.Details";
    public const string SubjectsEdit = "Subjects.Edit";
    public const string AssignmentsEdit = "Subjects.Edit.Assignments";
    public const string AssignmentsSubmit = "Subjects.Edit.Assignments.Submit";

    public const string ShortTermView = "ShortTerm.View";
    public const string ShortTermDetailsView = "ShortTerm.View.Details";
    public const string ShortTermEdit = "ShortTerm.Edit";
    public const string ShortTermCasualsEdit = "ShortTerm.Edit.Casuals";
    public const string ShortTermCoversEdit = "ShortTerm.Edit.Covers";
    public const string ShortTermOperationsRun = "ShortTerm.Run.Operations";

    public const string LessonsView = "Lessons.View";
    public const string LessonsEdit = "Lessons.Edit";

    public const string EquipmentView = "Equipment.View";
    public const string EquipmentEdit = "Equipment.Edit";
    public const string StocktakeView = "Equipment.View.Stocktake";
    public const string StocktakeEdit = "Equipment.Edit.Stocktake";

    public const string ReportsAwardsView = "Reports.View.Awards";
    public const string ReportsAwardsRun = "Reports.Run.Awards";
    public const string ReportsStudentAttendanceRun = "Reports.Run.Attendance";
    public const string ReportsFTERun = "Reports.Run.FTE";
    public const string ReportsPTOSetupRun = "Reports.Run.PTOSetup";
    public const string ReportsAbsencesRun = "Reports.Run.Absences";
    public const string ReportsAbsencesNotify = "Reports.Run.Absences.Notify";

    public const string PortalParentsAdmin = "Portal.Parents.Admin";
    public const string PortalSchoolsAdmin = "Portal.Schools.Admin";

    public const string UtilityAdmin = "Utility.Admin";

    public const string GroupTutorialsView = "GroupTutorials.View";
    public const string GroupTutorialsEdit = "GroupTutorials.Edit";
}