namespace Constellation.Core.Models.Operations;

using Enums;
using System;
using ValueObjects;

public abstract class TeamOperation
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; private set; } = DateTime.Now;
    public DateTime ScheduledFor { get; protected set; } = DateTime.Now;
    public bool IsCompleted { get; private set; }
    public bool IsDeleted { get; private set; }

    public void Complete() => IsCompleted = true;
    public void Delete() => IsDeleted = true;
}

public sealed class CreateTeamTeamOperation : TeamOperation
{
    private CreateTeamTeamOperation() { }

    public CreateTeamTeamOperation(
        string name,
        string description)
    {
        Name = name;
        Description = description;
    }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
}

public sealed class CreateTeamChannelTeamOperation : TeamOperation
{
    private CreateTeamChannelTeamOperation() { }

    public CreateTeamChannelTeamOperation(
        Guid teamId,
        string name,
        bool isPrivate)
    {
        TeamId = teamId;
        Name = name;
        IsPrivate = isPrivate;
    }

    public Guid TeamId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsPrivate { get; private set; }
}

public sealed class ModifyTeamMembershipTeamOperation : TeamOperation
{
    private ModifyTeamMembershipTeamOperation() { }

    public ModifyTeamMembershipTeamOperation(
        Guid teamId,
        EmailAddress userId,
        TeamAction action)
    {
        TeamId = teamId;
        UserId = userId;
        Action = action;
    }

    public Guid TeamId { get; private set; }
    public EmailAddress UserId { get; private set; } 
    public TeamAction Action { get; private set; }
}

public sealed class ModifyTeamChannelMembershipTeamOperation : TeamOperation
{
    private ModifyTeamChannelMembershipTeamOperation() { }

    public ModifyTeamChannelMembershipTeamOperation(
        Guid teamId,
        string channelName,
        EmailAddress userId,
        TeamAction action)
    {
        TeamId = teamId;
        ChannelName = channelName;
        UserId = userId;
        Action = action;
    }

    public Guid TeamId { get; private set; }
    public string ChannelName { get; private set; }
    public EmailAddress UserId { get; private set; }
    public TeamAction Action { get; private set; }
}

public sealed class ArchiveTeamTeamOperation : TeamOperation
{
    private ArchiveTeamTeamOperation() { }

    public ArchiveTeamTeamOperation(
        Guid teamId)
    {
        TeamId = teamId;
    }

    public Guid TeamId { get; private set; }
}

public sealed class ArchiveTeamChannelTeamOperation : TeamOperation
{
    private ArchiveTeamChannelTeamOperation() { }

    public ArchiveTeamChannelTeamOperation(
        Guid teamId,
        string channelName)
    {
        TeamId = teamId;
        ChannelName = channelName;
    }

    public Guid TeamId { get; private set; }
    public string ChannelName { get; private set; } = string.Empty;
}
