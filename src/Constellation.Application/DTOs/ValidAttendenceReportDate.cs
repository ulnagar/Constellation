namespace Constellation.Application.DTOs;

using System;

public class ValidAttendenceReportDate
{
    public string TermGroup { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Description { get; set; }
}
