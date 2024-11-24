using Constellation.Core.Primitives;
using System;

namespace Constellation.Core.Models.Reports.Identifiers;

public readonly record struct ExternalReportId(Guid Value)
    : IStronglyTypedId
{
    public static ExternalReportId FromValue(Guid value) =>
        new(value);

    public ExternalReportId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}