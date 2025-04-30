namespace Constellation.Application.Extensions;

using System;
using System.Globalization;

public static class DateTimeExtensions
{
    public static string AsRelativeTime(this DateTime date)
    {
        const int SECOND = 1;
        const int MINUTE = 60 * SECOND;
        const int HOUR = 60 * MINUTE;
        const int DAY = 24 * HOUR;
        const int MONTH = 30 * DAY;

        var ts = new TimeSpan(DateTime.Now.Ticks - date.Ticks);
        double delta = Math.Abs(ts.TotalSeconds);

        if (delta < 1 * MINUTE)
            return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";

        if (delta < 2 * MINUTE)
            return "a minute ago";

        if (delta < 45 * MINUTE)
            return ts.Minutes + " minutes ago";

        if (delta < 90 * MINUTE)
            return "an hour ago";

        if (delta < 24 * HOUR)
            return ts.Hours + " hours ago";

        if (delta < 48 * HOUR)
            return "yesterday";

        if (delta < 30 * DAY)
            return ts.Days + " days ago";

        if (delta < 12 * MONTH)
        {
            int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
            return months <= 1 ? "one month ago" : months + " months ago";
        }
        else
        {
            int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
            return years <= 1 ? "one year ago" : years + " years ago";
        }
    }

    public static int SecondsSinceYearStart(this DateTime dateTime)
    {
        var startDate = new DateTime(dateTime.Year, 1, 1);

        return (int)Math.Abs(startDate.Subtract(dateTime).TotalSeconds);
    }

    public static double GetUnixEpoch(this DateTime dateTime)
    {
        var unixTime = dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        return unixTime.TotalSeconds;
    }

    public static int GetDayNumber(this DateTime dateTime)
    {
        var day = (dateTime.DayOfWeek == 0) ? 7 : ((int)dateTime.DayOfWeek);

        if (day >= 6) return 0;
        var cal = DateTimeFormatInfo.CurrentInfo.Calendar;
        var weekNum = cal.GetWeekOfYear(dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        var firstOfYear = new DateTime(dateTime.Year, 1, 1);
        var yearStartDay = (firstOfYear.DayOfWeek == 0) ? 7 : ((int)firstOfYear.DayOfWeek);

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

    public static DateTime VerifyStartOfFortnight(this DateTime startDate)
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
}