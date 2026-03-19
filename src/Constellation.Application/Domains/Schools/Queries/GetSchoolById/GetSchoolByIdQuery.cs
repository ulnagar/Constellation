namespace Constellation.Application.Domains.Schools.Queries.GetSchoolById;

using Abstractions.Messaging;
using Core.Models.Identifiers;

public sealed record GetSchoolByIdQuery(
    SchoolCode SchoolCode)
    : IQuery<SchoolResponse>;