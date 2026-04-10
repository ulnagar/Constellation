namespace Constellation.Core.Models.StaffMembers.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct StaffId(Guid Value)
    : IStronglyTypedId<StaffId, Guid>
{
    public static StaffId Empty => new(Guid.Empty);

    public static StaffId FromValue(Guid value) =>
        new(value);

    public StaffId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}