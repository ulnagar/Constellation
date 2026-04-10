namespace Constellation.Core.Models.Assets.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct NoteId(Guid Value)
    : IStronglyTypedId<NoteId, Guid>
{
    public static readonly NoteId Empty = new(Guid.Empty);

    public static NoteId FromValue(Guid value) =>
        new(value);

    public NoteId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() => Value.ToString();
}