namespace Constellation.Core.Models.Auth.Enums;

using Common;

public sealed class NotificationType : StringEnumeration<NotificationType>
{
    public static readonly NotificationType AwardsDigest = new("AWARDS_DIGEST", "Awards Digest: A weekly email with the list of awards recently issued to students");

    private NotificationType(string value, string name)
        : base(value, name) { }
}