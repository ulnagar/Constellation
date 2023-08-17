namespace Constellation.Core.Models.Absences;

using Constellation.Core.Common;

public class AbsenceType : StringEnumeration<AbsenceType>
{
    public static readonly AbsenceType Whole = new("Whole", "Whole Absence");
    public static readonly AbsenceType Partial = new("Partial", "Partial Absence");

    public AbsenceType(string value, string name) 
        : base(value, name) { }

    public static implicit operator string(AbsenceType value) =>
        value is null ? string.Empty : value.Name.ToString();
}
