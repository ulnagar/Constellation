namespace Constellation.Core.Models.Subjects;

using Constellation.Core.Models.Faculties.Identifiers;
using Enums;
using Errors;
using Identifiers;
using Offerings;
using Primitives;
using Shared;
using System.Collections.Generic;

public sealed class Course : AggregateRoot
{
    private readonly List<Offering> _offerings = new();

    private Course() { }

    private Course(
        string name,
        string code,
        Grade grade,
        FacultyId facultyId,
        decimal fteValue,
        decimal targetMinutesPerCycle)
    {
        Id = new();
        Name = name;
        Code = code;
        Grade = grade;
        FacultyId = facultyId;
        FullTimeEquivalentValue = fteValue;
        TargetMinutesPerCycle = targetMinutesPerCycle;
    }

    public CourseId Id { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }
    public Grade Grade { get; private set; }
    public FacultyId FacultyId { get; private set; }
    public decimal FullTimeEquivalentValue { get; private set; }
    public double TargetMinutesPerCycle { get; private set; }
    public IReadOnlyList<Offering> Offerings => _offerings;

    public static Result<Course> Create(
        string name,
        string code,
        Grade grade,
        FacultyId facultyId,
        decimal fteValue,
        decimal targetMinutesPerCycle)
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
            fteValue,
            targetMinutesPerCycle);
    }

    public Result Update(
        string name,
        string code,
        Grade grade,
        FacultyId facultyId,
        decimal fteValue,
        decimal targetMinutesPerCycle)
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
        TargetMinutesPerCycle = targetMinutesPerCycle;

        return Result.Success();
    }

    public override string ToString() => $"{Grade} {Name}";
}