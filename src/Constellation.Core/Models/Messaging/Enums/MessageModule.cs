namespace Constellation.Core.Models.Messaging.Enums;

using Common;

public sealed class MessageModule : StringEnumeration<MessageModule>
{
    public static readonly MessageModule Empty = new("");
    public static readonly MessageModule System = new("System");

    public static readonly MessageModule Absences = new("Absences");
    public static readonly MessageModule AssessmentProvisions = new("Assessment Provisions");
    public static readonly MessageModule Assessments = new("Assessments");
    public static readonly MessageModule Attendance = new("Attendance");
    public static readonly MessageModule AttendancePlans = new("Attendance Plans");
    public static readonly MessageModule Authentication = new("Authentication");
    public static readonly MessageModule Awards = new("Awards");
    public static readonly MessageModule Covers = new("Covers");
    public static readonly MessageModule EnrolmentsPortal = new("Enrolments");
    public static readonly MessageModule ExternalReports = new("External Reports");
    public static readonly MessageModule Messaging = new("Messaging");
    public static readonly MessageModule RollMarking = new("RollMarking");
    public static readonly MessageModule ScheduledReport = new("Scheduled Report");
    public static readonly MessageModule SchoolContact = new("School Contact");
    public static readonly MessageModule SciencePrac = new("Science Practicals");
    public static readonly MessageModule AcademicReports = new("Student Reports");
    public static readonly MessageModule ThirdPartyConsent = new("Third Party Consent");
    public static readonly MessageModule TutorialSupport = new("Tutorial Support");
    public static readonly MessageModule WorkFlows = new("WorkFlows");

    /// <summary>
    /// No longer used. For historic purposes only.
    /// </summary>
    public static readonly MessageModule Assignments = new("Assignments");
    
    private MessageModule(string value)
        : base(value, value) { }
    
    public static implicit operator string(MessageModule module) => module.Value;
}