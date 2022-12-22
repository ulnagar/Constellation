﻿namespace Constellation.Core.Models.GroupTutorials;

using Constellation.Core.Enums;
using Constellation.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class TutorialRoll : Entity, IAuditableEntity
{
    private readonly List<TutorialRollStudent> _students = new();

    private TutorialRoll() { }

    public TutorialRoll(
        Guid Id,
        GroupTutorial tutorial,
        DateOnly sessionDate)
        : base(Id)
    {
        TutorialId = tutorial.Id;
        SessionDate = sessionDate;
    }

    public Guid TutorialId { get; set; }
    public DateOnly SessionDate { get; set; }
    public string StaffId { get; set; }
    public TutorialRollStatus Status => GetStatus();
    public IReadOnlyCollection<TutorialRollStudent> Students => _students;
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public void AddStudent(string studentId, bool enrolled)
    {
        var existingEntry = _students.FirstOrDefault(student => student.StudentId == studentId);

        if (existingEntry is not null)
        {
            if (enrolled && !existingEntry.Enrolled)
                existingEntry.Enrolled = true;
        } 
        else
        {
            var student = new TutorialRollStudent
            {
                StudentId = studentId,
                Enrolled = enrolled
            };

            _students.Add(student);
        }
    }

    public void RemoveStudent(string studentId)
    {
        var existingEntry = _students.FirstOrDefault(student => student.StudentId == studentId);

        if (existingEntry is not null)
        {
            _students.Remove(existingEntry);
        }
    }

    public void Cancel()
    {
        IsDeleted = true;
    }

    private TutorialRollStatus GetStatus()
    {
        if (IsDeleted)
            return TutorialRollStatus.Cancelled;

        if (!string.IsNullOrWhiteSpace(StaffId))
            return TutorialRollStatus.Submitted;

        return TutorialRollStatus.Unsubmitted;
    }
}
