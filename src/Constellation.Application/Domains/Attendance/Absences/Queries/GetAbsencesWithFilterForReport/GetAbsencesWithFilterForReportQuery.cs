namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsencesWithFilterForReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings.Identifiers;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetAbsencesWithFilterForReportQuery(
    List<OfferingId> OfferingCodes,
    List<Grade> Grades,
    List<string> SchoolCodes,
    List<StudentId> StudentIds)
    : IQuery<List<FilteredAbsenceResponse>>;
