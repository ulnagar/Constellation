﻿namespace Constellation.Presentation.Server.Pages.Shared.Components.StudentAttendanceReport;

public class AttendanceReportSelection
{
    public string StudentId { get; set; }
    public DateTime ReportDate { get; set; } = DateTime.Today;

    public Dictionary<string, string> StudentList { get; set; }
}
