using Constellation.Core.Shared;
using System;

namespace Constellation.Core.Errors;

public static class ApplicationErrors
{
    public static readonly Func<string, Error> ArgumentNull = argument => new(
        "Application.ArgumentNull",
        $"The argument {argument} is null");

    public static readonly Error SchoolInvalid = new(
        "Application.SchoolsPortal",
        "The selected school is invalid");
}