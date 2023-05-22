namespace Constellation.Core.Models.Absences;

using Constellation.Core.Common;

public class AbsenceType : StringEnumeration<AbsenceType>
{
    public static AbsenceType Whole = new("Whole", "Whole Absence");
    public static AbsenceType Partial = new("Partial", "Partial Absence");

    public AbsenceType(string value, string name) 
        : base(value, name) { }
}
