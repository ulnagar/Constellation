using System;

namespace Constellation.Application.DTOs;

public class AttendanceReportRequest
{
    public string StudentId { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
