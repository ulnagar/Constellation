namespace Constellation.Application.Schools.GetSchoolForEdit;

using Abstractions.Messaging;

public sealed record GetSchoolForEditQuery(
    string SchoolCode)
    : IQuery<SchoolEditResponse>;