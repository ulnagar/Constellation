namespace Constellation.Core.Models.WorkFlow.Identifiers;

using System;

public sealed record CaseId(Guid Value)
{
    public static CaseId FromValue(Guid value) =>
        new(value);

    public CaseId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}