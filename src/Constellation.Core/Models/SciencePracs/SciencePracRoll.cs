﻿namespace Constellation.Core.Models.SciencePracs;

using Constellation.Core.Models.Students.Identifiers;
using Enums;
using Errors;
using Identifiers;
using Shared;
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
    public string SubmittedBy { get; private set; }

    public IReadOnlyCollection<SciencePracAttendance> Attendance => _attendance;
    public DateOnly? LessonDate { get; private set; }
    public DateTime? SubmittedDate { get; private set; }
    public string Comment { get; private set; }
    public LessonStatus Status { get; private set; }
    public int NotificationCount { get; private set; }

    internal Result MarkRoll(
        string submittedBy,
        DateOnly lessonDate,
        string comment,
        List<StudentId> presentStudents,
        List<StudentId> absentStudents)
    {
        if (presentStudents.Count == 0 && string.IsNullOrWhiteSpace(comment))
            return Result.Failure(SciencePracRollErrors.CommentRequiredNonePresent);

        foreach (StudentId studentId in presentStudents)
        {
            SciencePracAttendance attendance = _attendance.SingleOrDefault(entry => entry.StudentId == studentId);

            if (attendance is null)
                continue;

            attendance.UpdateAttendance(true);
        }

        foreach (StudentId studentId in absentStudents)
        {
            SciencePracAttendance attendance = _attendance.SingleOrDefault(entry => entry.StudentId == studentId);

            if (attendance is null)
                continue;

            attendance.UpdateAttendance(false);
        }

        if (_attendance.Count(entry => entry.Present) == 0 && string.IsNullOrWhiteSpace(comment))
            return Result.Failure(SciencePracRollErrors.CommentRequiredNonePresent);

        SubmittedBy = submittedBy;
        LessonDate = lessonDate;
        Comment = comment;
        SubmittedDate = DateTime.Now;
        Status = LessonStatus.Completed;

        return Result.Success();
    }

    public Result CancelRoll(string comment)
    {
        if (Status == LessonStatus.Completed)
            return Result.Failure(SciencePracRollErrors.CannotCancelCompletedRoll);

        Status = LessonStatus.Cancelled;
        SubmittedDate = DateTime.Now;
        Comment = comment;

        return Result.Success();
    }

    public Result ReinstateRoll()
    {
        if (Status != LessonStatus.Cancelled)
            return Result.Failure(SciencePracRollErrors.MustBeCancelled);

        Status = LessonStatus.Active;
        SubmittedDate = null;
        Comment = null;

        return Result.Success();
    }

    public void AddStudent(StudentId studentId)
    {
        SciencePracAttendance record = _attendance.FirstOrDefault(entry => entry.StudentId == studentId);
        
        if (record is not null)
            return;

        _attendance.Add(new(
            Id,
            studentId));
    }


    /// <summary>
    /// Do not use. Does not delete record and throws NRE for the Attendance (as RollId is null)
    /// </summary>
    /// <param name="studentId"></param>
    public SciencePracAttendance? RemoveStudent(StudentId studentId)
    {
        SciencePracAttendance record = _attendance.FirstOrDefault(entry => entry.StudentId == studentId);

        if (record is null)
            return null;

        _attendance.Remove(record);

        return record;
    }

    public void IncrementNotificationCount() => NotificationCount++;
}
