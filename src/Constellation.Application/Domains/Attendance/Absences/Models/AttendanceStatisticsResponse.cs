namespace Constellation.Application.Domains.Attendance.Absences.Models;

using Core.Enums;
using System.Collections.Generic;

public sealed record AttendanceStatisticsResponse
{
    public Dictionary<string, decimal> WholeSchoolAttendancePercentage { get; set; } = [];
    public Dictionary<Grade, decimal> CurrentWholeSchoolAttendanceByGrade { get; set; } = [];
    public Dictionary<Grade, int> UnexplainedPartialAbsenceCountByGrade { get; set; } = [];
    public Dictionary<Grade, int> TotalPartialAbsenceCountByGrade { get; set; } = [];
    public Dictionary<Grade, decimal> PartialAbsencePercentageByGrade { get; set; } = [];
    public Dictionary<Grade, int> UnexplainedWholeAbsenceCountByGrade { get; set; } = [];
    public Dictionary<Grade, int> TotalWholeAbsenceCountByGrade { get; set; } = [];
    public Dictionary<Grade, decimal> WholeAbsencePercentageByGrade { get; set; } = [];
}
