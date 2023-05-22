namespace Constellation.Application.Interfaces.Configuration;

using Constellation.Core.Common;
using System;
using System.ComponentModel;
using System.Globalization;

[TypeConverter(typeof(AbsenceReasonConverter))]
public class AbsenceReason : StringEnumeration<AbsenceReason>
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