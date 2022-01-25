using System;

namespace Constellation.Application.Extensions
{
    public static class TimespanExtensions
    {
        public static TimeSpan GetTimeFromSentral(this string timeString)
        {
            var tt = timeString.Substring(timeString.Length - 2);
            var time = timeString.Substring(0, timeString.Length - 2);

            try
            {
                var value = TimeSpan.Parse(time);

                if (tt == "pm" && value.Hours < 12)
                    value = value.Add(new TimeSpan(12, 0, 0));

                return value;
            }
            catch (Exception)
            {
                throw new DataMisalignedException($"{timeString} could not be parsed to a TimeSpan value");
            }
        }

        public static string As12HourTime(this TimeSpan time)
        {
            var dt = DateTime.Today.AddTicks(time.Ticks);

            return dt.ToString("hh:mm tt");
        }
    }
}
