namespace Constellation.Application.ClassCovers.UpdateCover;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Covers;
using Constellation.Core.Models.Identifiers;
using System;

public sealed record UpdateCoverCommand(
    ClassCoverId CoverId,
    DateOnly StartDate,
    DateOnly EndDate)
    : ICommand<ClassCover>;
