namespace Constellation.Core.Models.Offerings.ValueObjects;

using Constellation.Core.Enums;
using Constellation.Core.Errors;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Primitives;
using Constellation.Core.Shared;
using System;
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

    // For legacy values
    private OfferingName(
        string grade,
        string course,
        char sequence)
    {
        Grade = grade;
        Course = course;
        Sequence = sequence;
    }

    public static Result<OfferingName> Create(
        Course course,
        string line,
        char sequence)
    {
        var gradeAsString = ((int)course.Grade).ToString().PadLeft(2, '0');

        if (gradeAsString.Length != 2 || gradeAsString == "00")
        {
            return Result.Failure<OfferingName>(DomainErrors.ValueObjects.OfferingName.InvalidGrade(course.Grade));
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
        Course course,
        string initials)
    {
        var gradeAsString = ((int)course.Grade).ToString().PadLeft(2, '0');

        if (gradeAsString.Length != 2 || gradeAsString == "00")
        {
            return Result.Failure<OfferingName>(DomainErrors.ValueObjects.OfferingName.InvalidGrade(course.Grade));
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

    public static OfferingName FromValue(string value)
    {
        if (value is null ||
            string.IsNullOrWhiteSpace(value))
            return null;

        var grade = value[..2];
        var courseCode = value[2..5];

        if (courseCode.ToUpper() == "TUT")
        {
            var initials = value[5..];

            return new OfferingName(
                grade,
                courseCode,
                initials);
        }
        else
        {
            char sequence = Convert.ToChar(value[6..]);

            if (value.Length == 7) 
            {
                var line = value[5..6];

                return new OfferingName(
                    grade,
                    courseCode,
                    line,
                    sequence);
            }

            return new OfferingName(
                grade,
                courseCode,
                sequence);
        }
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
        if (string.IsNullOrWhiteSpace(Line) && string.IsNullOrWhiteSpace(Initials))
        {
            // Detected legacy value

            return $"{Grade}{Course}{Sequence}";
        }

        if (!string.IsNullOrWhiteSpace(Initials))
            return $"{Grade}{Course}{Initials}";

        if (!string.IsNullOrWhiteSpace(Line))
            return $"{Grade}{Course}{Line}{Sequence}";

        return string.Empty;
    }

    public static implicit operator string(OfferingName offeringName) =>
        offeringName is null ? string.Empty : offeringName.ToString();
}
