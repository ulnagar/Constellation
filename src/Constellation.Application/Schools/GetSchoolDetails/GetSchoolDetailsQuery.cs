namespace Constellation.Application.Schools.GetSchoolDetails;

using Abstractions.Messaging;

public sealed record GetSchoolDetailsQuery(
    string SchoolCode)
    : IQuery<SchoolDetailsResponse>;
