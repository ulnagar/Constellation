namespace Constellation.Application.Attendance.GetAttendanceDataFromSentral;

using System.IO;

public class SystemAttendanceData
{
    public HtmlDocument YearToDateDayCalculationDocument { get; set; }
    public HtmlDocument FortnightDayCalculationDocument { get; set; }
    public Stream YearToDateMinuteCalculationDocument { get; set; }
    public Stream FortnightMinuteCalculationDocument { get; set; }
}