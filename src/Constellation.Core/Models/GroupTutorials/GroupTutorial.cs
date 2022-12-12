namespace Constellation.Core.Models.GroupTutorials;

using Constellation.Core.Errors;
using Constellation.Core.Primitives;
using Constellation.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class GroupTutorial : AggregateRoot, IAuditableEntity
{
    private readonly List<Staff> _teachers = new();
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
    public IReadOnlyCollection<Staff> Teachers => _teachers;
    public IReadOnlyCollection<TutorialEnrolment> Enrolments => _enrolments;
    public IReadOnlyCollection<TutorialRoll> Rolls => _rolls;

    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public Result AddTeacher(Staff teacher)
    {
        bool hasEndedOrBeenDeleted =
            EndDate > DateTime.Today ||
            IsDeleted == false;

        if (hasEndedOrBeenDeleted)
        {
            return Result.Failure(DomainErrors.GroupTutorials.TutorialHasExpired);
        }

        RaiseDomainEvent(new TeacherAddedToGroupTutorialDomainEvent(Guid.NewGuid(), Id, teacher.StaffId));

        _teachers.Add(teacher);

        return Result.Success();
    }

    public Result RemoveTeacher(Staff teacher)
    {
        if (_teachers.Contains(teacher))
        {
            _teachers.Remove(teacher);
        }

        return Result.Success();
    }

    public Result<TutorialEnrolment> EnrolStudent(Student student, DateTime? effectiveTo = null)
    {
        if (_enrolments.Any(enrol => enrol.StudentId == student.StudentId && !enrol.IsDeleted))
        {
            return Result.Failure<TutorialEnrolment>(DomainErrors.GroupTutorials.StudentAlreadyEnrolled);
        }

        var enrolment = new TutorialEnrolment(Guid.NewGuid(), student, effectiveTo);

        _enrolments.Add(enrolment);

        return enrolment;
    }

    public Result UnenrolStudent(Student student, DateTime? takesEffectOn = null)
    {
        if (_enrolments.Where(enrol => enrol.StudentId == student.StudentId).Any(enrol => !enrol.IsDeleted))
        {
            return Result.Success();
        }

        var enrolments = _enrolments.Where(enrol => enrol.StudentId == student.StudentId && !enrol.IsDeleted).ToList();

        foreach (var enrolment in enrolments)
        {
            if (takesEffectOn.HasValue)
            {
                enrolment.EffectiveTo = takesEffectOn.Value;
            } else
            {
                enrolment.IsDeleted = true;
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