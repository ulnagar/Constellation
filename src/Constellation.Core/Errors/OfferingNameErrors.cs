namespace Constellation.Core.Errors;

using Enums;
using Shared;

public static class OfferingNameErrors
{
    public static readonly Func<Grade, Error> InvalidGrade = grade => new(
        "OfferingName.InvalidGrade",
        $"Invalid grade supplied: {grade}");

    public static readonly Func<string, Error> InvalidCourseCode = code => new(
        "OfferingName.InvalidCourseCode",
        $"Invalid course code supplied: {code}");

    public static readonly Func<string, Error> InvalidTutorialInitials = initials => new(
        "OfferingName.InvalidTutorialInitials",
        $"Invalid initals supplied for tutorial class: {initials}");
}