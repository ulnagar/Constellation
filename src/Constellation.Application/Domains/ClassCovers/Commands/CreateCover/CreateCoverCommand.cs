namespace Constellation.Application.Domains.ClassCovers.Commands.CreateCover;

using Abstractions.Messaging;
using Core.Models.Covers;
using Core.Models.Offerings.Identifiers;
using Core.ValueObjects;
using System;

public sealed record CreateCoverCommand(
    Guid Id,
    OfferingId OfferingId,
    DateOnly StartDate,
    DateOnly EndDate,
    CoverTeacherType TeacherType,
    string TeacherId)
    : ICommand<ClassCover>;