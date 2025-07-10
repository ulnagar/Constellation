namespace Constellation.Application.Domains.Covers.Commands.CreateCover;

using Abstractions.Messaging;
using Core.Models.Covers;
using Core.Models.Covers.Enums;
using Core.Models.Offerings.Identifiers;
using System;

public sealed record CreateCoverCommand(
    OfferingId OfferingId,
    DateOnly StartDate,
    DateOnly EndDate,
    CoverTeacherType TeacherType,
    string TeacherId,
    CoverType CoverType,
    string Note = null)
    : ICommand<Cover>;