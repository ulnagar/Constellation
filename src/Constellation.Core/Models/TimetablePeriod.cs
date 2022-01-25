using System;
using System.Collections.Generic;

namespace Constellation.Core.Models
{
    public class TimetablePeriod
    {
        public TimetablePeriod()
        {
            OfferingSessions = new List<OfferingSession>();
        }

        public int Id { get; set; }
        public string Timetable { get; set; }
        public int Day { get; set; }
        public int Period { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DateDeleted { get; set; }
        public ICollection<OfferingSession> OfferingSessions { get; set; }
    }
}