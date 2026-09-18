namespace Constellation.Core.Models.Absences.Errors;

using Identifiers;
using Shared;

public static class AbsenceNotificationErrors
{
    public static readonly Func<AbsenceNotificationId, Error> NotFound = id => new(
        "Absences.Notification.NotFound",
        $"Could not find any absence notification with the Id {id}");
}