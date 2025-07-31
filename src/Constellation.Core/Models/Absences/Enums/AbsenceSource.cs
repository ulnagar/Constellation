namespace Constellation.Core.Models.Absences.Enums;

using Common;
using System;

public sealed class AbsenceSource : StringEnumeration<AbsenceSource>, IEquatable<AbsenceSource>
{
    public static readonly AbsenceSource Offering = new("Offering", "Class");
    public static readonly AbsenceSource Tutorial = new("Tutorial", "Tutorial");

    public AbsenceSource(string value, string name) 
        : base(value, name) { }

    public override string ToString() => Name;

    public static implicit operator string(AbsenceSource value) =>
        value is null ? string.Empty : value.Name;

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj?.GetType() != GetType()) return false;
        
        AbsenceType other = obj as AbsenceType;

        return Value == other?.Value && Name == other.Name;
    }

    public bool Equals(AbsenceSource other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.GetType() != GetType()) return false;

        return Value == other.Value && Name == other.Name;
    }

    public override int GetHashCode() => Value.GetHashCode(StringComparison.InvariantCultureIgnoreCase) ^ Name.GetHashCode(StringComparison.InvariantCultureIgnoreCase);

    public static bool operator ==(AbsenceSource obj1, AbsenceSource obj2)
    {
        if (ReferenceEquals(obj1, obj2))
            return true;

        if (obj1 is null)
            return false;

        if (obj2 is null)
            return false;

        return obj1.Value.Equals(obj2.Value, StringComparison.OrdinalIgnoreCase);
    }

    public static bool operator !=(AbsenceSource obj1, AbsenceSource obj2) => !(obj1 == obj2);
}
