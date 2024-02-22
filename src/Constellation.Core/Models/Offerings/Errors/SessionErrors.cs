namespace Constellation.Core.Models.Offerings.Errors;

using Identifiers;
using Shared;
using System;

public static class SessionErrors
{
    public static readonly Func<SessionId, Error> NotFound = id => new(
        "Offerings.Session.NotFound",
        $"Could not find Session with Id {id}");
}
