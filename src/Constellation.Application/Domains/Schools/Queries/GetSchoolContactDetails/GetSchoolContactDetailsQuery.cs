namespace Constellation.Application.Domains.Schools.Queries.GetSchoolContactDetails;

using Abstractions.Messaging;
using Core.Models.Identifiers;

public sealed record GetSchoolContactDetailsQuery(
    SchoolCode Code)
    : IQuery<SchoolContactDetailsResponse>;