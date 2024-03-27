namespace Constellation.Application.Absences.GetAbsencesWithFilterForReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings.Identifiers;
using System.Collections.Generic;

public sealed record GetAbsencesWithFilterForReportQuery(
    List<OfferingId> OfferingCodes,
    List<Grade> Grades,
    List<string> SchoolCodes,
    List<string> StudentIds)
    : IQuery<List<FilteredAbsenceResponse>>;
