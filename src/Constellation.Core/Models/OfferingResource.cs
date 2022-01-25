namespace Constellation.Core.Models
{
    public class OfferingResource
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string UrlPath { get; set; }
        public int OfferingId { get; set; }
        public bool ShowLink { get; set; }
        public CourseOffering Offering { get; set; }
    }
}