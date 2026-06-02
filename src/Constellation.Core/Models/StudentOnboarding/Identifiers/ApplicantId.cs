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
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}