namespace Constellation.Application.Domains.Schools.Queries.GetSchoolForEdit;

using Abstractions.Messaging;

public sealed record GetSchoolForEditQuery(
    string SchoolCode)
    : IQuery<SchoolEditResponse>;