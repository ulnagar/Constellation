namespace Constellation.Application.Domains.Covers.Models;

using Constellation.Core.Models.Covers.Identifiers;
using Core.Models.Covers.Enums;
using System;

public sealed record CoversListResponse(
    CoverId Id,
    string OfferingName,
    string TeacherId,
    string TeacherName,
    string TeacherSchool,
    CoverTeacherType TeacherType,
    DateOnly StartDate,
    DateOnly EndDate,
    CoverType CoverType,
    bool IsCurrent,
    bool IsFuture);
