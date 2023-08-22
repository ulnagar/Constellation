namespace Constellation.Core.Models.Subjects;

using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Models.Subjects.ValueObjects;
using System;

public abstract class Resource
{
    public ResourceId Id { get; protected set; }
    public ResourceType Type { get; protected set; }
    public OfferingId OfferingId { get; protected set; }
    public virtual Offering Offering { get; protected set; }
    public string ResourceId { get; protected set; }
    public string Name { get; protected set; }
    public string Url { get; protected set; }
}

public sealed class AdobeConnectRoomResource : Resource
{
    private AdobeConnectRoomResource() { } // Required by EF Core

    public AdobeConnectRoomResource(
        OfferingId offeringId,
        string scoId,
        string name,
        string url)
    {
        Id = new();
        Type = ResourceType.AdobeConnectRoom;
        OfferingId = offeringId;
        ResourceId = scoId;
        Name = name;
        Url = url;
    }

    public string ScoId => ResourceId;
}

public sealed class MicrosoftTeamResource : Resource
{
    private MicrosoftTeamResource() { } // Required by EF Core

    public MicrosoftTeamResource(
        OfferingId offeringId,
        Guid teamGroupId,
        string name,
        string url)
    {
        Id = new();
        Type = ResourceType.MicrosoftTeam;
        OfferingId = offeringId;
        ResourceId = teamGroupId.ToString();
        Name = name;
        Url = url;
    }

    public Guid GroupId => new Guid(ResourceId);
}

public sealed class CanvasCourseResource : Resource
{
    private CanvasCourseResource() { } // Required by EF Core

    public CanvasCourseResource(
        OfferingId offeringId,
        string canvasCourseId,
        string name,
        string url)
    {
        Id = new();
        Type = ResourceType.CanvasCourse;
        OfferingId = offeringId;
        ResourceId = canvasCourseId;
        Name = name;
        Url = url;
    }

    public string CourseId => ResourceId;
}