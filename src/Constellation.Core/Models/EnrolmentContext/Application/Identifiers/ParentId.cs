namespace Constellation.Core.Models.EnrolmentContext.Application.Identifiers;

using Primitives;

public readonly record struct ParentId(Guid Value)
    : IStronglyTypedId<ParentId, Guid>
{
    public static ParentId Empty => new(Guid.Empty);

    public static ParentId FromValue(Guid value) =>
        new(value);

    public ParentId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}