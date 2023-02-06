namespace Constellation.Application.ClassCovers.CreateCover;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Covers;
using System;

public sealed record CreateCoverCommand(
    Guid Id,
    int OfferingId,
    DateOnly StartDate,
    DateOnly EndDate,
    string TeacherType,
    string TeacherId)
    : ICommand<ClassCover>;