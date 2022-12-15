namespace Constellation.Core.Models;

using Constellation.Core.Enums;

public class Student
{
    public string StudentId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PortalUsername { get; set; } = string.Empty;
    public string AdobeConnectPrincipalId { get; set; } = string.Empty;
    public string SentralStudentId { get; set; } = string.Empty;
    public Grade CurrentGrade { get; set; }
    public Grade EnrolledGrade { get; set; }
    public string Gender { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime? DateDeleted { get; set; }
    public DateTime? DateEntered { get; set; } = DateTime.Now;
    public string SchoolCode { get; set; } = string.Empty;
    public virtual School? School { get; set; }
    public bool IncludeInAbsenceNotifications { get; set; }
    public DateTime? AbsenceNotificationStartDate { get; set; }
    public string DisplayName => FirstName.Trim() + " " + LastName.Trim();
    public string EmailAddress => PortalUsername + "@education.nsw.gov.au";
    public byte[] Photo { get; set; } = Array.Empty<byte>();

    public StudentFamily Family { get; set; } = new();
    public List<Enrolment> Enrolments { get; set; } = new();
    public List<DeviceAllocation> Devices { get; set; } = new();
    public List<StudentAdobeConnectOperation> AdobeConnectOperations { get; set; } = new();
    public List<StudentMSTeamOperation> MSTeamOperations { get; set; } = new();
    public List<Absence> Absences { get; set; } = new();
    public List<StudentPartialAbsence> PartialAbsences { get; set; } = new();
    public List<StudentWholeAbsence> WholeAbsences { get; set; } = new();
    public List<LessonRoll.LessonRollStudentAttendance> LessonsAttended { get; set; } = new();
    public List<StudentReport> Reports { get; set; } = new();
    public List<StudentAward> Awards { get; set; } = new();

    public Task EnableAbsenceNotifications(DateTime startDate)
    {
        IncludeInAbsenceNotifications = true;
        AbsenceNotificationStartDate = startDate;
        return Task.CompletedTask;
    }
}