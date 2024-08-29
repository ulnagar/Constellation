namespace Constellation.Application.Attendance.GetAttendanceDataFromSentral;

using Core.Enums;
using Core.Models.Students.Identifiers;
using System;

public class StudentAttendanceData
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public StudentId StudentId { get; set; }
    public string Name { get; set; }
    public Grade Grade { get; set; }
    public string SchoolName { get; set; }
    public decimal MinuteYTD { get; set; }
    public decimal MinuteWeek { get; set; }
    public decimal DayYTD { get; set; }
    public decimal DayWeek { get; set; }
}