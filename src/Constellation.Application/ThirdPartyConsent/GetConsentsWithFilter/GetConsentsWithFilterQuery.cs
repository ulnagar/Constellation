namespace Constellation.Application.ThirdPartyConsent.GetConsentsWithFilter;

using Abstractions.Messaging;
using Core.Enums;
using Core.Models.Offerings.Identifiers;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetConsentsWithFilterQuery(
    List<OfferingId> OfferingIds,
    List<Grade> Grades,
    List<string> SchoolCodes,
    List<StudentId> StudentIds)
    : IQuery<List<ConsentSummaryResponse>>;
