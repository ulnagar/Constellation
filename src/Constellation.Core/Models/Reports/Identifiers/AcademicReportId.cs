namespace Constellation.Core.Models.Reports.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct AcademicReportId(Guid Value)
    : IStronglyTypedId
{
    public static AcademicReportId FromValue(Guid value) =>
        new(value);

    public AcademicReportId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}