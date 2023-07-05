namespace Constellation.Application.Absences.GetAbsencesWithFilterForReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Enums;
using System;
using System.Collections.Generic;

public sealed record GetAbsencesWithFilterForReportQuery(
    List<int> OfferingCodes,
    List<Grade> Grades,
    List<string> SchoolCodes,
    List<string> StudentIds)
    : IQuery<List<FilteredAbsenceResponse>>;
