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

            src.OrderBy(period => period.Day).ThenBy(period => period.StartTime).ToList();

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
            var weekName = "";
            switch (weekNo)
            {
                case 0:
                    weekName = "Week A";
                    break;
                case 1:
                    weekName = "Week B";
                    break;
                case 2:
                    weekName = "Week C";
                    break;
                case 4:
                    weekName = "Week D";
                    break;
                default:
                    weekName = "";
                    break;
            }

            var dayNo = period.Day % 5;
            var dayName = "";
            switch (dayNo)
            {
                case 1:
                    dayName = "Monday";
                    break;
                case 2:
                    dayName = "Tuesday";
                    break;
                case 3:
                    dayName = "Wednesday";
                    break;
                case 4:
                    dayName = "Thursday";
                    break;
                case 0:
                    dayName = "Friday";
                    break;
                default:
                    dayName = "";
                    break;
            }

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
