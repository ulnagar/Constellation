namespace Constellation.Core.Models.Attendance.Identifiers;

using Primitives;
using System;

public readonly record struct AttendancePlanNoteId(Guid Value)
    : IStronglyTypedId
{
    public static readonly AttendancePlanNoteId Empty = new(Guid.Empty);

    public static AttendancePlanNoteId FromValue(Guid value) =>
        new(value);

    public AttendancePlanNoteId()
        : this(Guid.NewGuid()) { }

    public override string ToString() => 
        Value.ToString();
}