#nullable enable
namespace Constellation.Core.Models.Attendance;

using Abstractions.Clock;
using Abstractions.Services;
using Constellation.Core.Models.Timetables.Identifiers;
using Core.Enums;
using Enums;
using Errors;
using Events;
using Identifiers;
using Offerings;
using Primitives;
using Shared;
using Students;
using Students.Identifiers;
using Subjects;
using Subjects.Identifiers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Timetables;
using Timetables.Enums;
using ValueObjects;

public sealed class AttendancePlan : AggregateRoot, IFullyAuditableEntity
{
    private readonly List<AttendancePlanPeriod> _periods = new();
    private readonly List<AttendancePlanMissedLesson> _missedLessons = new();
    private readonly List<AttendancePlanFreePeriod> _freePeriods = new();
    private readonly List<AttendancePlanNote> _notes = new();

    private AttendancePlan() { } // Required for EF Core

    private AttendancePlan(
        Student student)
    {
        Id = new();
        Status = AttendancePlanStatus.Pending;

        StudentId = student.Id;
        Student = student.Name;
        Grade = student.CurrentEnrolment!.Grade;
        SchoolCode = student.CurrentEnrolment!.SchoolCode;
        School = student.CurrentEnrolment!.SchoolName;
    }

    public AttendancePlanId Id { get; private set; }
    public AttendancePlanStatus Status { get; private set; }
    public StudentId StudentId { get; private set; }
    public Name Student { get; private set; }
    public Grade Grade { get; private set; }
    public string SchoolCode { get; private set; }
    public string School { get; private set; }
    public IReadOnlyList<AttendancePlanPeriod> Periods => _periods.AsReadOnly();
    public IReadOnlyList<AttendancePlanFreePeriod> FreePeriods => _freePeriods.AsReadOnly();
    public IReadOnlyList<AttendancePlanMissedLesson> MissedLessons => _missedLessons.AsReadOnly();
    public IReadOnlyList<AttendancePlanNote> Notes => _notes.AsReadOnly();
    public AttendancePlanSciencePracLesson? SciencePracLesson { get; private set; }
    public IDictionary<string, double> Percentages => Status.Equals(AttendancePlanStatus.Pending) ? new() : CalculatePercentages();

    public string? SubmittedBy { get; private set; }
    public DateTime? SubmittedAt { get; private set; }

    public string? CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string? DeletedBy { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }

    public static AttendancePlan Create(
        Student student)
    {
        AttendancePlan plan = new(
            student);

        return plan;
    }

    public void AddPeriods(
        List<Period> periods,
        Offering offering,
        Course course)
    {
        foreach (Period period in periods)
        {
            AttendancePlanPeriod planPeriod = new(
                Id,
                period,
                offering,
                course);

            _periods.Add(planPeriod);
        }
    }

    public Result UpdatePeriods(
        List<(AttendancePlanPeriodId Id, TimeOnly EntryTime, TimeOnly ExitTime)> periods,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTime)
    {
        foreach ((AttendancePlanPeriodId Id, TimeOnly EntryTime, TimeOnly ExitTime) entry in periods)
        {
            AttendancePlanPeriod? period = _periods.FirstOrDefault(period => period.Id == entry.Id);

            if (period is null)
                return Result.Failure(AttendancePlanErrors.PeriodNotFound(entry.Id));

            period.UpdateDetails(entry.EntryTime, entry.ExitTime);
        }

        SubmittedBy = currentUserService.UserName;
        SubmittedAt = dateTime.Now;


        return Result.Success();
    }

    public Result CopyPeriodValues(
        AttendancePlanPeriodId id, 
        TimeOnly entryTime, 
        TimeOnly exitTime)
    {
        AttendancePlanPeriod? period = _periods.FirstOrDefault(period => period.Id == id);

        if (period is null)
            return Result.Failure(AttendancePlanErrors.PeriodNotFound(id));

        period.UpdateDetails(entryTime, exitTime);
     
        return Result.Success();
    }

    public Result UpdateSciencePracLesson(
        PeriodWeek week,
        PeriodDay day,
        string period)
    {
        AttendancePlanSciencePracLesson lesson = new(
            week,
            day,
            period);

        SciencePracLesson = lesson;

        return Result.Success();
    }

    public void AddMissedLessons(List<(string Subject, double TotalMinutes, double MissedMinutes)> missedLessons)
    {
        _missedLessons.Clear();

        foreach ((string Subject, double TotalMinutes, double MissedMinutes) missedLesson in missedLessons)
        {
            _missedLessons.Add(new(missedLesson.Subject, missedLesson.TotalMinutes, missedLesson.MissedMinutes));
        }
    }

