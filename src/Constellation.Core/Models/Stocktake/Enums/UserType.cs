namespace Constellation.Core.Models.Stocktake.Enums;

using Constellation.Core.Common;
using System.Collections.Generic;

public sealed class UserType : StringEnumeration<UserType>
{
    public static readonly UserType Student = new("Student");
    public static readonly UserType Staff = new("Staff Member");
    public static readonly UserType School = new("Partner School");
    public static readonly UserType CommunityMember = new("Community Member");
    public static readonly UserType Other = new("Other");

    private UserType(string value)
        : base(value, value) { }

    public static IEnumerable<UserType> GetOptions => GetEnumerable;
}