namespace Constellation.Core.Models.Students.Errors;

using Shared;

public static class SystemLinkErrors
{
    public static readonly Error EmptyValue = new(
        "Student.SystemLink.Empty",
        "A System Link must have a value");
}