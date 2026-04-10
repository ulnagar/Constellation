namespace Constellation.Core.Models.Tutorials.Identifiers;

using Primitives;
using System;

public readonly record struct RequestNoteId(Guid Value)
    : IStronglyTypedId<RequestNoteId, Guid>
{
    public static readonly RequestNoteId Empty = new(Guid.Empty);

    public static RequestNoteId FromValue(Guid value) =>
        new(value);

    public RequestNoteId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}