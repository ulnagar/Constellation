namespace Constellation.Core.Models.Subjects;

using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Subjects.Errors;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Primitives;
using Constellation.Core.Shared;
using System;
using System.Collections.Generic;
using System.Threading;

public sealed class Course : AggregateRoot
{
    private readonly List<Offering> _offerings = new();

    private Course() { }

    private Course(
        string name,
        string code,
        Grade grade,
        Guid facultyId,
        decimal fteValue)
    {
        Id = new();
        Name = name;
        Code = code;
        Grade = grade;
        FacultyId = facultyId;
        FullTimeEquivalentValue = fteValue;
    }

    public CourseId Id { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }
    public Grade Grade { get; private set; }
    public Guid FacultyId { get; private set; }
    public decimal FullTimeEquivalentValue { get; private set; }
    public IReadOnlyList<Offering> Offerings => _offerings;

    public static Result<Course> Create(
        string name,
        string code,
        Grade grade,
        Guid facultyId,
        decimal fteValue)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<Course>(CourseErrors.CodeEmpty);

        if (code.Length != 3)
            return Result.Failure<Course>(CourseErrors.CodeLengthInvalid);

        return new Course(
            name,
            code,
            grade,
            facultyId,
            fteValue);
    }

    public Result Update(
        string name,
        string code,
        Grade grade,
        Guid facultyId,
        decimal fteValue)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<Course>(CourseErrors.CodeEmpty);

        if (code.Length != 3)
            return Result.Failure<Course>(CourseErrors.CodeLengthInvalid);

        Name = name;
        Code = code;
        Grade = grade;
        FacultyId = facultyId;
        FullTimeEquivalentValue = fteValue;

        return Result.Success();
    }
}