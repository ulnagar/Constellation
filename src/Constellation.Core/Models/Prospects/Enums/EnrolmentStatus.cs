namespace Constellation.Core.Models.Prospects.Enums;

using Common;
using System.Collections.Generic;

public sealed class EnrolmentStatus : StringEnumeration<EnrolmentStatus>
{
    public static readonly EnrolmentStatus Accepted = new("Accepted");
    public static readonly EnrolmentStatus Lapsed = new("Lapsed");
    public static readonly EnrolmentStatus Declined = new("Declined");
    public static readonly EnrolmentStatus NotApplicable = new("Not Applicable");
    public static readonly EnrolmentStatus Unknown = new("Unknown");

    private EnrolmentStatus(string value)
        : base(value, value) { }

    public static IEnumerable<EnrolmentStatus> GetOptions => GetEnumerable;
}