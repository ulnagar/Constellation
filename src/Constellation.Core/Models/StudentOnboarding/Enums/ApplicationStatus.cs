namespace Constellation.Core.Models.StudentOnboarding.Enums;

using Core.Common;
using System.Collections.Generic;

public sealed class ApplicationStatus : StringEnumeration<ApplicationStatus>
{
    public static readonly ApplicationStatus Pending = new("Pending");
    public static readonly ApplicationStatus Accepted = new("Accepted");
    public static readonly ApplicationStatus Declined = new("Declined");
    public static readonly ApplicationStatus Lapsed = new("Lapsed");

    public ApplicationStatus(string value)
        : base(value, value) { }

    public static IEnumerable<ApplicationStatus> GetOptions => GetEnumerable;
}