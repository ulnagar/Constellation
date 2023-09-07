namespace Constellation.Application.Offerings.Models;

using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings.Identifiers;
using System;
using System.Collections.Generic;

public sealed record OfferingSummaryResponse(
    OfferingId Id,
    string Name,
    string CourseName,
    DateOnly StartDate,
    DateOnly EndDate,
    List<string> Teachers,
    int MinPerFN,
    string Faculty,
    Grade Grade,
    bool IsActive);