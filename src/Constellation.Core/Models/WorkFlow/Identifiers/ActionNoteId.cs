namespace Constellation.Core.Models.WorkFlow.Identifiers;

using System;

public sealed record ActionNoteId(Guid Value)
{
    public static ActionNoteId FromValue(Guid value) =>
        new(value);

    public ActionNoteId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}