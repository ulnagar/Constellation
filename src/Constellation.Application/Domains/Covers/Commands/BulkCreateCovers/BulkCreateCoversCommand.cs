namespace Constellation.Application.Domains.Covers.Commands.BulkCreateCovers;

using Abstractions.Messaging;
using Core.Models.Covers;
using Core.Models.Covers.Enums;
using Core.Models.Offerings.Identifiers;
using System;
using System.Collections.Generic;

public sealed record BulkCreateCoversCommand(
    List<OfferingId> OfferingId,
    DateOnly StartDate,
    DateOnly EndDate,
    CoverTeacherType TeacherType,
    string TeacherId,
    CoverType CoverType,
    string Note = null)
    : ICommand<List<Cover>>;