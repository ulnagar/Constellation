namespace Constellation.Core.Models.WorkFlow.Identifiers;

using Constellation.Core.Primitives;
using System;

public sealed record ActionNoteId(Guid Value)
    : IStronglyTypedId
{
    public static ActionNoteId FromValue(Guid value) =>
        new(value);

    public ActionNoteId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}