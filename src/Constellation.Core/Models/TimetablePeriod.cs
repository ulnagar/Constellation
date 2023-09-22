using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Constellation.Core.Models
{
    public class TimetablePeriod
    {
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
        public List<Session> OfferingSessions { get; set; } = new();
        public int Duration => GetDuration();

        private int GetDuration() => (int)EndTime.Subtract(StartTime).TotalMinutes;

        public string GroupName()
        {
            string gridName = Timetable[..3].ToUpper();

            int weekNo = (Day - 1) / 5;
            string weekName = weekNo switch
            {
                0 => "Week A",
                1 => "Week B",
                2 => "Week C",
                4 => "Week D",
                _ => "",
            };

            int dayNo = Day % 5;
            string dayName = dayNo switch
            {
                1 => "Monday",
                2 => "Tuesday",
                3 => "Wednesday",
                4 => "Thursday",
                0 => "Friday",
                _ => "",
            };

            return $"{gridName} {weekName} {dayName}";
        }

        public override string ToString() =>
            $"{GroupName()} - {Name}";
    }
}