namespace Constellation.Core.Models.StudentOnboarding.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct ApplicantId(Guid Value)
    : IStronglyTypedId<ApplicantId, Guid>
{
    public static ApplicantId Empty => new(Guid.Empty);

    public static ApplicantId FromValue(Guid value) =>
        new(value);

    public ApplicantId()
        : this(new Guid()) { }

    public override string ToString() =>
        Value.ToString();
}

public readonly record struct ParentId(Guid Value)
    : IStronglyTypedId<ParentId, Guid>
{
    public static ParentId Empty => new(Guid.Empty);

    public static ParentId FromValue(Guid value) =>
        new(value);

    public ParentId()
        : this(new Guid()) { }

    public override string ToString() =>
        Value.ToString();
}
