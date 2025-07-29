namespace Constellation.Core.Models.Absences.Enums;

using Common;
using System;
using System.ComponentModel;
using System.Globalization;

[TypeConverter(typeof(AbsenceReasonConverter))]
public class AbsenceReason : StringEnumeration<AbsenceReason>, IEquatable<AbsenceReason>
{
    public static readonly AbsenceReason Absent = new("Absent");
    public static readonly AbsenceReason Exempt = new("Exempt");
    public static readonly AbsenceReason Flexible = new("Flexible");
    public static readonly AbsenceReason Leave = new("Leave");
    public static readonly AbsenceReason SchoolBusiness = new("School Business");
    public static readonly AbsenceReason Sick = new("Sick");
    public static readonly AbsenceReason SharedEnrolment = new("Shared Enrolment");
    public static readonly AbsenceReason Suspended = new("Suspended");
    public static readonly AbsenceReason Unjustified = new("Unjustified");

    public AbsenceReason(string value)
        : base(value, value) { }

    public int CompareTo(AbsenceReason obj)
    {
        if (obj is not null)
            return string.Compare(Value, obj.Value, StringComparison.Ordinal);

        return -1;
    }

    public override string ToString() => Value;

    public static implicit operator string(AbsenceReason reason) =>
        reason is null ? string.Empty : reason.Value;

    public bool Equals(AbsenceReason other)
    {
        if (other is null)
            return false;

        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj?.GetType() != GetType()) return false;
        return Equals((AbsenceReason)obj);
    }

    public override int GetHashCode() => Value.GetHashCode(StringComparison.InvariantCultureIgnoreCase);

    public static bool operator ==(AbsenceReason obj1, AbsenceReason obj2)
    {
        if (ReferenceEquals(obj1, obj2))
            return true;

        if (obj1 is null)
            return false;

        if (obj2 is null)
            return false;

        return obj1.Value.Equals(obj2.Value, StringComparison.Ordinal);
    }

    public static bool operator !=(AbsenceReason obj1, AbsenceReason obj2) => !(obj1 == obj2);
}

public class AbsenceReasonConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value is string stringValue)
            return AbsenceReason.FromValue(stringValue);

        return base.ConvertFrom(context, culture, value);
    }
}