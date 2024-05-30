using System;

namespace Constellation.Core.Models.Assets.Identifiers;

public readonly record struct NoteId(Guid Value)
{
    public static readonly NoteId Empty = new(Guid.Empty);

    public static NoteId FromValue(Guid value) =>
        new(value);

    public NoteId()
        : this(Guid.NewGuid()) { }

    public override string ToString() => Value.ToString();
}