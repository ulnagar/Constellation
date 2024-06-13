using Constellation.Core.Common;
using System.Collections;
using System.Collections.Generic;

namespace Constellation.Core.Models.Assets.Enums;

public sealed class AllocationType : StringEnumeration<AllocationType>
{
    public static readonly AllocationType Student = new("Student");
    public static readonly AllocationType Staff = new("Staff");
    public static readonly AllocationType School = new("School");
    public static readonly AllocationType CommunityMember = new("Community Member");

    private AllocationType(string value)
        : base(value, value) { }

    public static IEnumerable<AllocationType> GetOptions => GetEnumerable;
}