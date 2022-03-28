using Constellation.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace Constellation.Application.Extensions
{
    public static class PeriodExtensions
    {
        public static IEnumerable<IEnumerable<TimetablePeriod>> GroupConsecutive(this IEnumerable<TimetablePeriod> src)
        {
            var more = false;

            src = src.OrderBy(period => period.Day).ThenBy(period => period.StartTime);

            IEnumerable<TimetablePeriod> ConsecutiveSequence(IEnumerator<TimetablePeriod> csi)
            {
                TimetablePeriod prevCurrent;
                do
                    yield return (prevCurrent = csi.Current);
                while ((more = csi.MoveNext()) && (csi.Current.StartTime == prevCurrent.EndTime && csi.Current.Day == prevCurrent.Day));
            }

            var si = src.GetEnumerator();
            if (si.MoveNext())
            {
                do
                    yield return ConsecutiveSequence(si).ToList();
                while (more);
            }
        }

        public static string GetPeriodGroup(this TimetablePeriod period)
        {
            var grid = period.Timetable.Substring(0, 3).ToUpper();
            var weekNo = (period.Day - 1) / 5;
            var weekName = weekNo switch
            {
                0 => "Week A",
                1 => "Week B",
                2 => "Week C",
                4 => "Week D",
                _ => "",
            };
            var dayNo = period.Day % 5;
            var dayName = dayNo switch
            {
                1 => "Monday",
                2 => "Tuesday",
                3 => "Wednesday",
                4 => "Thursday",
                0 => "Friday",
                _ => "",
            };
            return $"{grid} {weekName} {dayName}";
        }

        public static string GetPeriodDescriptor(this TimetablePeriod period)
        {
            if (period.Timetable == "PRIMARY")
            {
                if (period.Name.Contains("Period"))
                {
                    var periodNum = period.Name.Split(' ').Last();
                    return $"S{periodNum}";
                }

                return $"S{period.Name.Substring(0, 1)}";
            }

            if (period.Name.Contains("Period"))
                return period.Name.Split(' ').Last();

            return period.Name.Substring(0, 1);
        }
    }
}
