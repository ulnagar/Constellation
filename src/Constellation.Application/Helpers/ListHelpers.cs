namespace Constellation.Application.Helpers;

using Core.Models.Timetables.Enums;

public static class ListHelpers
{
    public static string FormatDayRanges(List<PeriodDay> days)
    {
        List<PeriodDay> sorted = days.OrderBy(d => d.Value).ToList();
        List<string> ranges = new List<string>();
        int i = 0;

        while (i < sorted.Count)
        {
            int start = i;
            while (i + 1 < sorted.Count && sorted[i + 1].Value == sorted[i].Value + 1)
            {
                i++;
            }

            ranges.Add(start == i
                ? sorted[start].Abbreviation
                : $"{sorted[start].Abbreviation} - {sorted[i].Abbreviation}");

            i++;
        }

        return string.Join(", ", ranges);
    }
}