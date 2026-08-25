namespace Constellation.Core.Models.Auth;

using Enums;

public sealed class AppUserNotificationPreference
{
    public required Guid AppUserId { get; init; }
    public required NotificationType NotificationType { get; init; }
}