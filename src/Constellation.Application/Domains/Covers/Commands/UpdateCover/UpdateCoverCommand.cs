namespace Constellation.Application.Domains.Covers.Commands.UpdateCover;

using Abstractions.Messaging;
using Constellation.Core.Models.Covers.Identifiers;
using Core.Models.Covers;
using System;

public sealed record UpdateCoverCommand(
    CoverId CoverId,
    DateOnly StartDate,
    DateOnly EndDate)
    : ICommand<Cover>;
