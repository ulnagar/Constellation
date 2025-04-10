#nullable enable
namespace Constellation.Application.ScheduledReports.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct ScheduledReportId(Guid Value)
    : IStronglyTypedId
{
    public static ScheduledReportId Empty => new(Guid.Empty);

    public static ScheduledReportId FromValue(Guid value) =>
        new(value);

    public ScheduledReportId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}