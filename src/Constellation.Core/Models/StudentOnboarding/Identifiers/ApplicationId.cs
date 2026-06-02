namespace Constellation.Core.Models.StudentOnboarding.Identifiers;

using Primitives;

public readonly record struct ApplicationId(Guid Value)
    : IStronglyTypedId<ApplicationId, Guid>
{
    public static ApplicationId Empty => new(Guid.Empty);

    public static ApplicationId FromValue(Guid value) =>
        new(value);

    public ApplicationId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}