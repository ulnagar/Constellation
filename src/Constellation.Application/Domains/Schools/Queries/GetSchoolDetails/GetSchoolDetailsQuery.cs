namespace Constellation.Application.Domains.Schools.Queries.GetSchoolDetails;

using Abstractions.Messaging;
using Core.Models.Identifiers;

public sealed record GetSchoolDetailsQuery(
    SchoolCode SchoolCode)
    : IQuery<SchoolDetailsResponse>;
