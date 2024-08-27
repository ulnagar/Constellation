namespace Constellation.Core.Models.Grade.Identifiers;

using System;

public readonly record struct GradeId(Guid Value)
{
    public static GradeId Empty => new(Guid.Empty);

    public static GradeId FromValue(Guid value) =>
        new(value);

    public GradeId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}