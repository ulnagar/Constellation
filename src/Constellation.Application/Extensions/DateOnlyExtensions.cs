namespace Constellation.Application.Extensions;

using System;
using System.Collections.Generic;
using System.Globalization;

public static class DateOnlyExtensions
{
    public static int GetDayNumber(this DateOnly date)
    {
        int day = (date.DayOfWeek == 0) ? 7 : ((int)date.DayOfWeek);

        if (day >= 6) return 0;
        Calendar cal = DateTimeFormatInfo.CurrentInfo.Calendar;
        int weekNum = cal.GetWeekOfYear(date.ToDateTime(TimeOnly.MinValue), CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        DateOnly firstOfYear = new DateOnly(date.Year, 1, 1);
        int yearStartDay = (firstOfYear.DayOfWeek == 0) ? 7 : ((int)firstOfYear.DayOfWeek);

        switch (yearStartDay)
        {
            case 1:
            case 2:
            case 3:
            case 6:
            case 7:
                if ((weekNum % 2) != 0)
                    day += 5;
                break;
            case 4:
            case 5:
                if ((weekNum % 2) == 0)
                    day += 5;
                break;
        }

        return day;
    }

    public static DateOnly VerifyStartOfFortnight(this DateOnly startDate)
    {
        var dayNumber = startDate.GetDayNumber();
        if (dayNumber > 1)
        {
            if (dayNumber > 5)
                dayNumber += 1;
            else
                dayNumber -= 1;

            startDate = startDate.AddDays(-dayNumber);

            startDate = VerifyStartOfFortnight(startDate);
        }
        else if (dayNumber == 0)
        {
            startDate = startDate.AddDays(-2);
            startDate = VerifyStartOfFortnight(startDate);
        }

        return startDate;
    }

    public static List<DateOnly> Range(this DateOnly startDate, DateOnly endDate)
    {
        List<DateOnly> datesInRange = new();

        DateOnly currentDate = startDate;

        while (currentDate <= endDate)
        {
            datesInRange.Add(currentDate);

            currentDate = currentDate.AddDays(1);
        }

        return datesInRange;
    }
}
