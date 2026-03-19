namespace Constellation.Application.Domains.ThirdPartyConsent.Queries.GetConsentsWithFilter;

using Abstractions.Messaging;
using Core.Enums;
using Core.Models.Identifiers;
using Core.Models.Offerings.Identifiers;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetConsentsWithFilterQuery(
    List<OfferingId> OfferingIds,
    List<Grade> Grades,
    List<SchoolCode> SchoolCodes,
    List<StudentId> StudentIds)
    : IQuery<List<ConsentSummaryResponse>>;
