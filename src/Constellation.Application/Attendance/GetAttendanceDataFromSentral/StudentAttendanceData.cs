namespace Constellation.Application.Attendance.GetAttendanceDataFromSentral;

using Core.Enums;
using System;

public class StudentAttendanceData
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string StudentId { get; set; }
    public string Name { get; set; }
    public Grade Grade { get; set; }
    public string SchoolName { get; set; }
    public decimal MinuteYTD { get; set; }
    public decimal MinuteFN { get; set; }
    public decimal DayYTD { get; set; }
    public decimal DayFN { get; set; }
}