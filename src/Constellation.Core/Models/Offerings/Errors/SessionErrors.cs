namespace Constellation.Core.Models.Offerings.Errors;

using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Shared;
using System;

public static class SessionErrors
{
    public static readonly Func<SessionId, Error> NotFound = id => new(
        "Offerings.Session.NotFound",
        $"Could not find Session with Id {id}");
}
