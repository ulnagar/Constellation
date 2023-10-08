namespace Constellation.Application.Attendance.GetAttendanceDataFromSentral;

public class StudentAttendanceData
{
    public string SRN { get; set; }
    public string Name { get; set; }
    public decimal MinuteYTD { get; set; }
    public decimal MinuteFN { get; set; }
    public decimal DayYTD { get; set; }
    public decimal DayFN { get; set; }
}