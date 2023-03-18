using System;
using System.Collections.Generic;
using System.Reflection;

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
        public int Duration => GetDuration();

        private int GetDuration()
        {
            return (int)EndTime.Subtract(StartTime).TotalMinutes;
        }

        public override string ToString()
        {
            var GridName = Timetable[..3].ToUpper();

            var WeekNo = (Day - 1) / 5;
            var WeekName = WeekNo switch
            {
                0 => "Week A",
                1 => "Week B",
                2 => "Week C",
                4 => "Week D",
                _ => "",
            };

            var DayNo = Day % 5;
            var DayName = DayNo switch
            {
                1 => "Monday",
                2 => "Tuesday",
                3 => "Wednesday",
                4 => "Thursday",
                0 => "Friday",
                _ => "",
            };

            return $"{GridName} {WeekName} {DayName} - {Name}";
        }
    }
}