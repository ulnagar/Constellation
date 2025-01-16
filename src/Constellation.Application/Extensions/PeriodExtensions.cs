namespace Constellation.Application.Extensions;

using Core.Models.Timetables;
using System.Collections.Generic;
using System.Linq;

public static class PeriodExtensions
{
    public static IEnumerable<IEnumerable<Period>> GroupConsecutive(this IEnumerable<Period> src)
    {
        var more = false;

        src = src.OrderBy(period => period.Day).ThenBy(period => period.StartTime);

        IEnumerable<Period> ConsecutiveSequence(IEnumerator<Period> csi)
        {
            Period prevCurrent;
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
}