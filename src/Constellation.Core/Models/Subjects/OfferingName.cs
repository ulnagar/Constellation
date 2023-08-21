namespace Constellation.Core.Models.Subjects;

using Constellation.Core.Enums;
using Constellation.Core.Errors;
using Constellation.Core.Primitives;
using Constellation.Core.Shared;
using System.Collections.Generic;

public sealed class OfferingName : ValueObject
{
    private OfferingName(
        string grade,
        string course,
        string line,
        char sequence)
    {
        Grade = grade;
        Course = course;
        Line = line;
        Sequence = sequence;
    }

    private OfferingName(
    string grade,
    string course,
    string initials)
    {
        Grade = grade;
        Course = course;
        Initials = initials;
    }

    public static Result<OfferingName> Create(
        Grade grade,
        Course course,
        string line,
        char sequence)
    {
        var gradeAsString = ((int)grade).ToString().PadLeft(2, '0');

        if (gradeAsString.Length != 2 || gradeAsString == "00") 
        {
            return Result.Failure<OfferingName>(DomainErrors.ValueObjects.OfferingName.InvalidGrade(grade));
        }

        var courseCode = course?.Code?.ToUpper();

        if (string.IsNullOrWhiteSpace(courseCode))
        {
            return Result.Failure<OfferingName>(DomainErrors.ValueObjects.OfferingName.InvalidCourseCode(courseCode));
        }

        return new OfferingName(
            gradeAsString,
            courseCode,
            line,
            sequence);
    }

    public static Result<OfferingName> Create(
        Grade grade,
        Course course,
        string initials)
    {
        var gradeAsString = ((int)grade).ToString().PadLeft(2, '0');

        if (gradeAsString.Length != 2 || gradeAsString == "00")
        {
            return Result.Failure<OfferingName>(DomainErrors.ValueObjects.OfferingName.InvalidGrade(grade));
        }

        var courseCode = course?.Code?.ToUpper();

        if (string.IsNullOrWhiteSpace(courseCode))
        {
            return Result.Failure<OfferingName>(DomainErrors.ValueObjects.OfferingName.InvalidCourseCode(courseCode));
        }

        if (initials.Length != 2)
        {
            return Result.Failure<OfferingName>(DomainErrors.ValueObjects.OfferingName.InvalidTutorialInitials(initials));
        }

        return new OfferingName(
            gradeAsString,
            courseCode,
            initials);
    }

    public string Grade { get; }
    public string Course { get; }
    public string Line { get; }
    public char Sequence { get; }
    public string Initials { get; }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Grade;
        yield return Course;
        yield return Line;
        yield return Sequence;
        yield return Initials;
    }

    public override string ToString()
    {
        if (!string.IsNullOrWhiteSpace(Initials))
            return $"{Grade}{Course}{Initials}";

        if (!string.IsNullOrWhiteSpace(Line))
            return $"{Grade}{Course}{Line}{Sequence}";

        return string.Empty;
    }

    public static implicit operator string(OfferingName offeringName) =>
        offeringName is null ? string.Empty : offeringName.ToString();
}
