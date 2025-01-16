#nullable enable
namespace Constellation.Core.Models.Attendance;

public sealed class AttendancePlanMissedLesson
{
    private AttendancePlanMissedLesson() { }

    internal AttendancePlanMissedLesson(
        string subject,
        double totalMinutes,
        double minutesMissed)
    {
        Subject = subject;
        TotalMinutesPerCycle = totalMinutes;
        MinutesMissedPerCycle = minutesMissed;
    }

    public string Subject { get; private set; }
    public double TotalMinutesPerCycle { get; private set; }
    public double MinutesMissedPerCycle { get; private set; }
    public double PercentMissed => MinutesMissedPerCycle / TotalMinutesPerCycle;
}