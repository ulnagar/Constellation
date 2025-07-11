namespace Constellation.Application.Domains.Covers.Queries.GetClassCoversSummaryByDateAndOffering;

using Core.Models.Covers.Enums;
using System;

public sealed record ClassCoverSummaryByDateAndOfferingResponse(
    DateTime CreatedAt,
    string TeacherName,
    CoverTeacherType CoverType);