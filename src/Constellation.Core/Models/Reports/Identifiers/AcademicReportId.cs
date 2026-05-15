namespace Constellation.Core.Models.Reports.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct AcademicReportId(Guid Value)
    : IStronglyTypedId<AcademicReportId, Guid>
{
    public static AcademicReportId Empty => new(Guid.Empty);

    public static AcademicReportId FromValue(Guid value) =>
        new(value);

    public AcademicReportId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}