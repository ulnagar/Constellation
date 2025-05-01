namespace Constellation.Application.Domains.ClassCovers.Models;

using Core.Models.Identifiers;
using Core.ValueObjects;
using System;

public sealed record CoversListResponse(
    ClassCoverId Id,
    string OfferingName,
    string TeacherId,
    string TeacherName,
    string TeacherSchool,
    CoverTeacherType TeacherType,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsCurrent,
    bool IsFuture);
