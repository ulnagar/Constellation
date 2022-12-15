namespace Constellation.Core.Models.GroupTutorials;

using Constellation.Core.DomainEvents;
using Constellation.Core.Errors;
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

    public GroupTutorial(
        Guid id,
        string name,
        DateTime startDate,
        DateTime endDate)
        : base(id)
    {
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
    }

    public string Name { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public IReadOnlyCollection<TutorialTeacher> Teachers => _teachers;
    public IReadOnlyCollection<TutorialEnrolment> Enrolments => _enrolments;
    public IReadOnlyCollection<TutorialRoll> Rolls => _rolls;

    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public Result<TutorialTeacher> AddTeacher(Staff teacher, DateTime? effectiveTo = null)
    {
        bool hasEndedOrBeenDeleted =
            EndDate < DateTime.Today ||
            IsDeleted;

        if (hasEndedOrBeenDeleted)
        {
            return Result.Failure<TutorialTeacher>(DomainErrors.GroupTutorials.TutorialHasExpired);
        }

        if (_teachers.Any(enrol => enrol.StaffId == teacher.StaffId && !enrol.IsDeleted))
        {
            var existingEntry = _teachers.FirstOrDefault(enrol => enrol.StaffId == teacher.StaffId && !enrol.IsDeleted);

            return existingEntry;
        }

        var entry = new TutorialTeacher(Guid.NewGuid(), teacher, effectiveTo);

        RaiseDomainEvent(new TeacherAddedToGroupTutorialDomainEvent(Guid.NewGuid(), Id, entry.Id));

        _teachers.Add(entry);

        return entry;
    }

    public Result RemoveTeacher(Staff teacher, DateTime? takesEffectOn = null)
    {
        if (_teachers.Where(enrol => enrol.StaffId == teacher.StaffId).All(enrol => enrol.IsDeleted))
        {
            return Result.Success();
        }

        var tutorialTeachers = _teachers.Where(enrol => enrol.StaffId == teacher.StaffId && !enrol.IsDeleted).ToList();

        foreach (var tutorialTeacher in tutorialTeachers)
        {
            if (takesEffectOn.HasValue && takesEffectOn.Value > DateTime.Today)
            {
                tutorialTeacher.EffectiveTo = takesEffectOn.Value;
            }
            else
            {
                tutorialTeacher.IsDeleted = true;

                RaiseDomainEvent(new TeacherRemovedFromGroupTutorialDomainEvent(Guid.NewGuid(), Id, tutorialTeacher.Id));
            }
        }

        return Result.Success();
    }

    public Result<TutorialEnrolment> EnrolStudent(Student student, DateTime? effectiveTo = null)
    {
        bool hasEndedOrBeenDeleted =
            EndDate < DateTime.Today ||
            IsDeleted;

        if (hasEndedOrBeenDeleted)
        {
            return Result.Failure<TutorialEnrolment>(DomainErrors.GroupTutorials.TutorialHasExpired);
        }

        if (_enrolments.Any(enrol => enrol.StudentId == student.StudentId && !enrol.IsDeleted))
        {
            var existingEntry = _enrolments.FirstOrDefault(enrol => enrol.StudentId == student.StudentId && !enrol.IsDeleted);

            return existingEntry;
        }

        var enrolment = new TutorialEnrolment(Guid.NewGuid(), student, effectiveTo);

        RaiseDomainEvent(new StudentAddedToGroupTutorialDomainEvent(Guid.NewGuid(), Id, enrolment.Id));

        _enrolments.Add(enrolment);

        return enrolment;
    }

    public Result UnenrolStudent(Student student, DateTime? takesEffectOn = null)
    {
        if (_enrolments.Where(enrol => enrol.StudentId == student.StudentId).All(enrol => enrol.IsDeleted))
        {
            return Result.Success();
        }

        var enrolments = _enrolments.Where(enrol => enrol.StudentId == student.StudentId && !enrol.IsDeleted).ToList();

        foreach (var enrolment in enrolments)
        {
            if (takesEffectOn.HasValue && takesEffectOn.Value > DateTime.Today)
            {
                enrolment.EffectiveTo = takesEffectOn.Value;
            } 
            else
            {
                enrolment.IsDeleted = true;

                RaiseDomainEvent(new StudentRemovedFromGroupTutorialDomainEvent(Guid.NewGuid(), Id, enrolment.Id));
            }
        }

        return Result.Success();
    }

    public Result<TutorialRoll> CreateRoll(DateTime rollDate)
    {
        if (_rolls.Any(roll => roll.SessionDate == rollDate))
        {
            return Result.Failure<TutorialRoll>(DomainErrors.GroupTutorials.RollAlreadyExistsForDate(rollDate));
        }

        var roll = new TutorialRoll(Guid.NewGuid(), rollDate);

        var students = _enrolments
            .Where(enrol => !enrol.IsDeleted || enrol.EffectiveTo < rollDate)
            .Select(enrol => enrol.StudentId)
            .ToList();

        foreach (var student in students)
        {
            roll.AddStudent(student);
        }

        return roll;
    }
}