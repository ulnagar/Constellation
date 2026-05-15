namespace Constellation.Core.Models.WorkFlow.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct ActionNoteId(Guid Value)
    : IStronglyTypedId<ActionNoteId, Guid>
{
    public static ActionNoteId Empty => new(Guid.Empty);

    public static ActionNoteId FromValue(Guid value) =>
        new(value);

    public ActionNoteId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}