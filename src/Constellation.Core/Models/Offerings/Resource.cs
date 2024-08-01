namespace Constellation.Core.Models.Offerings;

using Canvas.Models;
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
        CanvasCourseCode canvasCourseId,
        CanvasSectionCode canvasSectionId,
        string name,
        string url)
    {
        Id = new();
        Type = ResourceType.CanvasCourse;
        OfferingId = offeringId;
        ResourceId = canvasCourseId.ToString();
        SectionId = canvasSectionId;
        Name = name;

        Url = string.IsNullOrEmpty(url) ? $"https://aurora.instructure.com/courses/sis_course_id:{ResourceId}" : url;
    }

    public CanvasCourseCode CourseId => CanvasCourseCode.FromValue(ResourceId);
    public CanvasSectionCode SectionId { get; private set; }
}