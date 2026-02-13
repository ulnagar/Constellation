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

    public static readonly Error ExportServiceFailed = new (
        "Service.Export.DocumentServiceFailed", 
        "Document Service failed to create document");

    public static readonly Func<string, Error> InvalidConfiguration = config => new(
        "Application.InvalidConfiguration",
        $"The configuration required for {config} is missing or invalid");
}