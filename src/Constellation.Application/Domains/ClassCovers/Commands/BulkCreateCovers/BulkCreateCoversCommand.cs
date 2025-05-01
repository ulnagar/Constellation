namespace Constellation.Application.Domains.ClassCovers.Commands.BulkCreateCovers;

using Abstractions.Messaging;
using Core.Models.Covers;
using Core.Models.Offerings.Identifiers;
using Core.ValueObjects;
using System;
using System.Collections.Generic;

public sealed record BulkCreateCoversCommand(
    Guid Id,
    List<OfferingId> OfferingId,
    DateOnly StartDate,
    DateOnly EndDate,
    CoverTeacherType TeacherType,
    string TeacherId)
    : ICommand<List<ClassCover>>;