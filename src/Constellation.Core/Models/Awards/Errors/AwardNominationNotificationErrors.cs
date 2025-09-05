namespace Constellation.Core.Models.Awards.Errors;

using Identifiers;
using Shared;
using System;

public static class AwardNominationNotificationErrors
{
    public static readonly Func<NominationNotificationId, Error> NotFound = id => new(
        "Awards.Nomination.Notification.NotFound",
        $"Could not find a notification with the id {id}");
}