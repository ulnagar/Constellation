namespace Constellation.Core.Models.Absences;

using Constellation.Core.Common;

public class NotificationType : StringEnumeration<NotificationType>
{
    public static NotificationType Email = new("Email");
    public static NotificationType SMS = new("SMS");

    public NotificationType(string value)
        : base(value, value) { }
}
