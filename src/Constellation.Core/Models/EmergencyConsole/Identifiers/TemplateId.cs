namespace Constellation.Core.Models.EmergencyConsole.Identifiers;

using Constellation.Core.Primitives;
using System;

public sealed record TemplateId(Guid Value)
    : IStronglyTypedId
{
    public static readonly TemplateId Empty = new(Guid.Empty);

    public static TemplateId FromValue(Guid value) =>
        new(value);

    public TemplateId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
