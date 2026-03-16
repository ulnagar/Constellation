namespace Constellation.Core.Models.EmergencyConsole.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct TemplateId(Guid Value)
    : IStronglyTypedId
{
    public static readonly TemplateId Empty = new(Guid.Empty);

    public static TemplateId FromValue(Guid value) =>
        new(value);

    public TemplateId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}