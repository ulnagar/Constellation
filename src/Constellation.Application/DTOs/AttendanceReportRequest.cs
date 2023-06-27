using System;

namespace Constellation.Application.DTOs;

public class AttendanceReportRequest
{
    public string StudentId { get; set; } = "";
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}
