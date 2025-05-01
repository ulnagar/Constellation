namespace Constellation.Application.Domains.ClassCovers.Commands.UpdateCover;

using Abstractions.Messaging;
using Core.Models.Covers;
using Core.Models.Identifiers;
using System;

public sealed record UpdateCoverCommand(
    ClassCoverId CoverId,
    DateOnly StartDate,
    DateOnly EndDate)
    : ICommand<ClassCover>;
