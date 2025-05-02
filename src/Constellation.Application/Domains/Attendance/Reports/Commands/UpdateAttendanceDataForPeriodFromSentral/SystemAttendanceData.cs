namespace Constellation.Application.Domains.Attendance.Reports.Commands.UpdateAttendanceDataForPeriodFromSentral;

using HtmlAgilityPack;
using System.IO;

public class SystemAttendanceData
{
    public HtmlDocument YearToDateDayCalculationDocument { get; set; }
    public HtmlDocument WeekDayCalculationDocument { get; set; }
    public Stream YearToDateMinuteCalculationDocument { get; set; }
    public Stream WeekMinuteCalculationDocument { get; set; }
}