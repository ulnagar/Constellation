namespace Constellation.Core.Models.Absences;

using Constellation.Core.Common;

public class NotificationType : StringEnumeration<NotificationType>
{
    public static readonly NotificationType Email = new("Email");
    public static readonly NotificationType SMS = new("SMS");

    public NotificationType(string value)
        : base(value, value) { }
}
