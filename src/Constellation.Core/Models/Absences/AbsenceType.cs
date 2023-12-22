namespace Constellation.Core.Models.Absences;

using Constellation.Core.Common;
using System;

public class AbsenceType : StringEnumeration<AbsenceType>, IEquatable<AbsenceType>
{
    public static readonly AbsenceType Whole = new("Whole", "Whole Absence");
    public static readonly AbsenceType Partial = new("Partial", "Partial Absence");

    public AbsenceType(string value, string name) 
        : base(value, name) { }

    public static implicit operator string(AbsenceType value) =>
        value is null ? string.Empty : value.Name.ToString();

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        
        AbsenceType other = obj as AbsenceType;

        return Value == other.Value && Name == other.Name;
    }

    public bool Equals(AbsenceType other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.GetType() != this.GetType()) return false;

        return Value == other.Value && Name == other.Name;
    }

    public override int GetHashCode() => Value.GetHashCode() ^ Name.GetHashCode();

    public static bool operator ==(AbsenceType obj1, AbsenceType obj2)
    {
        if (ReferenceEquals(obj1, obj2))
            return true;

        if (ReferenceEquals(obj1, null))
            return false;

        if (ReferenceEquals(obj2, null))
            return false;

        return obj1.Value.Equals(obj2.Value);
    }

    public static bool operator !=(AbsenceType obj1, AbsenceType obj2) => !(obj1 == obj2);
}
