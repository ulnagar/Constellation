namespace Constellation.Core.Models.AppSettings.Enums;

using Common;

public sealed class SentralPath : StringEnumeration<SentralPath>
{
    public static readonly SentralPath FamilyEmail = new("Family Email");
    public static readonly SentralPath Parent1Name = new("Parent 1 Name");
    public static readonly SentralPath Parent1Mobile = new("Parent 1 Mobile");
    public static readonly SentralPath Parent1Email = new("Parent 1 Email");
    public static readonly SentralPath Parent2Name = new("Parent 2 Name");
    public static readonly SentralPath Parent2Mobile = new("Parent 2 Mobile");
    public static readonly SentralPath Parent2Email = new("Parent 2 Email");
    public static readonly SentralPath FamilyName = new("Family Name");
    public static readonly SentralPath AbsenceTable = new("Absence Table");
    public static readonly SentralPath StudentTable = new("Student Table");
    public static readonly SentralPath PartialAbsenceTable = new("Partial Absence Table");
    public static readonly SentralPath CalendarTable = new("Calendar Table");
    public static readonly SentralPath TermCalendarTable = new("Term Calendar Table");
    public static readonly SentralPath WellbeingStudentAwardsList = new("Wellbeing Student Awards List");
    public static readonly SentralPath IncidentCreatedDate = new("Incident Created Date");
    public static readonly SentralPath IndigenousStatus = new("Indigenous Status"); // "//*[@id=\"expander-content-1\"]/table/tr/td[1]/table/tr[7]/td"
    public static readonly SentralPath StudentSRNTable = new("Student SRN Table"); // "/html/body/div[8]/div/div[2]/div[3]/div/div/div/div[2]/table"
    public static readonly SentralPath StudentEnrolmentDates = new("Student Enrolment Dates"); // @"//*[contains(@class, 'pxp-roll')]"

    private SentralPath(string name)
        : base (name, name) { }
}