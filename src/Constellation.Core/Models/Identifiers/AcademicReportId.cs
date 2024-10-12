namespace Constellation.Core.Models.Identifiers;

using Constellation.Core.Primitives;
using System;

public sealed record AcademicReportId(Guid Value)
    : IStronglyTypedId
{
    public static AcademicReportId FromValue(Guid Value) =>
        new(Value);

    public AcademicReportId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
