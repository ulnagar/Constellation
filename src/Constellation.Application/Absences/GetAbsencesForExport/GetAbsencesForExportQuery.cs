namespace Constellation.Application.Absences.GetAbsencesForExport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using System.Collections.Generic;

public sealed record GetAbsencesForExportQuery(
    AbsenceFilterDto Filter)
    : IQuery<List<AbsenceExportResponse>>;