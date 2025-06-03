namespace Constellation.Core.Errors;

using Constellation.Core.Enums;
using Shared;
using System;

public static class SystemLinkErrors
{
    public static readonly Error EmptyValue = new(
        "SystemLink.Empty",
        "A System Link must have a value");

    public static readonly Func<SystemType, Error> NotFound = type => new(
        "SystemLink.NotFound",
        $"Could not find a SystemLink of type {type}");
}