namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.StudentAttendanceReport;

using Core.Models.Students.Identifiers;

public class AttendanceReportSelection
{
    public StudentId StudentId { get; set; } = StudentId.Empty;
    public DateTime ReportDate { get; set; } = DateTime.Today;

    public Dictionary<StudentId, string> StudentList { get; set; }
}
