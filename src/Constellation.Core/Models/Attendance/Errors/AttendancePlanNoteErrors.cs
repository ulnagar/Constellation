namespace Constellation.Core.Models.Attendance.Errors;

using Shared;

public static class AttendancePlanNoteErrors
{
    public static readonly Error MessageEmpty = new(
        "AttendancePlans.Notes.MessageEmpty",
        "Message must not be empty");
}