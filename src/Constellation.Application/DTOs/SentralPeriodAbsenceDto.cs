using System;

namespace Constellation.Application.DTOs
{
    public class SentralPeriodAbsenceDto
    {
        public static string Partial = "Partial";
        public static string Whole = "Whole";

        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public bool WholeDay { get; set; }
        public string Timeframe { get; set; }
        public string Period { get; set; }
        public string ClassName { get; set; }
        public string Type { get; set; }
        public string PartialType { get; set; }
        public int MinutesAbsent { get; set; }
        public string Reason { get; set; }

        public string ExternalExplanation { get; set; }
        public string ExternalExplanationSource { get; set; }

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Period) ||
                string.IsNullOrWhiteSpace(ClassName) ||
                string.IsNullOrWhiteSpace(Reason) ||
                string.IsNullOrWhiteSpace(Type))
            {
                return false;
            }

            if (Date == DateOnly.MinValue)
            {
                return false;
            }

            if (Type == Partial && MinutesAbsent == 0)
            {
                return false;
            }

            return true;
        }
    }
}
