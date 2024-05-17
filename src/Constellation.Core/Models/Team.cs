namespace Constellation.Core.Models;

using DomainEvents;
using Identifiers;
using Primitives;
using System;
using System.Web;

public class Team : AggregateRoot
{
    private Team(
        Guid id,
        string name,
        string description)
    {
        Id = id;
        UpdateTeamDetails(name, description);
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Link { get; private set; } = string.Empty;
    public bool IsArchived { get; private set; }

    public static Team Create(Guid id, string name, string description, string generalChannelId)
    {
        Team team = new(id, name, description);

        team.CreateLinkFromPart(generalChannelId);

        team.RaiseDomainEvent(new MicrosoftTeamRegisteredDomainEvent(new DomainEventId(Guid.NewGuid()), team.Id));

        return team;
    }

    private void CreateLinkFromPart(string generalChannelId)
    {
        // Link format is https://teams.microsoft.com/l/team/{General Channel Id}/conversations?groupId={Group Id}&tenantId={Tenant Id}

        Link = $"https://teams.microsoft.com/l/team/{HttpUtility.UrlEncode(generalChannelId)}/conversations?groupId={HttpUtility.UrlEncode(Id.ToString())}&tenantId={HttpUtility.UrlEncode("05a0e69a-418a-47c1-9c25-9387261bf991")}";
    }

    public void UpdateTeamDetails(string name, string description)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            Name = name.Trim();
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            Description = description.Trim();
        }
    }

    public void ArchiveTeam()
    {
        IsArchived = true;

        RaiseDomainEvent(new MicrosoftTeamArchivedDomainEvent(new DomainEventId(Guid.NewGuid()), Id));
    }
}
