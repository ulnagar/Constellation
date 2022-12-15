namespace Constellation.Core.Models;

public class OfferingResource
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string UrlPath { get; set; } = string.Empty;
    public int OfferingId { get; set; }
    public bool ShowLink { get; set; }
    public virtual CourseOffering? Offering { get; set; }
}