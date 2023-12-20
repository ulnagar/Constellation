namespace Constellation.Application.Schools.GetSchoolById;

using Abstractions.Messaging;

public sealed record GetSchoolByIdQuery(
        string SchoolCode)
    : IQuery<SchoolResponse>;