namespace Constellation.Application.Schools.GetSchoolContactDetails;

using Abstractions.Messaging;

public sealed record GetSchoolContactDetailsQuery(
    string Code)
    : IQuery<SchoolContactDetailsResponse>;