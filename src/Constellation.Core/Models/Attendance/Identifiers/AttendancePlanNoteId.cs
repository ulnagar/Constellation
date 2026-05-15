namespace Constellation.Core.Models.Attendance.Identifiers;

using Primitives;
using System;

public readonly record struct AttendancePlanNoteId(Guid Value)
    : IStronglyTypedId<AttendancePlanNoteId, Guid>
{
    public static AttendancePlanNoteId Empty => new(Guid.Empty);

    public static AttendancePlanNoteId FromValue(Guid value) =>
        new(value);

    public AttendancePlanNoteId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() => 
        Value.ToString();
}