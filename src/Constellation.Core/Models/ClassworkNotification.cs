namespace Constellation.Core.Models;

public class ClassworkNotification
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public virtual Staff? CompletedBy { get; set; }
    public string StaffId { get; set; } = string.Empty; 
    public DateTime? CompletedAt { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public List<Absence> Absences { get; set; } = new();
    public List<Staff> Teachers { get; set; } = new();
    public virtual CourseOffering? Offering { get; set; }
    public int OfferingId { get; set; }
    public DateTime AbsenceDate { get; set; }
    public List<ClassCover> Covers { get; set; } = new();
}
