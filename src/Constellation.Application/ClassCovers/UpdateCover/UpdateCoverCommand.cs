namespace Constellation.Application.ClassCovers.UpdateCover;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Covers;
using System;

public sealed record UpdateCoverCommand(
    Guid CoverId,
    DateOnly StartDate,
    DateOnly EndDate)
    : ICommand<ClassCover>;
