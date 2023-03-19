namespace Constellation.Application.ClassCovers.CancelCover;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record CancelCoverCommand(
    ClassCoverId CoverId)
    : ICommand;