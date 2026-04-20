namespace Constellation.Application.Domains.Attendance.Reports.Commands.UpdateAttendanceDataForPeriodFromSentral;

using HtmlAgilityPack;
using System.IO;

public class SystemAttendanceData
{
    public required HtmlDocument YearToDateDayCalculationDocument { get; set; }
    public required HtmlDocument WeekDayCalculationDocument { get; set; }
    public required Stream YearToDateMinuteCalculationDocument { get; set; }
    public required Stream WeekMinuteCalculationDocument { get; set; }
}