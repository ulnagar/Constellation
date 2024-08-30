namespace Constellation.Application.Attendance.GetAttendanceDataFromSentral;

using Core.Enums;
using Core.Models.Students.ValueObjects;
using System;

public class StudentAttendanceData
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public StudentReferenceNumber StudentReferenceNumber { get; set; }
    public string Name { get; set; }
    public Grade Grade { get; set; }
    public string SchoolName { get; set; }
    public decimal MinuteYTD { get; set; }
    public decimal MinuteWeek { get; set; }
    public decimal DayYTD { get; set; }
    public decimal DayWeek { get; set; }
}