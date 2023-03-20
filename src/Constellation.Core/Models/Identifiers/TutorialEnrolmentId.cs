namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record TutorialEnrolmentId(Guid Value)
{
    public TutorialEnrolmentId()
        : this(Guid.NewGuid()) { }
}
