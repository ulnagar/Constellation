namespace Constellation.Core.Models.Tutorials;

using Core.Enums;
using Core.ValueObjects;
using Enums;
using Identifiers;
using Primitives;
using Students;
using Students.Identifiers;
using System;
using System.Collections.Generic;
using Timetables;

public sealed class Request : AggregateRoot, IAuditableEntity
{
    private readonly List<Period> _periods = [];

    private Request(
        StudentId studentId,
        Name student,
        Grade grade,
        string school,
        TutorialType type,
        string subject,
        List<Period> periods)
    {
        Id = new();

        StudentId = studentId;
        Student = student;
        Grade = grade;
        School = school;
        Type = type;
        Subject = subject;
        _periods = periods;
    }

    public RequestId Id { get; private set; }
    public StudentId StudentId { get; private set; }
    public Name Student { get; private set; }
    public Grade Grade { get; private set; }
    public string School { get; private set; }
    public TutorialType Type { get; private set; }
    public string Subject { get; private set; }
    public IReadOnlyList<Period> Periods => _periods.AsReadOnly();
    public TutorialStatus Status { get; private set; }
    
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }


    public static Request Create(
        Student student,
        TutorialType type,
        string subject,
        List<Period> periods)
    {
        return new Request(
            student.Id,
            student.Name,
            student.CurrentEnrolment?.Grade ?? Core.Enums.Grade.SpecialProgram,
            student.CurrentEnrolment?.SchoolName ?? string.Empty,
            type,
            subject,
            periods);
    }
}
