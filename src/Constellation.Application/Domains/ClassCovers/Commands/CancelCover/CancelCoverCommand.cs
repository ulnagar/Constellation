namespace Constellation.Application.Domains.ClassCovers.Commands.CancelCover;

using Abstractions.Messaging;
using Core.Models.Identifiers;

public sealed record CancelCoverCommand(
    ClassCoverId CoverId)
    : ICommand;