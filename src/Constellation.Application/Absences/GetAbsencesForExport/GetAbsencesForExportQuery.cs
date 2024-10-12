namespace Constellation.Application.Absences.GetAbsencesForExport;

using Abstractions.Messaging;
using DTOs;
using System.Collections.Generic;

public sealed record GetAbsencesForExportQuery(
    AbsenceFilterDto Filter)
    : IQuery<List<AbsenceExportResponse>>;