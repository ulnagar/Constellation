using System;

namespace Constellation.Core.Models
{
    public class OfferingSession
    {
        public int Id { get; set; }
        public int OfferingId { get; set; }
        public CourseOffering Offering { get; set; }
        public string StaffId { get; set; }
        public Staff Teacher { get; set; }
        public int PeriodId { get; set; }
        public TimetablePeriod Period { get; set; }
        public string RoomId { get; set; }
        public AdobeConnectRoom Room { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateDeleted { get; set; }
    }
}