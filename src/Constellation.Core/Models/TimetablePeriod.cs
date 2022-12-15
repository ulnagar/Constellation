namespace Constellation.Core.Models;

public class TimetablePeriod
{
    public int Id { get; set; }
    public string Timetable { get; set; } = string.Empty;
    public int Day { get; set; }
    public int Period { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime? DateDeleted { get; set; }
    public List<OfferingSession> OfferingSessions { get; set; } = new();
}