namespace Constellation.Application.Domains.Schools.Queries.GetSchoolById;

using Abstractions.Messaging;

public sealed record GetSchoolByIdQuery(
        string SchoolCode)
    : IQuery<SchoolResponse>;