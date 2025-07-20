namespace Constellation.Core.Models.Tutorials;

using Identifiers;
using System;

public sealed class TeamsResource
{
    private TeamsResource() { } // Required by EF Core

    internal TeamsResource(
        Guid teamId,
        string name,
        string url)
    {
        Id = new();

        TeamId = teamId;
        Name = name;
        Url = url;
    }

    public TeamsResourceId Id { get; private set; }
    public TutorialId TutorialId { get; private set; }
    public Guid TeamId { get; private set; }
    public string Name { get; private set; }
    public string Url { get; private set; }
}