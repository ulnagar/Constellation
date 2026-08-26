namespace Constellation.Application.Domains.Auth.Models;

using Core.Models.Auth.Enums;

public sealed class NotificationSetting
{
    public NotificationType Type { get; set; }
    public bool Enabled { get; set; }
}
