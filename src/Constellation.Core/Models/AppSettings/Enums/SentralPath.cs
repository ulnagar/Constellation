namespace Constellation.Core.Models.AppSettings.Enums;

using Core.Common;

public sealed class SentralPath : StringEnumeration<SentralPath>
{
    public static readonly SentralPath FamilyEmail = new("Family Email");
    public static readonly SentralPath AbsenceTable = new("Absence Table");
    public static readonly SentralPath StudentTable = new("Student Table");
    public static readonly SentralPath PartialAbsenceTable = new("Partial Absence Table");
    public static readonly SentralPath CalendarTable = new("Calendar Table");
    public static readonly SentralPath TermCalendarTable = new("Term Calendar Table");
    public static readonly SentralPath WellbeingStudentAwardsList = new("Wellbeing Student Awards List");
    public static readonly SentralPath IncidentCreatedDate = new("Incident Created Date");
    public static readonly SentralPath IndigenousStatus = new("Indigenous Status");
    public static readonly SentralPath StudentSRNTable = new("Student SRN Table");
    public static readonly SentralPath StudentEnrolmentDates = new("Student Enrolment Dates");

    private SentralPath(string name)
        : base (name, name) { }
}