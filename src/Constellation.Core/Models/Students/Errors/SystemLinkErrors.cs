namespace Constellation.Core.Models.Students.Errors;

using Enums;
using Shared;
using System;

public static class SystemLinkErrors
{
    public static readonly Error EmptyValue = new(
        "Student.SystemLink.Empty",
        "A System Link must have a value");

    public static readonly Func<SystemType, Error> NotFound = type => new(
        "Student.SystemLink.NotFound",
        $"Could not find a SystemLink of type {type}");
}