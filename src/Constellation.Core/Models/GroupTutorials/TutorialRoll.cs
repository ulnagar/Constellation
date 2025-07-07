namespace Constellation.Core.Models.GroupTutorials;

using Constellation.Core.Models.Students.Identifiers;
using Enums;
using Identifiers;
using Primitives;
using StaffMembers.Identifiers;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class TutorialRoll : IAuditableEntity
{
    private readonly List<TutorialRollStudent> _students = new();

    // Private ctor needed to allow EFCore to create entity
    private TutorialRoll() { }

    public TutorialRoll(
        TutorialRollId id,
        GroupTutorial tutorial,
        DateOnly sessionDate)
    {
        Id = id;
        TutorialId = tutorial.Id;
        SessionDate = sessionDate;
    }

    public TutorialRollId Id { get; private set; }
    public GroupTutorialId TutorialId { get; private set; }
    public DateOnly SessionDate { get; private set; }
    public StaffId? StaffId { get; private set; }
    public TutorialRollStatus Status => GetStatus();
    public IReadOnlyCollection<TutorialRollStudent> Students => _students;
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public void AddStudent(StudentId studentId, bool enrolled)
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

    public void RemoveStudent(StudentId studentId)
    {
        var existingEntry = _students.FirstOrDefault(student => student.StudentId == studentId);

        if (existingEntry is not null)
        {
            _students.Remove(existingEntry);
        }
    }

    public void Submit(StaffId staffId, Dictionary<StudentId, bool> students)
    {
        StaffId = staffId;

        foreach (var entry in students)
        {
            var student = Students.FirstOrDefault(student => student.StudentId == entry.Key);

            if (student is null)
            {
                continue;
            }

            student.Present = entry.Value;
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

        if (StaffId.HasValue && StaffId.Value != StaffMembers.Identifiers.StaffId.Empty)
            return TutorialRollStatus.Submitted;

        return TutorialRollStatus.Unsubmitted;
    }
}
