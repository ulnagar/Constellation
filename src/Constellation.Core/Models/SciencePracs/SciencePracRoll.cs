namespace Constellation.Core.Models.SciencePracs;

using Constellation.Core.Enums;
using Constellation.Core.Errors;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class SciencePracRoll
{
    private readonly List<SciencePracAttendance> _attendance = new();

    public SciencePracRoll(
        SciencePracLessonId lessonId,
        string schoolCode)
    {
        Id = new();

        LessonId = lessonId;
        SchoolCode = schoolCode;

        Status = LessonStatus.Active;
    }

    public SciencePracRollId Id { get; private set; }
    public SciencePracLessonId LessonId { get; private set; }
    public string SchoolCode { get; private set; }
    public int? SchoolContactId { get; private set; }
    public IReadOnlyCollection<SciencePracAttendance> Attendance => _attendance;
    public DateOnly? LessonDate { get; private set; }
    public DateTime? SubmittedDate { get; private set; }
    public string Comment { get; private set; }
    public LessonStatus Status { get; private set; }
    public int NotificationCount { get; private set; }

    public Result MarkRoll(
        int schoolContactId,
        DateOnly lessonDate,
        string comment,
        List<string> presentStudents,
        List<string> absentStudents)
    {
        if (presentStudents.Count == 0 && string.IsNullOrWhiteSpace(comment))
            return Result.Failure(DomainErrors.SciencePracs.Roll.CommentRequiredNonePresent);

        foreach (string studentId in presentStudents)
        {
            SciencePracAttendance attendance = _attendance.SingleOrDefault(entry => entry.StudentId == studentId);

            if (attendance is null)
                continue;

            attendance.UpdateAttendance(true);
        }

        foreach (string studentId in absentStudents)
        {
            SciencePracAttendance attendance = _attendance.SingleOrDefault(entry => entry.StudentId == studentId);

            if (attendance is null)
                continue;

            attendance.UpdateAttendance(false);
        }

        if (_attendance.Count(entry => entry.Present) == 0 && string.IsNullOrWhiteSpace(comment))
            return Result.Failure(DomainErrors.SciencePracs.Roll.CommentRequiredNonePresent);

        SchoolContactId = schoolContactId;
        LessonDate = lessonDate;
        Comment = comment;
        SubmittedDate = DateTime.Now;
        Status = LessonStatus.Completed;

        return Result.Success();
    }

    public Result CancelRoll(string comment)
    {
        if (Status == LessonStatus.Completed)
            return Result.Failure(DomainErrors.SciencePracs.Roll.CannotCancelCompletedRoll);

        Status = LessonStatus.Cancelled;
        SubmittedDate = DateTime.Now;
        Comment = comment;

        return Result.Success();
    }

    public void IncrementNotificationCount() => NotificationCount++;
}
