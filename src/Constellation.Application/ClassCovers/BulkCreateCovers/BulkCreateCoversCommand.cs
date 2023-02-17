namespace Constellation.Application.ClassCovers.BulkCreateCovers;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Covers;
using Constellation.Core.ValueObjects;
using System;
using System.Collections.Generic;

public sealed record BulkCreateCoversCommand(
    Guid Id,
    List<int> OfferingId,
    DateOnly StartDate,
    DateOnly EndDate,
    CoverTeacherType TeacherType,
    string TeacherId)
    : ICommand<List<ClassCover>>;