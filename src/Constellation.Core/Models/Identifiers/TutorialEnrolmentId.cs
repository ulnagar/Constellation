namespace Constellation.Core.Models.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct TutorialEnrolmentId(Guid Value)
    : IStronglyTypedId
{
    public static TutorialEnrolmentId Empty => new(Guid.Empty);

    public static TutorialEnrolmentId FromValue(Guid value) =>
        new(value);

    public TutorialEnrolmentId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
