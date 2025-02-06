namespace Constellation.Core.Errors;

using Constellation.Core.Shared;
using System;

public static class ApplicationErrors
{
    public static readonly Func<string, Error> ArgumentNull = argument => new(
        "Application.ArgumentNull",
        $"The argument {argument} is null");

    public static readonly Error SchoolInvalid = new(
        "Application.SchoolsPortal",
        "The selected school is invalid");

    public static readonly Error UnknownError = new(
        "Application.UnknownError",
        "An unknown error has occurred");
}