namespace Constellation.Core.Models.Absences;

using Common;
using System;

public class AbsenceType : StringEnumeration<AbsenceType>, IEquatable<AbsenceType>
{
    public static readonly AbsenceType Whole = new("Whole", "Whole Absence");
    public static readonly AbsenceType Partial = new("Partial", "Partial Absence");

    public AbsenceType(string value, string name) 
        : base(value, name) { }

    public override string ToString() => Name;

    public static implicit operator string(AbsenceType? value) =>
        value is null ? string.Empty : value.Name;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj?.GetType() != GetType()) return false;
        
        AbsenceType? other = obj as AbsenceType;

        return Value == other?.Value && Name == other.Name;
    }

    public bool Equals(AbsenceType? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.GetType() != GetType()) return false;

        return Value == other.Value && Name == other.Name;
    }

    public override int GetHashCode() => Value.GetHashCode(StringComparison.InvariantCultureIgnoreCase) ^ Name.GetHashCode(StringComparison.InvariantCultureIgnoreCase);

    public static bool operator ==(AbsenceType? obj1, AbsenceType? obj2)
    {
        if (ReferenceEquals(obj1, obj2))
            return true;

        if (obj1 is null)
            return false;

        if (obj2 is null)
            return false;

        return obj1.Value.Equals(obj2.Value, StringComparison.OrdinalIgnoreCase);
    }

    public static bool operator !=(AbsenceType obj1, AbsenceType obj2) => !(obj1 == obj2);
}