    public void AddFreePeriods(List<(PeriodWeek Week, PeriodDay Day, string Period, double Minutes, string Activity)> freePeriods)
    {
        _freePeriods.Clear();

        foreach ((PeriodWeek Week, PeriodDay Day, string Period, double Minutes, string Activity) freePeriod in freePeriods)
        {
            _freePeriods.Add(new(freePeriod.Week, freePeriod.Day, freePeriod.Period, freePeriod.Minutes, freePeriod.Activity));
        }

    }
    
    private Result UpdateStatus(AttendancePlanStatus newStatus)
    {
        if (Status.Equals(AttendancePlanStatus.Accepted) || 
            Status.Equals(AttendancePlanStatus.Rejected) ||
            Status.Equals(AttendancePlanStatus.Superseded) ||
            Status.Equals(AttendancePlanStatus.Archived))
            return Result.Failure(AttendancePlanErrors.InvalidCurrentStatus(Status));

        if (newStatus.Equals(AttendancePlanStatus.Archived))
            return Result.Failure(AttendancePlanErrors.InvalidNewStatus(newStatus));

        Result<AttendancePlanNote> note = AttendancePlanNote.Create(Id, $"Updated Status from {Status} to {newStatus}");

        if (note.IsFailure)
            return Result.Failure(note.Error);

        _notes.Add(note.Value);

        Status = newStatus;

        return Result.Success();
    }

    public Result ArchivePlan()
    {
        Result<AttendancePlanNote> note = AttendancePlanNote.Create(Id, $"Plan archived");

        if (note.IsFailure)
            return Result.Failure(note.Error);

        _notes.Add(note.Value);

        Status = AttendancePlanStatus.Archived;

        return Result.Success();
    }

    public Result SubmitPlan(string comment)
    {
        Result statusUpdate = UpdateStatus(AttendancePlanStatus.Processing);

        if (statusUpdate.IsFailure)
            return statusUpdate;

        if (string.IsNullOrWhiteSpace(comment))
            return Result.Success();

        Result<AttendancePlanNote> note = AttendancePlanNote.Create(Id, comment);

        if (note.IsFailure)
            return Result.Failure(note.Error);

        _notes.Add(note.Value);

        return Result.Success();
    }

    public Result ApprovePlan(string comment)
    {
        Result statusUpdate = UpdateStatus(AttendancePlanStatus.Accepted);

        if (statusUpdate.IsFailure)
            return statusUpdate;

        if (string.IsNullOrWhiteSpace(comment))
            return Result.Failure(AttendancePlanErrors.CommentRequired);

        Result<AttendancePlanNote> note = AttendancePlanNote.Create(Id, comment);

        if (note.IsFailure)
            return Result.Failure(note.Error);

        _notes.Add(note.Value);

        RaiseDomainEvent(new AttendancePlanAcceptedDomainEvent(new(), Id));

        return Result.Success();
    }

    public Result SupersededBy(AttendancePlanId newPlanId)
    {
        Result<AttendancePlanNote> note = AttendancePlanNote.Create(Id, $"Updated Status from {Status} to {AttendancePlanStatus.Superseded}");

        if (note.IsFailure)
            return Result.Failure(note.Error);

        _notes.Add(note.Value);

        Status = AttendancePlanStatus.Superseded;

        note = AttendancePlanNote.Create(Id, $"Plan superseded by {newPlanId}");

        if (note.IsFailure)
            return Result.Failure(note.Error);

        _notes.Add(note.Value);

        return Result.Success();
    }

    public Result RejectPlan(string comment)
    {
        Result statusUpdate = UpdateStatus(AttendancePlanStatus.Rejected);

        if (statusUpdate.IsFailure)
            return statusUpdate;

        Result<AttendancePlanNote> note = AttendancePlanNote.Create(Id, comment);

        if (note.IsFailure)
            return Result.Failure(note.Error);

        _notes.Add(note.Value);

        RaiseDomainEvent(new AttendancePlanRejectedDomainEvent(new(), Id));

        return Result.Success();
    }

    public Result EditPlan(string comment)
    {
        Result statusUpdate = UpdateStatus(AttendancePlanStatus.Pending);

        if (statusUpdate.IsFailure)
            return statusUpdate;

        Result<AttendancePlanNote> note = AttendancePlanNote.Create(Id, comment);

        if (note.IsFailure)
            return Result.Failure(note.Error);

        _notes.Add(note.Value);

        return Result.Success();
    }

    private Dictionary<string, double> CalculatePercentages()
    {
        Dictionary<string, double> percentages = new();

        IEnumerable<IGrouping<CourseId, AttendancePlanPeriod>> periods = _periods.GroupBy(period => period.CourseId);

        foreach (IGrouping<CourseId, AttendancePlanPeriod> periodGroup in periods)
        {
            double target = periodGroup.First().TargetMinutesPerCycle;

            double total = periodGroup.Sum(period => period.MinutesPresent);

            double percentage = (total / target);

            percentages.Add(periodGroup.First().CourseName, percentage);
        }

        return percentages;
    }

    public void AddNote(AttendancePlanNote note) => _notes.Add(note);
}