namespace Constellation.Core.Models.Subjects.Errors;

using Constellation.Core.Shared;
using System;

public static class SessionErrors
{
    public static readonly Func<int, Error> NotFound = id => new(
        "Subjects.Session.NotFound",
        $"Could not find Session with Id {id}");
}