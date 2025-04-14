namespace Constellation.Core.Models.Operations;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Operations.Enums;
using Core.Enums;
using Identifiers;
using System;

public abstract record TeamsOperation
{
    public TeamsOperationId Id { get; init; }
    public TeamsAction Action { get; init; }
    public TeamsPermission Permission { get; init; }
    public DateTime DateScheduled { get; init; }
    public bool IsCompleted { get; private set; }
    public bool IsDeleted { get; private set; }

    public void Complete()
    {
        IsCompleted = true;
    }

    public void Delete()
    {
        IsDeleted = true;
    }
}

public abstract record CoverTeamsOperation : TeamsOperation
{
    public ClassCoverId CoverId { get; init; }
}

public sealed record CasualCoverTeamsOperation : CoverTeamsOperation
{
    public CasualId CasualId { get; init; }
    public string Name { get; init; }
}

public sealed record TeacherCoverTeamsOperation : CoverTeamsOperation
{
    public string StaffId { get; init; }
    public string Name { get; init; }
}