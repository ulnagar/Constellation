namespace Constellation.Application.Domains.Offerings.Queries.GetCurrentOfferingsForStudent;

using Core.Models.Offerings.Identifiers;
using Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public sealed record OfferingDetailResponse(
    string OfferingName,
    string CourseName,
    List<OfferingDetailResponse.Teacher> Teachers,
    List<OfferingDetailResponse.Resource> Resources)
{
    public sealed record Teacher(
        Name Name,
        EmailAddress EmailAddress);

    public sealed record Resource(
        string Type,
        string Name,
        string Url);
};