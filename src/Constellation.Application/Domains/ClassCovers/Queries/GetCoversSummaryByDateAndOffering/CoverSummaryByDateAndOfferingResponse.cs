namespace Constellation.Application.Domains.ClassCovers.Queries.GetCoversSummaryByDateAndOffering;

using System;

public sealed record CoverSummaryByDateAndOfferingResponse(
    DateTime CreatedAt,
    string TeacherName,
    string CoverType);