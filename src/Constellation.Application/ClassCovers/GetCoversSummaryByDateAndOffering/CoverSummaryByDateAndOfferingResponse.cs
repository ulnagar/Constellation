namespace Constellation.Application.ClassCovers.GetCoversSummaryByDateAndOffering;

using System;

public sealed record CoverSummaryByDateAndOfferingResponse(
    DateTime CreatedAt,
    string TeacherName,
    string CoverType);