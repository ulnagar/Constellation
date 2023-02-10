namespace Constellation.Application.ClassCovers.CreateCover;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Covers;
using Constellation.Core.ValueObjects;
using System;

public sealed record CreateCoverCommand(
    Guid Id,
    int OfferingId,
    DateOnly StartDate,
    DateOnly EndDate,
    CoverTeacherType TeacherType,
    string TeacherId)
    : ICommand<ClassCover>;