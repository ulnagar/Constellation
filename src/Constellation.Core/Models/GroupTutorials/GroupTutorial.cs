namespace Constellation.Core.Models.GroupTutorials;

using Constellation.Core.DomainEvents;
using Constellation.Core.Errors;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Primitives;
using Constellation.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class GroupTutorial : AggregateRoot, IAuditableEntity
{
    private readonly List<TutorialTeacher> _teachers = new();
    private readonly List<TutorialEnrolment> _enrolments = new();
    private readonly List<TutorialRoll> _rolls = new();

    private GroupTutorial(
        GroupTutorialId id,
        string name,
        DateOnly startDate,
        DateOnly endDate)
    {
        Id = id;
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
    }

    public GroupTutorialId Id { get; private set; }
    public string Name { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public IReadOnlyCollection<TutorialTeacher> Teachers => _teachers;
    public IReadOnlyCollection<TutorialEnrolment> Enrolments => _enrolments;
    public IReadOnlyCollection<TutorialEnrolment> CurrentEnrolments =>
        GetActiveEnrolmentsForDate(DateOnly.FromDateTime(DateTime.Today));
    public IReadOnlyCollection<TutorialRoll> Rolls => _rolls;

    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public static GroupTutorial Create(GroupTutorialId id, string name, DateOnly startDate, DateOnly endDate)
    {
        var tutorial = new GroupTutorial(id, name, startDate, endDate);

        tutorial.RaiseDomainEvent(new GroupTutorialCreatedDomainEvent(new DomainEventId(), tutorial.Id));

        return tutorial;
    }

    public void Edit(string name, DateOnly startDate, DateOnly endDate)
    {
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
    }

    public List<TutorialEnrolment> GetActiveEnrolmentsForDate(DateOnly date) =>
        _enrolments
            .Where(member =>
                !member.IsDeleted &&
                member.EffectiveFrom <= date &&
                (!member.EffectiveTo.HasValue || member.EffectiveTo.Value >= date))
            .ToList();

    public Result<TutorialTeacher> AddTeacher(Staff teacher, DateOnly? effectiveTo = null)
    {
        bool hasEndedOrBeenDeleted =
            EndDate < DateOnly.FromDateTime(DateTime.Today) ||
            IsDeleted;

        if (hasEndedOrBeenDeleted)
        {
            return Result.Failure<TutorialTeacher>(DomainErrors.GroupTutorials.GroupTutorial.TutorialHasExpired);
        }

        if (_teachers.Any(enrol => enrol.StaffId == teacher.StaffId && !enrol.IsDeleted))
        {
            var existingEntry = _teachers.FirstOrDefault(enrol => enrol.StaffId == teacher.StaffId && !enrol.IsDeleted);

            return existingEntry;
        }

        var entry = new TutorialTeacher(new TutorialTeacherId(), teacher.StaffId, effectiveTo);

        RaiseDomainEvent(new TeacherAddedToGroupTutorialDomainEvent(new DomainEventId(), Id, entry.Id));

        _teachers.Add(entry);

        return entry;
    }

    public Result RemoveTeacher(Staff teacher, DateOnly? takesEffectOn = null)
    {
        if (_teachers.Where(enrol => enrol.StaffId == teacher.StaffId).All(enrol => enrol.IsDeleted))
        {
            return Result.Success();
        }

        var tutorialTeachers = _teachers.Where(enrol => enrol.StaffId == teacher.StaffId && !enrol.IsDeleted).ToList();

        foreach (var tutorialTeacher in tutorialTeachers)
        {
            if (takesEffectOn.HasValue && takesEffectOn.Value > DateOnly.FromDateTime(DateTime.Today))
            {
                tutorialTeacher.EffectiveTo = takesEffectOn.Value;
            }
            else
            {
                tutorialTeacher.IsDeleted = true;

                RaiseDomainEvent(new TeacherRemovedFromGroupTutorialDomainEvent(new DomainEventId(), Id, tutorialTeacher.Id));
            }
        }

        return Result.Success();
    }

    public Result<TutorialEnrolment> EnrolStudent(Student student, DateOnly? effectiveTo = null)
    {
        bool hasEndedOrBeenDeleted =
            EndDate < DateOnly.FromDateTime(DateTime.Today) ||
            IsDeleted;

        if (hasEndedOrBeenDeleted)
        {
            return Result.Failure<TutorialEnrolment>(DomainErrors.GroupTutorials.GroupTutorial.TutorialHasExpired);
        }

        if (_enrolments.Any(enrol => enrol.StudentId == student.StudentId && !enrol.IsDeleted))
        {
            var existingEntry = _enrolments.FirstOrDefault(enrol => enrol.StudentId == student.StudentId && !enrol.IsDeleted);

            return existingEntry;
        }

        var enrolment = new TutorialEnrolment(new TutorialEnrolmentId(), student, effectiveTo);

        RaiseDomainEvent(new StudentAddedToGroupTutorialDomainEvent(new DomainEventId(), Id, enrolment.Id));

        _enrolments.Add(enrolment);

        return enrolment;
    }

    public Result UnenrolStudent(Student student, DateOnly? takesEffectOn = null)
    {
        if (_enrolments.Where(enrol => enrol.StudentId == student.StudentId).All(enrol => enrol.IsDeleted))
        {
            return Result.Success();
        }

        var enrolments = _enrolments.Where(enrol => enrol.StudentId == student.StudentId && !enrol.IsDeleted).ToList();

        foreach (var enrolment in enrolments)
        {
            if (takesEffectOn.HasValue && takesEffectOn.Value > DateOnly.FromDateTime(DateTime.Today))
            {
                enrolment.EffectiveTo = takesEffectOn.Value;
            } 
            else
            {
                enrolment.IsDeleted = true;

                RaiseDomainEvent(new StudentRemovedFromGroupTutorialDomainEvent(new DomainEventId(), Id, enrolment.Id));
            }
        }

        return Result.Success();
    }

    public Result<TutorialRoll> CreateRoll(DateOnly rollDate)
    {
        if (Rolls.Any(roll => roll.SessionDate == rollDate))
        {
            return Result.Failure<TutorialRoll>(DomainErrors.GroupTutorials.TutorialRoll.RollAlreadyExistsForDate(rollDate));
        }

        if (rollDate < StartDate || rollDate > EndDate)
        {
            return Result.Failure<TutorialRoll>(DomainErrors.GroupTutorials.TutorialRoll.RollDateInvalid(rollDate));
        }

        var roll = new TutorialRoll(new TutorialRollId(), this, rollDate);

        var students = GetActiveEnrolmentsForDate(rollDate).Select(member => member.StudentId);

        foreach (var studentId in students)
        {
            roll.AddStudent(studentId, true);
        }

        _rolls.Add(roll);

        return roll;
    }

    public Result SubmitRoll(TutorialRoll roll, Staff staffMember, Dictionary<string, bool> studentPresence)
    {
        if (roll.Status != Enums.TutorialRollStatus.Unsubmitted)
        {
            return Result.Failure(DomainErrors.GroupTutorials.TutorialRoll.SubmitInvalidStatus);
        }

        roll.Submit(staffMember.StaffId, studentPresence);

        RaiseDomainEvent(new TutorialRollSubmittedDomainEvent(new DomainEventId(), Id, roll.Id));

        return Result.Success();
    }

    public void Delete()
    {
        IsDeleted = true;
    }
}