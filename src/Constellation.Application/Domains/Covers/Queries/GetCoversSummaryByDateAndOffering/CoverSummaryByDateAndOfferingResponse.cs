namespace Constellation.Application.Domains.Covers.Queries.GetCoversSummaryByDateAndOffering;

using Core.Models.Covers.Enums;
using System;

public sealed record CoverSummaryByDateAndOfferingResponse(
    DateTime CreatedAt,
    string TeacherName,
    CoverType CoverType,
    CoverTeacherType TeacherType);