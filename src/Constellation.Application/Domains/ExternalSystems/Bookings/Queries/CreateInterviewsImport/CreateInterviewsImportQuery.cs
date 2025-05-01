namespace Constellation.Application.Domains.ExternalSystems.Bookings.Queries.CreateInterviewsImport;

using Abstractions.Messaging;
using Core.Models.Offerings.Identifiers;
using DTOs;
using System.Collections.Generic;

public sealed record CreateInterviewsImportQuery(
    List<int> Grades,
    List<OfferingId> ClassList,
    bool PerFamily,
    bool ResidentialFamilyOnly)
    : IQuery<List<InterviewExportDto>>;