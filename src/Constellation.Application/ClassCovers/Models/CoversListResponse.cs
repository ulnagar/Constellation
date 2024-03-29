﻿namespace Constellation.Application.ClassCovers.Models;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.ValueObjects;
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
