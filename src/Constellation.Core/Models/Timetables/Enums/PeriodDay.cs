namespace Constellation.Core.Models.Timetables.Enums;

using Common;
using System;
using System.Collections.Generic;
using System.Linq;

public class PeriodDay : IntEnumeration<PeriodDay>, IComparable<PeriodDay>
{
    public static readonly PeriodDay Unknown = new(0, "Unknown", string.Empty);
    public static readonly PeriodDay Monday = new(1, "Monday", "Mon");
    public static readonly PeriodDay Tuesday = new(2, "Tuesday", "Tue");
    public static readonly PeriodDay Wednesday = new(3, "Wednesday", "Wed");
    public static readonly PeriodDay Thursday = new(4, "Thursday", "Thu");
    public static readonly PeriodDay Friday = new(5, "Friday", "Fri");

    /// <summary>
    /// Do not use. For serialization purposes only.
    /// </summary>
    private PeriodDay() { }

    private PeriodDay(int value, string name, string abbreviation)
        : base(value, name)
    {
        Abbreviation = abbreviation;
    }

    public string Abbreviation { get; set; }

    public static PeriodDay FromDayNumber(int dayNumber) =>
        dayNumber switch
        {
            1 or 6 => Monday,
            2 or 7 => Tuesday,
            3 or 8 => Wednesday,
            4 or 9 => Thursday,
            5 or 10 => Friday,
            _ => Unknown
        };

    public static IEnumerable<PeriodDay> GetOptions 
        => Enumerations
            .Select(entry => entry.Value)
            .Where(entry => entry.Value > 0);

    public int CompareTo(PeriodDay other) =>
        Value.CompareTo(other.Value);
}