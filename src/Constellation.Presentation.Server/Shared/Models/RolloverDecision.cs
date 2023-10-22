namespace Constellation.Presentation.Server.Shared.Models;

using Constellation.Core.Enums;

public class RolloverDecision
{
    public string StudentId { get; set; }
    public string StudentName { get; set; }
    public Grade Grade { get; set; }
    public string SchoolName { get; set; }
    public Status Decision { get; set; }

    public enum Status
    {
        Unknown,
        Withdraw,
        Rollover,
        Remain
    }
}