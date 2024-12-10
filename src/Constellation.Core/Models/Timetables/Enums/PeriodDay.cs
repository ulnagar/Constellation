namespace Constellation.Core.Models.Timetables.Enums;

using Common;
using System.Collections.Generic;
using System.Linq;

public class PeriodDay : IntEnumeration<PeriodDay>
{
    public static readonly PeriodDay Monday = new(1, "Monday");
    public static readonly PeriodDay Tuesday = new(2, "Tuesday");
    public static readonly PeriodDay Wednesday = new(3, "Wednesday");
    public static readonly PeriodDay Thursday = new(4, "Thursday");
    public static readonly PeriodDay Friday = new(5, "Friday");
    
    private PeriodDay(int value, string name)
        : base(value, name)
    { }

    public static IEnumerable<PeriodDay> GetOptions 
        => Enumerations.Select(entry => entry.Value);
}