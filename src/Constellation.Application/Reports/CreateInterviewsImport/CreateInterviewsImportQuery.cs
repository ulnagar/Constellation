namespace Constellation.Application.Reports.CreateInterviewsImport;

using Abstractions.Messaging;
using Constellation.Application.DTOs;
using Core.Models.Offerings.Identifiers;
using System.Collections.Generic;

public sealed record CreateInterviewsImportQuery(
    List<int> Grades,
    List<OfferingId> ClassList,
    bool PerFamily,
    bool ResidentialFamilyOnly)
    : IQuery<List<InterviewExportDto>>;