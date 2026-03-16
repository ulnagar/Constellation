namespace Constellation.Core.Models.WorkFlow.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct CaseDetailId(Guid Value)
    : IStronglyTypedId
{
    public static CaseDetailId Empty => new(Guid.Empty);

    public static CaseDetailId FromValue(Guid value) =>
        new(value);

    public CaseDetailId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}