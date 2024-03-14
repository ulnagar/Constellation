namespace Constellation.Core.Models.WorkFlow.Identifiers;

using System;

public sealed record CaseDetailId(Guid Value)
{
    public static CaseDetailId FromValue(Guid value) =>
        new(value);

    public CaseDetailId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}