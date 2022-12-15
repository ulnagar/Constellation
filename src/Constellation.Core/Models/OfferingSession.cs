namespace Constellation.Core.Models;

public class OfferingSession
{
    public int Id { get; set; }
    public int OfferingId { get; set; }
    public virtual CourseOffering? Offering { get; set; }
    public string StaffId { get; set; } = string.Empty;
    public virtual Staff? Teacher { get; set; }
    public int PeriodId { get; set; }
    public virtual TimetablePeriod? Period { get; set; }
    public string RoomId { get; set; } = string.Empty;
    public virtual AdobeConnectRoom? Room { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? DateDeleted { get; set; }
}