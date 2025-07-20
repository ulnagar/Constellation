namespace Constellation.Application.Domains.Offerings.Queries.GetCurrentOfferingsAndTutorialsForStudent;

using Constellation.Core.ValueObjects;
using System.Collections.Generic;

public sealed record DetailResponse(
    string OfferingName,
    string CourseName,
    List<DetailResponse.Teacher> Teachers,
    List<DetailResponse.Resource> Resources)
{
    public sealed record Teacher(
        Name Name,
        EmailAddress EmailAddress);

    public sealed record Resource(
        string Type,
        string Name,
        string Url);
};