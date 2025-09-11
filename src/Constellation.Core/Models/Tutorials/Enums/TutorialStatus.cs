namespace Constellation.Core.Models.Tutorials.Enums;

using Common;
using System.Collections.Generic;

public sealed class TutorialStatus : StringEnumeration<TutorialStatus>
{
    public static readonly TutorialStatus Requested = new("Requested", "Requested");
    public static readonly TutorialStatus Active = new("Active", "Active");
    public static readonly TutorialStatus Rejected = new("Rejected", "Rejected");
    public static readonly TutorialStatus Expired = new("Expired", "Expired");
    
    private TutorialStatus(string value, string name)
        : base(value, name) { }

    public static IEnumerable<TutorialStatus> GetOptions => GetEnumerable;
}