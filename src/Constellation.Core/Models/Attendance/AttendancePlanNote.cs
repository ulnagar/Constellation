namespace Constellation.Core.Models.Attendance;

using Constellation.Core.Models.Assets.Errors;
using Constellation.Core.Primitives;
using Constellation.Core.Shared;
using Identifiers;
using System;

public sealed class AttendancePlanNote : IAuditableEntity
{
    // Required by EF Core
    private AttendancePlanNote() { }

    private AttendancePlanNote(
        AttendancePlanId planId,
        string message)
    {
        PlanId = planId;
        Message = message;
    }

    public AttendancePlanNoteId Id { get; private set; } = new();
    public AttendancePlanId PlanId { get; private set; }
    public string Message { get; private set; } = string.Empty;

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }

    public static Result<AttendancePlanNote> Create(
        AttendancePlanId planId,
        string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return Result.Failure<AttendancePlanNote>(NoteErrors.MessageEmpty);

        AttendancePlanNote note = new(
            planId,
            message);

        return note;
    }
}
