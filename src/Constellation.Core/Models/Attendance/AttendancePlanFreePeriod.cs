#nullable enable
namespace Constellation.Core.Models.Attendance;

using Timetables.Enums;

public sealed class AttendancePlanFreePeriod
{
    private AttendancePlanFreePeriod() { }

    internal AttendancePlanFreePeriod(
        PeriodWeek week,
        PeriodDay day,
        string period,
        double minutes,
        string activity)
    {
        Week = week;
        Day = day;
        Period = period;
        Minutes = minutes;
        Activity = activity;
    }

    public PeriodWeek Week { get; set; }
    public PeriodDay Day { get; set; }
    public string Period { get; set; }
    public double Minutes { get; set; }
    public string Activity { get; set; }
}