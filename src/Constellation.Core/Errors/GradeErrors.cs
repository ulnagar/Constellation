namespace Constellation.Core.Errors;

using Shared;

public static class GradeErrors
{
    public static readonly Func<int, Error> NotFound = number => new(
        "ValueObjects.Grade",
        $"Could not find a Grade with the number {number}");
}