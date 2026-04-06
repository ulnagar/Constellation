namespace Constellation.Application.Domains.Schools.Queries.GetSchoolForEdit;

using Abstractions.Messaging;
using Core.Models.Identifiers;

public sealed record GetSchoolForEditQuery(
    SchoolCode SchoolCode)
    : IQuery<SchoolEditResponse>;