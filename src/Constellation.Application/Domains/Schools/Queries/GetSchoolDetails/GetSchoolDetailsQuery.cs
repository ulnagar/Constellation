namespace Constellation.Application.Domains.Schools.Queries.GetSchoolDetails;

using Abstractions.Messaging;

public sealed record GetSchoolDetailsQuery(
    string SchoolCode)
    : IQuery<SchoolDetailsResponse>;
