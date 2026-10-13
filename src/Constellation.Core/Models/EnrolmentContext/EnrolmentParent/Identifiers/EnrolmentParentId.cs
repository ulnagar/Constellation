namespace Constellation.Core.Models.EnrolmentContext.EnrolmentParent.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct EnrolmentParentId(Guid Value)
    : IStronglyTypedId<EnrolmentParentId, Guid>
{
    public static EnrolmentParentId Empty => new(Guid.Empty);

    public static EnrolmentParentId FromValue(Guid value) =>
        new(value);

    public EnrolmentParentId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();

}
