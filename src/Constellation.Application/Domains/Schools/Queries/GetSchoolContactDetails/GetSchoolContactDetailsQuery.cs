namespace Constellation.Application.Domains.Schools.Queries.GetSchoolContactDetails;

using Abstractions.Messaging;

public sealed record GetSchoolContactDetailsQuery(
    string Code)
    : IQuery<SchoolContactDetailsResponse>;