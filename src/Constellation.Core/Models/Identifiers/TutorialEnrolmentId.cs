namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record TutorialEnrolmentId(Guid Value)
{
    public static TutorialEnrolmentId FromValue(Guid value) =>
        new(value);

    public TutorialEnrolmentId()
        : this(Guid.NewGuid()) { }
}
