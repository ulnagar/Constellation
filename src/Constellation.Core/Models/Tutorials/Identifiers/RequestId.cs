namespace Constellation.Core.Models.Tutorials.Identifiers;

using Primitives;
using System;

public readonly record struct RequestId(Guid Value)
    : IStronglyTypedId<RequestId, Guid>
{
    public static RequestId Empty => new(Guid.Empty);

    public static RequestId FromValue(Guid value) =>
        new(value);

    public RequestId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}