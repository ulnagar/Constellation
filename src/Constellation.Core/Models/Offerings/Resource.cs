namespace Constellation.Core.Models.Offerings;

using Identifiers;
using ValueObjects;
using Constellation.Core.Models.Subjects.Identifiers;
using System;

public abstract class Resource
{
    public ResourceId Id { get; protected set; }
    public ResourceType Type { get; protected set; }
    public OfferingId OfferingId { get; protected set; }
    public Offering Offering { get; protected set; }
    public string ResourceId { get; protected set; }
    public string Name { get; protected set; }
    public string Url { get; protected set; }
}

public sealed class AdobeConnectRoomResource : Resource
{
    private AdobeConnectRoomResource() { } // Required by EF Core

    internal AdobeConnectRoomResource(
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

    internal MicrosoftTeamResource(
        OfferingId offeringId,
        string teamName,
        string name,
        string url)
    {
        Id = new();
        Type = ResourceType.MicrosoftTeam;
        OfferingId = offeringId;
        ResourceId = teamName; // Should be name of the Team so that the ID can be searched in the database
        Name = name;
        Url = url;
    }

    public string TeamName => ResourceId;
}

public sealed class CanvasCourseResource : Resource
{
    private CanvasCourseResource() { } // Required by EF Core

    internal CanvasCourseResource(
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

        if (string.IsNullOrEmpty(url))
            Url = $"https://aurora.instructure.com/courses/sis_course_id:{ResourceId}";
        else
            Url = url;
    }

    public string CourseId => ResourceId;
}