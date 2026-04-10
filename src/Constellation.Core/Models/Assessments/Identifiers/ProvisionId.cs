namespace Constellation.Core.Models.Assessments.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct ProvisionId(Guid Value)
    : IStronglyTypedId<ProvisionId, Guid>
{
    public static ProvisionId Empty => new(Guid.Empty);
    public static ProvisionId FromValue(Guid value) =>
        new(value);

    public ProvisionId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}