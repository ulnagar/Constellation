namespace Constellation.Core.Models.Awards.Enums;

using Constellation.Core.Common;
using System.Collections.Generic;

public sealed class AwardNotificationType : StringEnumeration<AwardNotificationType>
{
    public static readonly AwardNotificationType Parent = new("Parent");
    public static readonly AwardNotificationType PartnerSchool = new("Partner School");

    private AwardNotificationType(string value)
        : base(value, value) { }

    public static IEnumerable<AwardNotificationType> GetOptions => GetEnumerable;
}