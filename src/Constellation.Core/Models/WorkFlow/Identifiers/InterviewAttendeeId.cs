namespace Constellation.Core.Models.WorkFlow.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct InterviewAttendeeId(Guid Value)
    : IStronglyTypedId
{
    public static InterviewAttendeeId Empty = new(Guid.Empty);

    public static InterviewAttendeeId FromValue(Guid value) =>
        new(value);

    public InterviewAttendeeId() 
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}