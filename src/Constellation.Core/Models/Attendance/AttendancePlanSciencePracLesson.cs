#nullable enable
namespace Constellation.Core.Models.Attendance;

using Timetables.Enums;

public sealed class AttendancePlanSciencePracLesson
{
    private AttendancePlanSciencePracLesson() { }
    
    internal AttendancePlanSciencePracLesson(
        PeriodWeek week,
        PeriodDay day,
        string period)
    {
        Week = week;
        Day = day;
        Period = period;
    }

    public PeriodWeek Week { get; private set; }
    public PeriodDay Day { get; private set; }
    public string Period { get; private set; }
}